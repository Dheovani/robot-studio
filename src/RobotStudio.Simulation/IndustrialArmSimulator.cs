using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class IndustrialArmSimulator
{
    private readonly IndustrialArmMotionPlanner motionPlanner;
    private readonly IndustrialArmCartesianMotionPlanner cartesianMotionPlanner;
    private readonly IndustrialArmKinematics kinematics;
    private readonly SpatialSimulationEnvironment environment;

    public IndustrialArmSimulator()
        : this(
            new IndustrialArmMotionPlanner(),
            new IndustrialArmCartesianMotionPlanner(),
            new IndustrialArmKinematics(),
            SpatialSimulationEnvironment.Empty)
    {
    }

    public IndustrialArmSimulator(SpatialSimulationEnvironment environment)
        : this(
            new IndustrialArmMotionPlanner(),
            new IndustrialArmCartesianMotionPlanner(),
            new IndustrialArmKinematics(),
            environment)
    {
    }

    public IndustrialArmSimulator(
        IndustrialArmMotionPlanner motionPlanner,
        IndustrialArmKinematics kinematics,
        SpatialSimulationEnvironment environment)
        : this(
            motionPlanner,
            new IndustrialArmCartesianMotionPlanner(kinematics),
            kinematics,
            environment)
    {
    }

    public IndustrialArmSimulator(
        IndustrialArmMotionPlanner motionPlanner,
        IndustrialArmCartesianMotionPlanner cartesianMotionPlanner,
        IndustrialArmKinematics kinematics,
        SpatialSimulationEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(motionPlanner);
        ArgumentNullException.ThrowIfNull(cartesianMotionPlanner);
        ArgumentNullException.ThrowIfNull(kinematics);
        ArgumentNullException.ThrowIfNull(environment);
        this.motionPlanner = motionPlanner;
        this.cartesianMotionPlanner = cartesianMotionPlanner;
        this.kinematics = kinematics;
        this.environment = environment;
    }

    public IndustrialArmSimulationResult Execute(
        IndustrialArmSimulationContext initialContext,
        RobotCommandSequence commandSequence)
    {
        ArgumentNullException.ThrowIfNull(initialContext);
        ArgumentNullException.ThrowIfNull(commandSequence);

        var context = initialContext;
        var timeline = new List<IndustrialArmSimulationStep>
        {
            CreateStep(context, "Industrial arm simulation started.")
        };

        for (var index = 0; index < commandSequence.Commands.Count; index++)
        {
            var command = commandSequence.Commands[index];
            try
            {
                context = ExecuteCommand(context, command, index, timeline);
            }
            catch (InvalidOperationException exception)
            {
                context = context with { State = RobotState.Faulted };
                timeline.Add(CreateStep(context, exception.Message, index, command.GetType().Name, command.Source));
                return new IndustrialArmSimulationResult(initialContext, context, timeline.AsReadOnly(), exception);
            }
        }

        return new IndustrialArmSimulationResult(initialContext, context, timeline.AsReadOnly(), Failure: null);
    }

    private IndustrialArmSimulationContext ExecuteCommand(
        IndustrialArmSimulationContext context,
        RobotCommand command,
        int commandIndex,
        List<IndustrialArmSimulationStep> timeline)
    {
        RobotCommandValidator.Validate(command, context.RobotProfile);

        return command switch
        {
            HomeCommand => ExecuteMove(context, IndustrialArmJointPosition.Home, null, RobotState.Homing, command, commandIndex, timeline),
            ResetFaultCommand reset => ExecuteResetFault(context, reset, commandIndex, timeline),
            IndustrialArmMoveJointsCommand move => ExecuteMove(context, move.TargetJoints, move.RequestedJointVelocityDegreesPerSecond, RobotState.Moving, command, commandIndex, timeline),
            IndustrialArmLinearMoveCommand move => ExecuteLinearMove(context, move, commandIndex, timeline),
            WaitCommand wait => ExecuteWait(context, wait, commandIndex, timeline),
            _ => throw new InvalidOperationException($"Unsupported robot command type: {command.GetType().Name}.")
        };
    }

    private IndustrialArmSimulationContext ExecuteLinearMove(
        IndustrialArmSimulationContext context,
        IndustrialArmLinearMoveCommand command,
        int commandIndex,
        List<IndustrialArmSimulationStep> timeline)
    {
        var plan = cartesianMotionPlanner.PlanLinearMove(
            context.CurrentJoints,
            command.TargetToolPose,
            context.RobotProfile,
            command.RequestedToolVelocityMillimetersPerSecond,
            command.Configuration);

        foreach (var segment in plan.Segments)
        {
            var collision = IndustrialArmLinkCollisionDetector.FindFirstCollision(
                segment.StartJoints,
                segment.EndJoints,
                context.RobotProfile,
                environment);
            if (collision is not null)
            {
                throw new SpatialPathObstructedException("6-DOF Industrial Arm", collision);
            }
        }

        var activeContext = TransitionTo(context, RobotState.Moving);
        timeline.Add(CreateStep(
            activeContext,
            $"{nameof(IndustrialArmLinearMoveCommand)} started.",
            commandIndex,
            nameof(IndustrialArmLinearMoveCommand),
            command.Source,
            plan.ProgressMotionProfile,
            plan));
        var completed = activeContext with
        {
            CurrentJoints = plan.Segments.LastOrDefault()?.EndJoints ?? context.CurrentJoints,
            State = RobotState.Completed,
            ElapsedTime = activeContext.ElapsedTime + plan.TotalDuration
        };
        timeline.Add(CreateStep(
            completed,
            $"{nameof(IndustrialArmLinearMoveCommand)} completed.",
            commandIndex,
            nameof(IndustrialArmLinearMoveCommand),
            command.Source,
            plan.ProgressMotionProfile,
            plan));
        return completed;
    }

    private IndustrialArmSimulationContext ExecuteResetFault(
        IndustrialArmSimulationContext context,
        ResetFaultCommand command,
        int commandIndex,
        List<IndustrialArmSimulationStep> timeline)
    {
        RobotStateTransitions.EnsureCanResetFault(context.State);
        var recoveredContext = context with { State = RobotState.Idle };
        timeline.Add(CreateStep(recoveredContext, "Fault reset. Joint positions and elapsed time were preserved.", commandIndex, nameof(ResetFaultCommand), command.Source));
        return recoveredContext;
    }

    private IndustrialArmSimulationContext ExecuteMove(
        IndustrialArmSimulationContext context,
        IndustrialArmJointPosition target,
        double? requestedVelocity,
        RobotState activeState,
        RobotCommand command,
        int commandIndex,
        List<IndustrialArmSimulationStep> timeline)
    {
        var plan = motionPlanner.PlanMove(context.CurrentJoints, target, context.RobotProfile, requestedVelocity);
        var collision = IndustrialArmLinkCollisionDetector.FindFirstCollision(
            context.CurrentJoints, target, context.RobotProfile, environment);
        if (collision is not null)
        {
            throw new SpatialPathObstructedException("6-DOF Industrial Arm", collision);
        }

        var activeContext = TransitionTo(context, activeState);
        var motionProfile = plan.Segments.SingleOrDefault()?.Profile;
        timeline.Add(CreateStep(activeContext, $"{command.GetType().Name} started.", commandIndex, command.GetType().Name, command.Source, motionProfile));
        var completed = activeContext with
        {
            CurrentJoints = target,
            State = RobotState.Completed,
            ElapsedTime = activeContext.ElapsedTime + plan.TotalDuration
        };
        timeline.Add(CreateStep(completed, $"{command.GetType().Name} completed.", commandIndex, command.GetType().Name, command.Source, motionProfile));
        return completed;
    }

    private IndustrialArmSimulationContext ExecuteWait(
        IndustrialArmSimulationContext context,
        WaitCommand command,
        int commandIndex,
        List<IndustrialArmSimulationStep> timeline)
    {
        var waiting = TransitionTo(context, RobotState.Waiting);
        timeline.Add(CreateStep(waiting, "Wait command started.", commandIndex, nameof(WaitCommand), command.Source));
        var completed = waiting with
        {
            State = RobotState.Completed,
            ElapsedTime = waiting.ElapsedTime + command.Duration
        };
        timeline.Add(CreateStep(completed, "Wait command completed.", commandIndex, nameof(WaitCommand), command.Source));
        return completed;
    }

    private static IndustrialArmSimulationContext TransitionTo(
        IndustrialArmSimulationContext context,
        RobotState state)
    {
        RobotStateTransitions.EnsureCanTransitionTo(context.State, state);
        return context with { State = state };
    }

    private IndustrialArmSimulationStep CreateStep(
        IndustrialArmSimulationContext context,
        string description,
        int? commandIndex = null,
        string? commandName = null,
        RobotCommandSource? commandSource = null,
        TrapezoidalMotionProfile? motionProfile = null,
        IndustrialArmCartesianMotionPlan? cartesianMotionPlan = null) =>
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
            MotionProfile = motionProfile,
            CartesianMotionPlan = cartesianMotionPlan
        };
}
