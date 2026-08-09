using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class DroneSimulator
{
    private readonly DroneMotionPlanner motionPlanner;

    public DroneSimulator()
        : this(new DroneMotionPlanner())
    {
    }

    public DroneSimulator(DroneMotionPlanner motionPlanner)
    {
        ArgumentNullException.ThrowIfNull(motionPlanner);

        this.motionPlanner = motionPlanner;
    }

    public DroneSimulationResult Execute(
        DroneSimulationContext initialContext,
        RobotCommandSequence commandSequence)
    {
        ArgumentNullException.ThrowIfNull(initialContext);
        ArgumentNullException.ThrowIfNull(commandSequence);

        var currentContext = initialContext;
        var timeline = new List<DroneSimulationStep>
        {
            CreateStep(currentContext, "Drone simulation started.")
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

                return new DroneSimulationResult(
                    initialContext,
                    currentContext,
                    timeline.AsReadOnly(),
                    exception);
            }
        }

        return new DroneSimulationResult(
            initialContext,
            currentContext,
            timeline.AsReadOnly(),
            Failure: null);
    }

    private DroneSimulationContext ExecuteCommand(
        DroneSimulationContext context,
        RobotCommand command,
        int commandIndex,
        List<DroneSimulationStep> timeline)
    {
        RobotCommandValidator.Validate(command, context.RobotProfile);

        return command switch
        {
            HomeCommand homeCommand => ExecuteHome(context, homeCommand, commandIndex, timeline),
            ResetFaultCommand resetCommand => ExecuteResetFault(context, resetCommand, commandIndex, timeline),
            DroneMoveCommand moveCommand => ExecuteMove(context, moveCommand, commandIndex, timeline),
            WaitCommand waitCommand => ExecuteWait(context, waitCommand, commandIndex, timeline),
            _ => throw new InvalidOperationException($"Unsupported robot command type: {command.GetType().Name}.")
        };
    }

    private static DroneSimulationContext ExecuteResetFault(
        DroneSimulationContext context,
        ResetFaultCommand command,
        int commandIndex,
        List<DroneSimulationStep> timeline)
    {
        RobotStateTransitions.EnsureCanResetFault(context.State);
        var recoveredContext = context with { State = RobotState.Idle };
        timeline.Add(CreateStep(recoveredContext, "Fault reset. Pose and elapsed time were preserved.", commandIndex, nameof(ResetFaultCommand), command.Source));
        return recoveredContext;
    }

    private DroneSimulationContext ExecuteHome(
        DroneSimulationContext context,
        HomeCommand command,
        int commandIndex,
        List<DroneSimulationStep> timeline)
    {
        var targetPose = new DronePose(
            XMillimeters: 0,
            YMillimeters: 0,
            ZMillimeters: 0,
            YawDegrees: 0,
            RollDegrees: 0,
            PitchDegrees: 0);
        var homingContext = TransitionTo(context, RobotState.Homing);
        var motionPlan = motionPlanner.PlanMove(
            context.CurrentPose,
            targetPose,
            context.RobotProfile);
        var motionSegment = motionPlan.Segments.SingleOrDefault();
        timeline.Add(CreateStep(
            homingContext,
            "Home command started.",
            commandIndex,
            nameof(HomeCommand),
            command.Source,
            motionSegment));

        var completedContext = homingContext with
        {
            CurrentPose = targetPose,
            State = RobotState.Completed,
            ElapsedTime = homingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(
            completedContext,
            "Home command completed.",
            commandIndex,
            nameof(HomeCommand),
            command.Source,
            motionSegment));

        return completedContext;
    }

    private DroneSimulationContext ExecuteMove(
        DroneSimulationContext context,
        DroneMoveCommand command,
        int commandIndex,
        List<DroneSimulationStep> timeline)
    {
        var movingContext = TransitionTo(context, RobotState.Moving);
        var motionPlan = motionPlanner.PlanMove(
            context.CurrentPose,
            command.TargetPose,
            context.RobotProfile,
            command.RequestedLinearVelocityMillimetersPerSecond,
            command.RequestedYawVelocityDegreesPerSecond,
            command.RequestedAttitudeVelocityDegreesPerSecond);
        var motionSegment = motionPlan.Segments.SingleOrDefault();
        timeline.Add(CreateStep(
            movingContext,
            "Drone move started.",
            commandIndex,
            nameof(DroneMoveCommand),
            command.Source,
            motionSegment));

        var completedContext = movingContext with
        {
            CurrentPose = command.TargetPose with
            {
                YawDegrees = DronePose.NormalizeYawDegrees(command.TargetPose.YawDegrees)
            },
            State = RobotState.Completed,
            ElapsedTime = movingContext.ElapsedTime + motionPlan.TotalDuration
        };

        timeline.Add(CreateStep(
            completedContext,
            "Drone move completed.",
            commandIndex,
            nameof(DroneMoveCommand),
            command.Source,
            motionSegment));

        return completedContext;
    }

    private static DroneSimulationContext ExecuteWait(
        DroneSimulationContext context,
        WaitCommand command,
        int commandIndex,
        List<DroneSimulationStep> timeline)
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

    private static DroneSimulationContext TransitionTo(
        DroneSimulationContext context,
        RobotState nextState)
    {
        RobotStateTransitions.EnsureCanTransitionTo(context.State, nextState);

        return context with { State = nextState };
    }

    private static DroneSimulationStep CreateStep(
        DroneSimulationContext context,
        string description,
        int? commandIndex = null,
        string? commandName = null,
        RobotCommandSource? commandSource = null,
        DroneMotionSegment? motionSegment = null) =>
        new(
            context.ElapsedTime,
            context.State,
            context.CurrentPose,
            description,
            commandIndex,
            commandName,
            commandSource)
        {
            TranslationProfile = motionSegment?.TranslationProfile,
            AttitudeProfile = motionSegment?.AttitudeProfile,
            YawProfile = motionSegment?.YawProfile
        };

    private static string GetCommandName(RobotCommand command) => command switch
    {
        HomeCommand => nameof(HomeCommand),
        ResetFaultCommand => nameof(ResetFaultCommand),
        DroneMoveCommand => nameof(DroneMoveCommand),
        WaitCommand => nameof(WaitCommand),
        _ => command.GetType().Name
    };
}
