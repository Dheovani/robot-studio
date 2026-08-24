using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class ScaraSimulator
{
    private readonly ScaraMotionPlanner motionPlanner;
    private readonly ScaraCartesianMotionPlanner cartesianMotionPlanner;
    private readonly ScaraKinematics kinematics;
    private readonly PlanarSimulationEnvironment environment;
    private readonly double maximumCollisionJointStepDegrees;

    public ScaraSimulator()
        : this(
            new ScaraMotionPlanner(),
            new ScaraCartesianMotionPlanner(),
            new ScaraKinematics(),
            PlanarSimulationEnvironment.Empty,
            ScaraLinkCollisionDetector.DefaultMaximumJointStepDegrees)
    {
    }

    public ScaraSimulator(PlanarSimulationEnvironment environment)
        : this(
            new ScaraMotionPlanner(),
            new ScaraCartesianMotionPlanner(),
            new ScaraKinematics(),
            environment,
            ScaraLinkCollisionDetector.DefaultMaximumJointStepDegrees)
    {
    }

    public ScaraSimulator(
        ScaraMotionPlanner motionPlanner,
        ScaraKinematics kinematics)
        : this(
            motionPlanner,
            new ScaraCartesianMotionPlanner(kinematics),
            kinematics,
            PlanarSimulationEnvironment.Empty,
            ScaraLinkCollisionDetector.DefaultMaximumJointStepDegrees)
    {
    }

    public ScaraSimulator(
        ScaraMotionPlanner motionPlanner,
        ScaraCartesianMotionPlanner cartesianMotionPlanner,
        ScaraKinematics kinematics,
        PlanarSimulationEnvironment environment,
        double maximumCollisionJointStepDegrees)
    {
        ArgumentNullException.ThrowIfNull(motionPlanner);
        ArgumentNullException.ThrowIfNull(cartesianMotionPlanner);
        ArgumentNullException.ThrowIfNull(kinematics);
        ArgumentNullException.ThrowIfNull(environment);

        if (!double.IsFinite(maximumCollisionJointStepDegrees) || maximumCollisionJointStepDegrees <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumCollisionJointStepDegrees),
                "Maximum collision sampling step must be a finite number greater than zero.");
        }

        this.motionPlanner = motionPlanner;
        this.cartesianMotionPlanner = cartesianMotionPlanner;
        this.kinematics = kinematics;
        this.environment = environment;
        this.maximumCollisionJointStepDegrees = maximumCollisionJointStepDegrees;
    }

    public ScaraSimulationResult Execute(
        ScaraSimulationContext initialContext,
        RobotCommandSequence commandSequence)
    {
        ArgumentNullException.ThrowIfNull(initialContext);
        ArgumentNullException.ThrowIfNull(commandSequence);

        var currentContext = initialContext;
        var timeline = new List<ScaraSimulationStep>
        {
            CreateStep(currentContext, "SCARA simulation started.")
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

                return new ScaraSimulationResult(
                    initialContext,
                    currentContext,
                    timeline.AsReadOnly(),
                    exception);
            }
        }

        return new ScaraSimulationResult(
            initialContext,
            currentContext,
            timeline.AsReadOnly(),
            Failure: null);
    }

    private ScaraSimulationContext ExecuteCommand(
        ScaraSimulationContext context,
        RobotCommand command,
        int commandIndex,
        List<ScaraSimulationStep> timeline)
    {
        RobotCommandValidator.Validate(command, context.RobotProfile);

        return command switch
        {
            HomeCommand homeCommand => ExecuteHome(context, homeCommand, commandIndex, timeline),
            ResetFaultCommand resetCommand => ExecuteResetFault(context, resetCommand, commandIndex, timeline),
            ScaraMoveJointsCommand moveCommand => ExecuteMove(context, moveCommand, commandIndex, timeline),
            ScaraLinearMoveCommand linearMoveCommand => ExecuteLinearMove(context, linearMoveCommand, commandIndex, timeline),
            WaitCommand waitCommand => ExecuteWait(context, waitCommand, commandIndex, timeline),
            _ => throw new InvalidOperationException($"Unsupported robot command type: {command.GetType().Name}.")
        };
    }

    private ScaraSimulationContext ExecuteResetFault(
        ScaraSimulationContext context,
        ResetFaultCommand command,
        int commandIndex,
        List<ScaraSimulationStep> timeline)
    {
        RobotStateTransitions.EnsureCanResetFault(context.State);
        var recoveredContext = context with { State = RobotState.Idle };
        timeline.Add(CreateStep(recoveredContext, "Fault reset. Joint positions and elapsed time were preserved.", commandIndex, nameof(ResetFaultCommand), command.Source));
        return recoveredContext;
    }

    private ScaraSimulationContext ExecuteHome(
        ScaraSimulationContext context,
        HomeCommand command,
        int commandIndex,
        List<ScaraSimulationStep> timeline)
    {
        var targetJoints = new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0);
        var motionPlan = motionPlanner.PlanMove(
            context.CurrentJoints,
            targetJoints,
            context.RobotProfile);
        EnsurePathIsClear(context, targetJoints);
        var homingContext = TransitionTo(context, RobotState.Homing);
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

    private ScaraSimulationContext ExecuteMove(
        ScaraSimulationContext context,
        ScaraMoveJointsCommand command,
        int commandIndex,
        List<ScaraSimulationStep> timeline)
    {
        var motionPlan = motionPlanner.PlanMove(
            context.CurrentJoints,
            command.TargetJoints,
            context.RobotProfile,
            command.RequestedJointVelocityDegreesPerSecond);
        EnsurePathIsClear(context, command.TargetJoints);
        var movingContext = TransitionTo(context, RobotState.Moving);
        var motionProfile = motionPlan.Segments.SingleOrDefault()?.Profile;
        timeline.Add(CreateStep(movingContext, "SCARA joint move started.", commandIndex, nameof(ScaraMoveJointsCommand), command.Source, motionProfile));

        var completedContext = movingContext with
        {
            CurrentJoints = command.TargetJoints,
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "SCARA joint move completed.", commandIndex, nameof(ScaraMoveJointsCommand), command.Source, motionProfile));

        return completedContext;
    }

    private ScaraSimulationContext ExecuteLinearMove(
        ScaraSimulationContext context,
        ScaraLinearMoveCommand command,
        int commandIndex,
        List<ScaraSimulationStep> timeline)
    {
        var motionPlan = cartesianMotionPlanner.PlanLinearMove(
            context.CurrentJoints,
            command.TargetToolPose,
            context.RobotProfile,
            command.RequestedToolVelocityMillimetersPerSecond);
        var movingContext = TransitionTo(context, RobotState.Moving);

        if (motionPlan.IsStationary)
        {
            timeline.Add(CreateStep(
                movingContext,
                "SCARA linear tool move started.",
                commandIndex,
                nameof(ScaraLinearMoveCommand),
                command.Source));
            var stationaryContext = movingContext with { State = RobotState.Completed };
            timeline.Add(CreateStep(
                stationaryContext,
                "SCARA linear tool move completed without displacement.",
                commandIndex,
                nameof(ScaraLinearMoveCommand),
                command.Source));
            return stationaryContext;
        }

        EnsureCartesianPathIsClear(context, motionPlan);

        timeline.Add(CreateStep(
            movingContext,
            "SCARA linear tool move started.",
            commandIndex,
            nameof(ScaraLinearMoveCommand),
            command.Source,
            motionPlan.ToolMotionProfile,
            motionPlan));

        var completedContext = movingContext with
        {
            CurrentJoints = motionPlan.Segments[^1].EndJoints,
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };
        timeline.Add(CreateStep(
            completedContext,
            "SCARA linear tool move completed.",
            commandIndex,
            nameof(ScaraLinearMoveCommand),
            command.Source));

        return completedContext;
    }

    private ScaraSimulationContext ExecuteWait(
        ScaraSimulationContext context,
        WaitCommand command,
        int commandIndex,
        List<ScaraSimulationStep> timeline)
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

    private static ScaraSimulationContext TransitionTo(
        ScaraSimulationContext context,
        RobotState nextState)
    {
        RobotStateTransitions.EnsureCanTransitionTo(context.State, nextState);

        return context with { State = nextState };
    }

    private void EnsurePathIsClear(
        ScaraSimulationContext context,
        ScaraJointPosition targetJoints)
    {
        var collision = ScaraLinkCollisionDetector.FindFirstCollision(
            context.CurrentJoints,
            targetJoints,
            context.RobotProfile,
            environment,
            maximumCollisionJointStepDegrees);

        if (collision is not null)
        {
            throw new ScaraPathObstructedException(collision);
        }
    }

    private void EnsureCartesianPathIsClear(
        ScaraSimulationContext context,
        ScaraCartesianMotionPlan motionPlan)
    {
        foreach (var segment in motionPlan.Segments)
        {
            var collision = ScaraLinkCollisionDetector.FindFirstCollision(
                segment.StartJoints,
                segment.EndJoints,
                context.RobotProfile,
                environment,
                maximumCollisionJointStepDegrees);

            if (collision is not null)
            {
                throw new ScaraPathObstructedException(collision);
            }
        }
    }

    private ScaraSimulationStep CreateStep(
        ScaraSimulationContext context,
        string description,
        int? commandIndex = null,
        string? commandName = null,
        RobotCommandSource? commandSource = null,
        TrapezoidalMotionProfile? motionProfile = null,
        ScaraCartesianMotionPlan? cartesianMotionPlan = null) =>
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

    private static string GetCommandName(RobotCommand command) => command switch
    {
        HomeCommand => nameof(HomeCommand),
        ResetFaultCommand => nameof(ResetFaultCommand),
        ScaraMoveJointsCommand => nameof(ScaraMoveJointsCommand),
        ScaraLinearMoveCommand => nameof(ScaraLinearMoveCommand),
        WaitCommand => nameof(WaitCommand),
        _ => command.GetType().Name
    };
}
