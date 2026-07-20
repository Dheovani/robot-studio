using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class RobotSimulator
{
    private readonly MotionPlanner motionPlanner;

    public RobotSimulator()
        : this(new MotionPlanner())
    {
    }

    public RobotSimulator(MotionPlanner motionPlanner)
    {
        ArgumentNullException.ThrowIfNull(motionPlanner);

        this.motionPlanner = motionPlanner;
    }

    public SimulationResult Execute(
        SimulationContext initialContext,
        RobotCommandSequence commandSequence)
    {
        ArgumentNullException.ThrowIfNull(initialContext);
        ArgumentNullException.ThrowIfNull(commandSequence);

        var currentContext = initialContext;
        var timeline = new List<SimulationStep>
        {
            CreateStep(currentContext, "Simulation started.")
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

                return new SimulationResult(
                    initialContext,
                    currentContext,
                    timeline.AsReadOnly(),
                    exception);
            }
        }

        return new SimulationResult(
            initialContext,
            currentContext,
            timeline.AsReadOnly(),
            Failure: null);
    }

    private SimulationContext ExecuteCommand(
        SimulationContext context,
        RobotCommand command,
        int commandIndex,
        List<SimulationStep> timeline)
    {
        RobotCommandValidator.Validate(command, context.RobotProfile);

        return command switch
        {
            HomeCommand homeCommand => ExecuteHome(context, homeCommand, commandIndex, timeline),
            MoveToCommand moveToCommand => ExecuteMove(context, moveToCommand, commandIndex, timeline),
            WaitCommand waitCommand => ExecuteWait(context, waitCommand, commandIndex, timeline),
            _ => throw new InvalidOperationException($"Unsupported robot command type: {command.GetType().Name}.")
        };
    }

    private SimulationContext ExecuteHome(
        SimulationContext context,
        HomeCommand command,
        int commandIndex,
        List<SimulationStep> timeline)
    {
        var targetPosition = new CartesianPosition(X: 0, Y: 0, Z: 0);
        var movingContext = TransitionTo(context, RobotState.Homing);
        timeline.Add(CreateStep(movingContext, "Home command started.", commandIndex, nameof(HomeCommand), command.Source));

        var motionPlan = motionPlanner.PlanLinearMove(
            context.CurrentPosition,
            targetPosition,
            context.RobotProfile);

        var completedContext = movingContext with
        {
            CurrentPosition = targetPosition,
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Home command completed.", commandIndex, nameof(HomeCommand), command.Source));

        return completedContext;
    }

    private SimulationContext ExecuteMove(
        SimulationContext context,
        MoveToCommand command,
        int commandIndex,
        List<SimulationStep> timeline)
    {
        var movingContext = TransitionTo(context, RobotState.Moving);
        timeline.Add(CreateStep(movingContext, "Move command started.", commandIndex, nameof(MoveToCommand), command.Source));

        var motionPlan = motionPlanner.PlanLinearMove(
            context.CurrentPosition,
            command.TargetPosition,
            context.RobotProfile,
            command.RequestedVelocityMillimetersPerSecond);

        var completedContext = movingContext with
        {
            CurrentPosition = command.TargetPosition,
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Move command completed.", commandIndex, nameof(MoveToCommand), command.Source));

        return completedContext;
    }

    private static SimulationContext ExecuteWait(
        SimulationContext context,
        WaitCommand command,
        int commandIndex,
        List<SimulationStep> timeline)
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

    private static SimulationContext TransitionTo(
        SimulationContext context,
        RobotState nextState)
    {
        RobotStateTransitions.EnsureCanTransitionTo(context.State, nextState);

        return context with { State = nextState };
    }

    private static SimulationStep CreateStep(
        SimulationContext context,
        string description,
        int? commandIndex = null,
        string? commandName = null,
        RobotCommandSource? commandSource = null) =>
        new(
            context.ElapsedTime,
            context.State,
            context.CurrentPosition,
            description,
            commandIndex,
            commandName,
            commandSource);

    private static string GetCommandName(RobotCommand command) => command switch
    {
        HomeCommand => nameof(HomeCommand),
        MoveToCommand => nameof(MoveToCommand),
        WaitCommand => nameof(WaitCommand),
        _ => command.GetType().Name
    };
}
