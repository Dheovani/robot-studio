using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Parallel;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class DeltaSimulator
{
    private readonly DeltaMotionPlanner motionPlanner;
    private readonly DeltaKinematics kinematics;

    public DeltaSimulator()
        : this(new DeltaMotionPlanner(), new DeltaKinematics())
    {
    }

    public DeltaSimulator(
        DeltaMotionPlanner motionPlanner,
        DeltaKinematics kinematics)
    {
        ArgumentNullException.ThrowIfNull(motionPlanner);
        ArgumentNullException.ThrowIfNull(kinematics);

        this.motionPlanner = motionPlanner;
        this.kinematics = kinematics;
    }

    public DeltaSimulationResult Execute(
        DeltaSimulationContext initialContext,
        RobotCommandSequence commandSequence)
    {
        ArgumentNullException.ThrowIfNull(initialContext);
        ArgumentNullException.ThrowIfNull(commandSequence);

        var currentContext = initialContext;
        var timeline = new List<DeltaSimulationStep>
        {
            CreateStep(currentContext, "Delta simulation started.")
        };

        for (var commandIndex = 0; commandIndex < commandSequence.Commands.Count; commandIndex++)
        {
            var command = commandSequence.Commands[commandIndex];

            try
            {
                currentContext = ExecuteCommand(currentContext, command, commandIndex, timeline);
            }
            catch (InvalidOperationException exception)
            {
                currentContext = currentContext with { State = RobotState.Faulted };
                timeline.Add(CreateStep(currentContext, exception.Message, commandIndex, GetCommandName(command), command.Source));

                return new DeltaSimulationResult(
                    initialContext,
                    currentContext,
                    timeline.AsReadOnly(),
                    exception);
            }
        }

        return new DeltaSimulationResult(
            initialContext,
            currentContext,
            timeline.AsReadOnly(),
            Failure: null);
    }

    private DeltaSimulationContext ExecuteCommand(
        DeltaSimulationContext context,
        RobotCommand command,
        int commandIndex,
        List<DeltaSimulationStep> timeline)
    {
        RobotCommandValidator.Validate(command, context.RobotProfile);

        return command switch
        {
            HomeCommand homeCommand => ExecuteHome(context, homeCommand, commandIndex, timeline),
            DeltaMoveActuatorsCommand moveCommand => ExecuteMove(context, moveCommand, commandIndex, timeline),
            WaitCommand waitCommand => ExecuteWait(context, waitCommand, commandIndex, timeline),
            _ => throw new InvalidOperationException($"Unsupported robot command type: {command.GetType().Name}.")
        };
    }

    private DeltaSimulationContext ExecuteHome(
        DeltaSimulationContext context,
        HomeCommand command,
        int commandIndex,
        List<DeltaSimulationStep> timeline)
    {
        var targetActuators = new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 0, CMillimeters: 0);
        var homingContext = TransitionTo(context, RobotState.Homing);
        timeline.Add(CreateStep(homingContext, "Home command started.", commandIndex, nameof(HomeCommand), command.Source));

        var motionPlan = motionPlanner.PlanMove(
            context.CurrentActuators,
            targetActuators,
            context.RobotProfile);

        var completedContext = homingContext with
        {
            CurrentActuators = targetActuators,
            State = RobotState.Completed,
            ElapsedTime = homingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Home command completed.", commandIndex, nameof(HomeCommand), command.Source));

        return completedContext;
    }

    private DeltaSimulationContext ExecuteMove(
        DeltaSimulationContext context,
        DeltaMoveActuatorsCommand command,
        int commandIndex,
        List<DeltaSimulationStep> timeline)
    {
        var movingContext = TransitionTo(context, RobotState.Moving);
        timeline.Add(CreateStep(movingContext, "Delta actuator move started.", commandIndex, nameof(DeltaMoveActuatorsCommand), command.Source));

        var motionPlan = motionPlanner.PlanMove(
            context.CurrentActuators,
            command.TargetActuators,
            context.RobotProfile,
            command.RequestedActuatorVelocityMillimetersPerSecond);

        var completedContext = movingContext with
        {
            CurrentActuators = command.TargetActuators,
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Delta actuator move completed.", commandIndex, nameof(DeltaMoveActuatorsCommand), command.Source));

        return completedContext;
    }

    private DeltaSimulationContext ExecuteWait(
        DeltaSimulationContext context,
        WaitCommand command,
        int commandIndex,
        List<DeltaSimulationStep> timeline)
    {
        var waitingContext = TransitionTo(context, RobotState.Waiting);
        timeline.Add(CreateStep(waitingContext, "Wait command started.", commandIndex, nameof(WaitCommand), command.Source));

        var completedContext = waitingContext with
        {
            State = RobotState.Completed,
            ElapsedTime = waitingContext.ElapsedTime + command.Duration
        };

        timeline.Add(CreateStep(completedContext, "Wait command completed.", commandIndex, nameof(WaitCommand), command.Source));

        return completedContext;
    }

    private static DeltaSimulationContext TransitionTo(
        DeltaSimulationContext context,
        RobotState nextState)
    {
        RobotStateTransitions.EnsureCanTransitionTo(context.State, nextState);

        return context with { State = nextState };
    }

    private DeltaSimulationStep CreateStep(
        DeltaSimulationContext context,
        string description,
        int? commandIndex = null,
        string? commandName = null,
        RobotCommandSource? commandSource = null) =>
        new(
            context.ElapsedTime,
            context.State,
            context.CurrentActuators,
            kinematics.Forward(context.RobotProfile, context.CurrentActuators),
            description,
            commandIndex,
            commandName,
            commandSource);

    private static string GetCommandName(RobotCommand command) => command switch
    {
        HomeCommand => nameof(HomeCommand),
        DeltaMoveActuatorsCommand => nameof(DeltaMoveActuatorsCommand),
        WaitCommand => nameof(WaitCommand),
        _ => command.GetType().Name
    };
}
