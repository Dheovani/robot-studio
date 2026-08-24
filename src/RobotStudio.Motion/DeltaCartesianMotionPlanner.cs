using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Motion;

public sealed class DeltaCartesianMotionPlanner
{
    public const double DefaultMaximumToolSegmentLengthMillimeters = 2;

    private const double ToolToleranceMillimeters = 0.000_001;
    private const double ActuatorToleranceMillimeters = 0.000_001;

    private readonly DeltaKinematics kinematics;

    public DeltaCartesianMotionPlanner()
        : this(new DeltaKinematics())
    {
    }

    public DeltaCartesianMotionPlanner(DeltaKinematics kinematics)
    {
        ArgumentNullException.ThrowIfNull(kinematics);
        this.kinematics = kinematics;
    }

    public DeltaCartesianMotionPlan PlanLinearMove(
        DeltaActuatorPosition startActuators,
        DeltaToolPose targetToolPose,
        DeltaRobotProfile robotProfile,
        double? requestedToolVelocityMillimetersPerSecond = null,
        double maximumToolSegmentLengthMillimeters = DefaultMaximumToolSegmentLengthMillimeters)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedToolVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedToolVelocityMillimetersPerSecond),
                "Requested Delta tool velocity must be greater than zero.");
        }

        if (!double.IsFinite(maximumToolSegmentLengthMillimeters) ||
            maximumToolSegmentLengthMillimeters <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumToolSegmentLengthMillimeters),
                "Maximum Delta tool segment length must be finite and greater than zero.");
        }

        robotProfile.ValidatePosition(startActuators);
        var startToolPose = kinematics.Forward(robotProfile, startActuators);
        var distance = Distance(startToolPose, targetToolPose);
        if (distance <= ToolToleranceMillimeters)
        {
            return new DeltaCartesianMotionPlan(
                startToolPose,
                targetToolPose,
                0,
                ToolMotionProfile: null,
                Segments: Array.Empty<DeltaCartesianMotionSegment>());
        }

        var sampleCount = Math.Max(
            1,
            (int)Math.Ceiling(distance / maximumToolSegmentLengthMillimeters));
        var segments = new List<DeltaCartesianMotionSegment>(sampleCount);
        var currentPose = startToolPose;
        var currentActuators = startActuators;

        for (var sampleIndex = 1; sampleIndex <= sampleCount; sampleIndex++)
        {
            var progress = sampleIndex / (double)sampleCount;
            var nextPose = Interpolate(startToolPose, targetToolPose, progress);
            var nextActuators = kinematics.Inverse(robotProfile, nextPose);
            segments.Add(new DeltaCartesianMotionSegment(
                currentPose,
                nextPose,
                currentActuators,
                nextActuators));
            currentPose = nextPose;
            currentActuators = nextActuators;
        }

        var toolVelocityLimit = CalculateToolLimit(
            segments,
            robotProfile,
            actuator => actuator.MaximumVelocityMillimetersPerSecond);
        var toolAccelerationLimit = CalculateToolLimit(
            segments,
            robotProfile,
            actuator => actuator.MaximumAccelerationMillimetersPerSecondSquared);
        var effectiveToolVelocity = requestedToolVelocityMillimetersPerSecond is { } requested
            ? Math.Min(requested, toolVelocityLimit)
            : toolVelocityLimit;

        return new DeltaCartesianMotionPlan(
            startToolPose,
            targetToolPose,
            distance,
            new TrapezoidalMotionProfile(distance, effectiveToolVelocity, toolAccelerationLimit),
            segments.AsReadOnly());
    }

    public static DeltaToolPose Interpolate(
        DeltaToolPose start,
        DeltaToolPose end,
        double progress)
    {
        var clampedProgress = Math.Clamp(progress, 0, 1);
        return new DeltaToolPose(
            start.XMillimeters + ((end.XMillimeters - start.XMillimeters) * clampedProgress),
            start.YMillimeters + ((end.YMillimeters - start.YMillimeters) * clampedProgress),
            start.ZMillimeters + ((end.ZMillimeters - start.ZMillimeters) * clampedProgress));
    }

    private static double CalculateToolLimit(
        IReadOnlyCollection<DeltaCartesianMotionSegment> segments,
        DeltaRobotProfile profile,
        Func<DeltaActuator, double> selectActuatorLimit)
    {
        var toolLimit = double.PositiveInfinity;

        foreach (var segment in segments)
        {
            var toolDistance = Distance(segment.StartToolPose, segment.EndToolPose);
            foreach (var actuator in profile.Actuators)
            {
                var actuatorDelta = Math.Abs(
                    segment.EndActuators.GetCoordinate(actuator.Id) -
                    segment.StartActuators.GetCoordinate(actuator.Id));
                if (actuatorDelta <= ActuatorToleranceMillimeters)
                {
                    continue;
                }

                toolLimit = Math.Min(
                    toolLimit,
                    selectActuatorLimit(actuator) / (actuatorDelta / toolDistance));
            }
        }

        return double.IsPositiveInfinity(toolLimit)
            ? throw new ImpossibleMovementException(
                "The Delta Cartesian path produced no measurable actuator constraint.")
            : toolLimit;
    }

    private static double Distance(DeltaToolPose start, DeltaToolPose end)
    {
        var deltaX = end.XMillimeters - start.XMillimeters;
        var deltaY = end.YMillimeters - start.YMillimeters;
        var deltaZ = end.ZMillimeters - start.ZMillimeters;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
    }
}
