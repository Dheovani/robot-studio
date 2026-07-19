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

        foreach (var command in commandSequence.Commands)
        {
            try
            {
                currentContext = ExecuteCommand(currentContext, command, timeline);
            }
            catch (InvalidOperationException exception)
            {
                currentContext = currentContext with { State = RobotState.Faulted };
                timeline.Add(CreateStep(currentContext, exception.Message));

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
        List<SimulationStep> timeline)
    {
        RobotCommandValidator.Validate(command, context.RobotProfile);

        return command switch
        {
            HomeCommand => ExecuteHome(context, timeline),
            MoveToCommand moveToCommand => ExecuteMove(context, moveToCommand, timeline),
            WaitCommand waitCommand => ExecuteWait(context, waitCommand, timeline),
            _ => throw new InvalidOperationException($"Unsupported robot command type: {command.GetType().Name}.")
        };
    }

    private SimulationContext ExecuteHome(
        SimulationContext context,
        List<SimulationStep> timeline)
    {
        var targetPosition = new CartesianPosition(X: 0, Y: 0, Z: 0);
        var movingContext = TransitionTo(context, RobotState.Homing);
        timeline.Add(CreateStep(movingContext, "Home command started."));

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

        timeline.Add(CreateStep(completedContext, "Home command completed."));

        return completedContext;
    }

    private SimulationContext ExecuteMove(
        SimulationContext context,
        MoveToCommand command,
        List<SimulationStep> timeline)
    {
        var movingContext = TransitionTo(context, RobotState.Moving);
        timeline.Add(CreateStep(movingContext, "Move command started."));

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

        timeline.Add(CreateStep(completedContext, "Move command completed."));

        return completedContext;
    }

    private static SimulationContext ExecuteWait(
        SimulationContext context,
        WaitCommand command,
        List<SimulationStep> timeline)
    {
        var waitingContext = TransitionTo(context, RobotState.Waiting);
        timeline.Add(CreateStep(waitingContext, "Wait command started."));

        var completedContext = waitingContext with
        {
            State = RobotState.Completed,
            ElapsedTime = waitingContext.ElapsedTime + command.Duration
        };

        timeline.Add(CreateStep(completedContext, "Wait command completed."));

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
        string description) =>
        new(
            context.ElapsedTime,
            context.State,
            context.CurrentPosition,
            description);
}
