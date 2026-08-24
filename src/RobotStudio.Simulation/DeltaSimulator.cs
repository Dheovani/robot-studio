using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Parallel;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class DeltaSimulator
{
    private readonly DeltaMotionPlanner motionPlanner;
    private readonly DeltaCartesianMotionPlanner cartesianMotionPlanner;
    private readonly DeltaKinematics kinematics;
    private readonly SpatialSimulationEnvironment environment;

    public DeltaSimulator()
        : this(
            new DeltaMotionPlanner(),
            new DeltaCartesianMotionPlanner(),
            new DeltaKinematics(),
            SpatialSimulationEnvironment.Empty)
    {
    }

    public DeltaSimulator(SpatialSimulationEnvironment environment)
        : this(
            new DeltaMotionPlanner(),
            new DeltaCartesianMotionPlanner(),
            new DeltaKinematics(),
            environment)
    {
    }

    public DeltaSimulator(
        DeltaMotionPlanner motionPlanner,
        DeltaKinematics kinematics)
        : this(
            motionPlanner,
            new DeltaCartesianMotionPlanner(kinematics),
            kinematics,
            SpatialSimulationEnvironment.Empty)
    {
    }

    public DeltaSimulator(
        DeltaMotionPlanner motionPlanner,
        DeltaCartesianMotionPlanner cartesianMotionPlanner,
        DeltaKinematics kinematics,
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
            ResetFaultCommand resetCommand => ExecuteResetFault(context, resetCommand, commandIndex, timeline),
            DeltaMoveActuatorsCommand moveCommand => ExecuteMove(context, moveCommand, commandIndex, timeline),
            DeltaLinearMoveCommand linearMoveCommand => ExecuteLinearMove(context, linearMoveCommand, commandIndex, timeline),
            WaitCommand waitCommand => ExecuteWait(context, waitCommand, commandIndex, timeline),
            _ => throw new InvalidOperationException($"Unsupported robot command type: {command.GetType().Name}.")
        };
    }

    private DeltaSimulationContext ExecuteResetFault(
        DeltaSimulationContext context,
        ResetFaultCommand command,
        int commandIndex,
        List<DeltaSimulationStep> timeline)
    {
        RobotStateTransitions.EnsureCanResetFault(context.State);
        var recoveredContext = context with { State = RobotState.Idle };
        timeline.Add(CreateStep(recoveredContext, "Fault reset. Actuator positions and elapsed time were preserved.", commandIndex, nameof(ResetFaultCommand), command.Source));
        return recoveredContext;
    }

    private DeltaSimulationContext ExecuteHome(
        DeltaSimulationContext context,
        HomeCommand command,
        int commandIndex,
        List<DeltaSimulationStep> timeline)
    {
        var targetActuators = new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 0, CMillimeters: 0);
        EnsurePathIsClear(context, targetActuators);
        var homingContext = TransitionTo(context, RobotState.Homing);
        var motionPlan = motionPlanner.PlanMove(
            context.CurrentActuators,
            targetActuators,
            context.RobotProfile);
        var motionProfile = motionPlan.Segments.SingleOrDefault()?.Profile;
        timeline.Add(CreateStep(homingContext, "Home command started.", commandIndex, nameof(HomeCommand), command.Source, motionProfile));

        var completedContext = homingContext with
        {
            CurrentActuators = targetActuators,
            State = RobotState.Completed,
            ElapsedTime = homingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Home command completed.", commandIndex, nameof(HomeCommand), command.Source, motionProfile));

        return completedContext;
    }

    private DeltaSimulationContext ExecuteMove(
        DeltaSimulationContext context,
        DeltaMoveActuatorsCommand command,
        int commandIndex,
        List<DeltaSimulationStep> timeline)
    {
        EnsurePathIsClear(context, command.TargetActuators);
        var movingContext = TransitionTo(context, RobotState.Moving);
        var motionPlan = motionPlanner.PlanMove(
            context.CurrentActuators,
            command.TargetActuators,
            context.RobotProfile,
            command.RequestedActuatorVelocityMillimetersPerSecond);
        var motionProfile = motionPlan.Segments.SingleOrDefault()?.Profile;
        timeline.Add(CreateStep(movingContext, "Delta actuator move started.", commandIndex, nameof(DeltaMoveActuatorsCommand), command.Source, motionProfile));

        var completedContext = movingContext with
        {
            CurrentActuators = command.TargetActuators,
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(completedContext, "Delta actuator move completed.", commandIndex, nameof(DeltaMoveActuatorsCommand), command.Source, motionProfile));

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

    private DeltaSimulationContext ExecuteLinearMove(
        DeltaSimulationContext context,
        DeltaLinearMoveCommand command,
        int commandIndex,
        List<DeltaSimulationStep> timeline)
    {
        var motionPlan = cartesianMotionPlanner.PlanLinearMove(
            context.CurrentActuators,
            command.TargetToolPose,
            context.RobotProfile,
            command.RequestedToolVelocityMillimetersPerSecond);
        var movingContext = TransitionTo(context, RobotState.Moving);

        if (motionPlan.IsStationary)
        {
            timeline.Add(CreateStep(
                movingContext,
                "Delta linear tool move started.",
                commandIndex,
                nameof(DeltaLinearMoveCommand),
                command.Source));
            var stationaryContext = movingContext with { State = RobotState.Completed };
            timeline.Add(CreateStep(
                stationaryContext,
                "Delta linear tool move completed without displacement.",
                commandIndex,
                nameof(DeltaLinearMoveCommand),
                command.Source));
            return stationaryContext;
        }

        EnsureCartesianPathIsClear(context, motionPlan);
        timeline.Add(CreateStep(
            movingContext,
            "Delta linear tool move started.",
            commandIndex,
            nameof(DeltaLinearMoveCommand),
            command.Source,
            motionPlan.ToolMotionProfile,
            motionPlan));

        var completedContext = movingContext with
        {
            CurrentActuators = motionPlan.Segments[^1].EndActuators,
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };
        timeline.Add(CreateStep(
            completedContext,
            "Delta linear tool move completed.",
            commandIndex,
            nameof(DeltaLinearMoveCommand),
            command.Source));
        return completedContext;
    }

    private static DeltaSimulationContext TransitionTo(
        DeltaSimulationContext context,
        RobotState nextState)
    {
        RobotStateTransitions.EnsureCanTransitionTo(context.State, nextState);

        return context with { State = nextState };
    }

    private void EnsurePathIsClear(DeltaSimulationContext context, DeltaActuatorPosition target)
    {
        var collision = DeltaMechanismCollisionDetector.FindFirstCollision(
            context.CurrentActuators, target, context.RobotProfile, environment);
        if (collision is not null)
        {
            throw new SpatialPathObstructedException("Delta Robot", collision);
        }
    }

    private void EnsureCartesianPathIsClear(
        DeltaSimulationContext context,
        DeltaCartesianMotionPlan motionPlan)
    {
        foreach (var segment in motionPlan.Segments)
        {
            var collision = DeltaMechanismCollisionDetector.FindFirstCollision(
                segment.StartActuators,
                segment.EndActuators,
                context.RobotProfile,
                environment);
            if (collision is not null)
            {
                throw new SpatialPathObstructedException("Delta Robot", collision);
            }
        }
    }

    private DeltaSimulationStep CreateStep(
        DeltaSimulationContext context,
        string description,
        int? commandIndex = null,
        string? commandName = null,
        RobotCommandSource? commandSource = null,
        TrapezoidalMotionProfile? motionProfile = null,
        DeltaCartesianMotionPlan? cartesianMotionPlan = null) =>
        new(
            context.ElapsedTime,
            context.State,
            context.CurrentActuators,
            kinematics.Forward(context.RobotProfile, context.CurrentActuators),
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
        DeltaMoveActuatorsCommand => nameof(DeltaMoveActuatorsCommand),
        DeltaLinearMoveCommand => nameof(DeltaLinearMoveCommand),
        WaitCommand => nameof(WaitCommand),
        _ => command.GetType().Name
    };
}
