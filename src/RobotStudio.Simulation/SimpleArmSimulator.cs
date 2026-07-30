using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class SimpleArmSimulator
{
    private readonly SimpleArmMotionPlanner motionPlanner;
    private readonly SimpleArmKinematics kinematics;

    public SimpleArmSimulator()
        : this(new SimpleArmMotionPlanner(), new SimpleArmKinematics())
    {
    }

    public SimpleArmSimulator(
        SimpleArmMotionPlanner motionPlanner,
        SimpleArmKinematics kinematics)
    {
        ArgumentNullException.ThrowIfNull(motionPlanner);
        ArgumentNullException.ThrowIfNull(kinematics);

        this.motionPlanner = motionPlanner;
        this.kinematics = kinematics;
    }

    public SimpleArmSimulationResult Execute(
        SimpleArmSimulationContext initialContext,
        RobotCommandSequence commandSequence)
    {
        ArgumentNullException.ThrowIfNull(initialContext);
        ArgumentNullException.ThrowIfNull(commandSequence);

        var currentContext = initialContext;
        var timeline = new List<SimpleArmSimulationStep>
        {
            CreateStep(currentContext, "Simple arm simulation started.")
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

                return new SimpleArmSimulationResult(
                    initialContext,
                    currentContext,
                    timeline.AsReadOnly(),
                    exception);
            }
        }

        return new SimpleArmSimulationResult(
            initialContext,
            currentContext,
            timeline.AsReadOnly(),
            Failure: null);
    }

    private SimpleArmSimulationContext ExecuteCommand(
        SimpleArmSimulationContext context,
        RobotCommand command,
        int commandIndex,
        List<SimpleArmSimulationStep> timeline)
    {
        RobotCommandValidator.Validate(command, context.RobotProfile);

        return command switch
        {
            HomeCommand homeCommand => ExecuteHome(context, homeCommand, commandIndex, timeline),
            SimpleArmMoveJointsCommand moveCommand => ExecuteMove(context, moveCommand, commandIndex, timeline),
            WaitCommand waitCommand => ExecuteWait(context, waitCommand, commandIndex, timeline),
            _ => throw new InvalidOperationException($"Unsupported robot command type: {command.GetType().Name}.")
        };
    }

    private SimpleArmSimulationContext ExecuteHome(
        SimpleArmSimulationContext context,
        HomeCommand command,
        int commandIndex,
        List<SimpleArmSimulationStep> timeline)
    {
        var targetJoints = new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0);
        var homingContext = TransitionTo(context, RobotState.Homing);
        timeline.Add(CreateStep(homingContext, "Home command started.", commandIndex, nameof(HomeCommand), command.Source));

        var motionPlan = motionPlanner.PlanMove(
            context.CurrentJoints,
            targetJoints,
            context.RobotProfile);

        var completedContext = homingContext with
        {
            CurrentJoints = targetJoints,
            State = RobotState.Completed,
            ElapsedTime = homingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Home command completed.", commandIndex, nameof(HomeCommand), command.Source));

        return completedContext;
    }

    private SimpleArmSimulationContext ExecuteMove(
        SimpleArmSimulationContext context,
        SimpleArmMoveJointsCommand command,
        int commandIndex,
        List<SimpleArmSimulationStep> timeline)
    {
        var movingContext = TransitionTo(context, RobotState.Moving);
        timeline.Add(CreateStep(movingContext, "Simple arm joint move started.", commandIndex, nameof(SimpleArmMoveJointsCommand), command.Source));

        var motionPlan = motionPlanner.PlanMove(
            context.CurrentJoints,
            command.TargetJoints,
            context.RobotProfile,
            command.RequestedJointVelocityDegreesPerSecond);

        var completedContext = movingContext with
        {
            CurrentJoints = command.TargetJoints,
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Simple arm joint move completed.", commandIndex, nameof(SimpleArmMoveJointsCommand), command.Source));

        return completedContext;
    }

    private SimpleArmSimulationContext ExecuteWait(
        SimpleArmSimulationContext context,
        WaitCommand command,
        int commandIndex,
        List<SimpleArmSimulationStep> timeline)
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

    private static SimpleArmSimulationContext TransitionTo(
        SimpleArmSimulationContext context,
        RobotState nextState)
    {
        RobotStateTransitions.EnsureCanTransitionTo(context.State, nextState);

        return context with { State = nextState };
    }

    private SimpleArmSimulationStep CreateStep(
        SimpleArmSimulationContext context,
        string description,
        int? commandIndex = null,
        string? commandName = null,
        RobotCommandSource? commandSource = null) =>
        new(
            context.ElapsedTime,
            context.State,
            context.CurrentJoints,
            kinematics.Forward(context.RobotProfile, context.CurrentJoints),
            description,
            commandIndex,
            commandName,
            commandSource);

    private static string GetCommandName(RobotCommand command) => command switch
    {
        HomeCommand => nameof(HomeCommand),
        SimpleArmMoveJointsCommand => nameof(SimpleArmMoveJointsCommand),
        WaitCommand => nameof(WaitCommand),
        _ => command.GetType().Name
    };
}
