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
        var motionPlan = motionPlanner.PlanMove(
            context.CurrentPose,
            targetPose,
            context.RobotProfile);

        return ExecuteMotionPlan(
            homingContext,
            motionPlan,
            commandIndex,
            nameof(HomeCommand),
            command.Source,
            timeline);
    }

    private DifferentialDriveSimulationContext ExecuteMove(
        DifferentialDriveSimulationContext context,
        DifferentialDriveMoveCommand command,
        int commandIndex,
        List<DifferentialDriveSimulationStep> timeline)
    {
        var movingContext = TransitionTo(context, RobotState.Moving);
        var motionPlan = motionPlanner.PlanMove(
            context.CurrentPose,
            command.TargetPose,
            context.RobotProfile,
            command.RequestedLinearVelocityMillimetersPerSecond,
            command.RequestedAngularVelocityDegreesPerSecond);

        return ExecuteMotionPlan(
            movingContext,
            motionPlan,
            commandIndex,
            nameof(DifferentialDriveMoveCommand),
            command.Source,
            timeline);
    }

    private static DifferentialDriveSimulationContext ExecuteMotionPlan(
        DifferentialDriveSimulationContext activeContext,
        DifferentialDriveMotionPlan motionPlan,
        int commandIndex,
        string commandName,
        RobotCommandSource? commandSource,
        List<DifferentialDriveSimulationStep> timeline)
    {
        var segmentContext = activeContext;

        if (motionPlan.IsStationary)
        {
            timeline.Add(CreateStep(
                segmentContext,
                $"{commandName} started.",
                commandIndex,
                commandName,
                commandSource));
        }

        foreach (var segment in motionPlan.Segments)
        {
            timeline.Add(CreateStep(
                segmentContext,
                $"Differential drive {segment.Kind.ToString().ToLowerInvariant()} started.",
                commandIndex,
                commandName,
                commandSource,
                segment.Profile));

            segmentContext = segmentContext with
            {
                CurrentPose = segment.End,
                Odometry = DifferentialDriveOdometryCalculator.Advance(
                    segmentContext.Odometry,
                    segmentContext.RobotProfile,
                    segment.Start,
                    segment.End),
                ElapsedTime = segmentContext.ElapsedTime + segment.Duration
            };
        }

        var completedContext = segmentContext with { State = RobotState.Completed };
        timeline.Add(CreateStep(
            completedContext,
            $"{commandName} completed.",
            commandIndex,
            commandName,
            commandSource));

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
        RobotCommandSource? commandSource = null,
        TrapezoidalMotionProfile? motionProfile = null) =>
        new(
            context.ElapsedTime,
            context.State,
            context.CurrentPose,
            context.Odometry,
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
        DifferentialDriveMoveCommand => nameof(DifferentialDriveMoveCommand),
        WaitCommand => nameof(WaitCommand),
        _ => command.GetType().Name
    };
}
