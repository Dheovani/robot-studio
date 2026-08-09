using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class SimpleArmSimulator
{
    private readonly SimpleArmMotionPlanner motionPlanner;
    private readonly SimpleArmKinematics kinematics;
    private readonly SpatialSimulationEnvironment environment;

    public SimpleArmSimulator()
        : this(new SimpleArmMotionPlanner(), new SimpleArmKinematics(), SpatialSimulationEnvironment.Empty)
    {
    }

    public SimpleArmSimulator(SpatialSimulationEnvironment environment)
        : this(new SimpleArmMotionPlanner(), new SimpleArmKinematics(), environment)
    {
    }

    public SimpleArmSimulator(
        SimpleArmMotionPlanner motionPlanner,
        SimpleArmKinematics kinematics)
        : this(motionPlanner, kinematics, SpatialSimulationEnvironment.Empty)
    {
    }

    public SimpleArmSimulator(
        SimpleArmMotionPlanner motionPlanner,
        SimpleArmKinematics kinematics,
        SpatialSimulationEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(motionPlanner);
        ArgumentNullException.ThrowIfNull(kinematics);
        ArgumentNullException.ThrowIfNull(environment);

        this.motionPlanner = motionPlanner;
        this.kinematics = kinematics;
        this.environment = environment;
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
            ResetFaultCommand resetCommand => ExecuteResetFault(context, resetCommand, commandIndex, timeline),
            SimpleArmMoveJointsCommand moveCommand => ExecuteMove(context, moveCommand, commandIndex, timeline),
            WaitCommand waitCommand => ExecuteWait(context, waitCommand, commandIndex, timeline),
            _ => throw new InvalidOperationException($"Unsupported robot command type: {command.GetType().Name}.")
        };
    }

    private SimpleArmSimulationContext ExecuteResetFault(
        SimpleArmSimulationContext context,
        ResetFaultCommand command,
        int commandIndex,
        List<SimpleArmSimulationStep> timeline)
    {
        RobotStateTransitions.EnsureCanResetFault(context.State);
        var recoveredContext = context with { State = RobotState.Idle };
        timeline.Add(CreateStep(recoveredContext, "Fault reset. Joint positions and elapsed time were preserved.", commandIndex, nameof(ResetFaultCommand), command.Source));
        return recoveredContext;
    }

    private SimpleArmSimulationContext ExecuteHome(
        SimpleArmSimulationContext context,
        HomeCommand command,
        int commandIndex,
        List<SimpleArmSimulationStep> timeline)
    {
        var targetJoints = new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0);
        EnsurePathIsClear(context, targetJoints);
        var homingContext = TransitionTo(context, RobotState.Homing);
        var motionPlan = motionPlanner.PlanMove(
            context.CurrentJoints,
            targetJoints,
            context.RobotProfile);
        var motionProfile = motionPlan.Segments.SingleOrDefault()?.Profile;
        timeline.Add(CreateStep(homingContext, "Home command started.", commandIndex, nameof(HomeCommand), command.Source, motionProfile));

        var completedContext = homingContext with
        {
            CurrentJoints = targetJoints,
            State = RobotState.Completed,
            ElapsedTime = homingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Home command completed.", commandIndex, nameof(HomeCommand), command.Source, motionProfile));

        return completedContext;
    }

    private SimpleArmSimulationContext ExecuteMove(
        SimpleArmSimulationContext context,
        SimpleArmMoveJointsCommand command,
        int commandIndex,
        List<SimpleArmSimulationStep> timeline)
    {
        EnsurePathIsClear(context, command.TargetJoints);
        var movingContext = TransitionTo(context, RobotState.Moving);
        var motionPlan = motionPlanner.PlanMove(
            context.CurrentJoints,
            command.TargetJoints,
            context.RobotProfile,
            command.RequestedJointVelocityDegreesPerSecond);
        var motionProfile = motionPlan.Segments.SingleOrDefault()?.Profile;
        timeline.Add(CreateStep(movingContext, "Simple arm joint move started.", commandIndex, nameof(SimpleArmMoveJointsCommand), command.Source, motionProfile));

        var completedContext = movingContext with
        {
            CurrentJoints = command.TargetJoints,
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Simple arm joint move completed.", commandIndex, nameof(SimpleArmMoveJointsCommand), command.Source, motionProfile));

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

    private void EnsurePathIsClear(SimpleArmSimulationContext context, SimpleArmJointPosition target)
    {
        var collision = SimpleArmLinkCollisionDetector.FindFirstCollision(
            context.CurrentJoints, target, context.RobotProfile, environment);
        if (collision is not null)
        {
            throw new SpatialPathObstructedException("Simple Articulated Arm", collision);
        }
    }

    private SimpleArmSimulationStep CreateStep(
        SimpleArmSimulationContext context,
        string description,
        int? commandIndex = null,
        string? commandName = null,
        RobotCommandSource? commandSource = null,
        TrapezoidalMotionProfile? motionProfile = null) =>
        new(
            context.ElapsedTime,
            context.State,
            context.CurrentJoints,
            kinematics.Forward(context.RobotProfile, context.CurrentJoints),
            description,
            commandIndex,
            commandName,
            commandSource)
        {
            MotionProfile = motionProfile
        };

    private static string GetCommandName(RobotCommand command) => command switch
    {
        HomeCommand => nameof(HomeCommand),
        ResetFaultCommand => nameof(ResetFaultCommand),
        SimpleArmMoveJointsCommand => nameof(SimpleArmMoveJointsCommand),
        WaitCommand => nameof(WaitCommand),
        _ => command.GetType().Name
    };
}
