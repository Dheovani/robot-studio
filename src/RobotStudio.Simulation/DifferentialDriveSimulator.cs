using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class DifferentialDriveSimulator
{
    private readonly DifferentialDriveMotionPlanner motionPlanner;

    public DifferentialDriveSimulator()
        : this(new DifferentialDriveMotionPlanner())
    {
    }

    public DifferentialDriveSimulator(DifferentialDriveMotionPlanner motionPlanner)
    {
        ArgumentNullException.ThrowIfNull(motionPlanner);

        this.motionPlanner = motionPlanner;
    }

    public DifferentialDriveSimulationResult Execute(
        DifferentialDriveSimulationContext initialContext,
        RobotCommandSequence commandSequence)
    {
        ArgumentNullException.ThrowIfNull(initialContext);
        ArgumentNullException.ThrowIfNull(commandSequence);

        var currentContext = initialContext;
        var timeline = new List<DifferentialDriveSimulationStep>
        {
            CreateStep(currentContext, "Differential drive simulation started.")
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

                return new DifferentialDriveSimulationResult(
                    initialContext,
                    currentContext,
                    timeline.AsReadOnly(),
                    exception);
            }
        }

        return new DifferentialDriveSimulationResult(
            initialContext,
            currentContext,
            timeline.AsReadOnly(),
            Failure: null);
    }

    private DifferentialDriveSimulationContext ExecuteCommand(
        DifferentialDriveSimulationContext context,
        RobotCommand command,
        int commandIndex,
        List<DifferentialDriveSimulationStep> timeline)
    {
        RobotCommandValidator.Validate(command, context.RobotProfile);

        return command switch
        {
            HomeCommand homeCommand => ExecuteHome(context, homeCommand, commandIndex, timeline),
            DifferentialDriveMoveCommand moveCommand => ExecuteMove(context, moveCommand, commandIndex, timeline),
            WaitCommand waitCommand => ExecuteWait(context, waitCommand, commandIndex, timeline),
            _ => throw new InvalidOperationException($"Unsupported robot command type: {command.GetType().Name}.")
        };
    }

    private DifferentialDriveSimulationContext ExecuteHome(
        DifferentialDriveSimulationContext context,
        HomeCommand command,
        int commandIndex,
        List<DifferentialDriveSimulationStep> timeline)
    {
        var targetPose = new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0);
        var homingContext = TransitionTo(context, RobotState.Homing);
        timeline.Add(CreateStep(homingContext, "Home command started.", commandIndex, nameof(HomeCommand), command.Source));

        var motionPlan = motionPlanner.PlanMove(
            context.CurrentPose,
            targetPose,
            context.RobotProfile);

        var completedContext = homingContext with
        {
            CurrentPose = targetPose,
            State = RobotState.Completed,
            ElapsedTime = homingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Home command completed.", commandIndex, nameof(HomeCommand), command.Source));

        return completedContext;
    }

    private DifferentialDriveSimulationContext ExecuteMove(
        DifferentialDriveSimulationContext context,
        DifferentialDriveMoveCommand command,
        int commandIndex,
        List<DifferentialDriveSimulationStep> timeline)
    {
        var movingContext = TransitionTo(context, RobotState.Moving);
        timeline.Add(CreateStep(movingContext, "Differential drive move started.", commandIndex, nameof(DifferentialDriveMoveCommand), command.Source));

        var motionPlan = motionPlanner.PlanMove(
            context.CurrentPose,
            command.TargetPose,
            context.RobotProfile,
            command.RequestedLinearVelocityMillimetersPerSecond,
            command.RequestedAngularVelocityDegreesPerSecond);

        var completedContext = movingContext with
        {
            CurrentPose = command.TargetPose,
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Differential drive move completed.", commandIndex, nameof(DifferentialDriveMoveCommand), command.Source));

        return completedContext;
    }

    private static DifferentialDriveSimulationContext ExecuteWait(
        DifferentialDriveSimulationContext context,
        WaitCommand command,
        int commandIndex,
        List<DifferentialDriveSimulationStep> timeline)
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

    private static DifferentialDriveSimulationContext TransitionTo(
        DifferentialDriveSimulationContext context,
        RobotState nextState)
    {
        RobotStateTransitions.EnsureCanTransitionTo(context.State, nextState);

        return context with { State = nextState };
    }

    private static DifferentialDriveSimulationStep CreateStep(
        DifferentialDriveSimulationContext context,
        string description,
        int? commandIndex = null,
        string? commandName = null,
        RobotCommandSource? commandSource = null) =>
        new(
            context.ElapsedTime,
            context.State,
            context.CurrentPose,
            description,
            commandIndex,
            commandName,
            commandSource);

    private static string GetCommandName(RobotCommand command) => command switch
    {
        HomeCommand => nameof(HomeCommand),
        DifferentialDriveMoveCommand => nameof(DifferentialDriveMoveCommand),
        WaitCommand => nameof(WaitCommand),
        _ => command.GetType().Name
    };
}
