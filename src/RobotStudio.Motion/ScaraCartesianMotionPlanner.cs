using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion;

public sealed class ScaraCartesianMotionPlanner
{
    public const double DefaultMaximumToolSegmentLengthMillimeters = 2;

    private const double ToolToleranceMillimeters = 0.000_001;
    private const double JointToleranceDegrees = 0.000_001;

    private readonly ScaraKinematics kinematics;
    public ScaraCartesianMotionPlanner()
        : this(new ScaraKinematics())
    {
    }

    public ScaraCartesianMotionPlanner(ScaraKinematics kinematics)
    {
        ArgumentNullException.ThrowIfNull(kinematics);

        this.kinematics = kinematics;
    }

    public ScaraCartesianMotionPlan PlanLinearMove(
        ScaraJointPosition startJoints,
        ScaraToolPose targetToolPose,
        ScaraRobotProfile robotProfile,
        double? requestedToolVelocityMillimetersPerSecond = null,
        double maximumToolSegmentLengthMillimeters = DefaultMaximumToolSegmentLengthMillimeters)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedToolVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedToolVelocityMillimetersPerSecond),
                "Requested SCARA tool velocity must be greater than zero.");
        }

        if (!double.IsFinite(maximumToolSegmentLengthMillimeters) ||
            maximumToolSegmentLengthMillimeters <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumToolSegmentLengthMillimeters),
                "Maximum SCARA tool segment length must be a finite number greater than zero.");
        }

        robotProfile.ValidatePosition(startJoints);
        EnsureElbowDownConfiguration(startJoints, robotProfile);

        var startToolPose = kinematics.Forward(robotProfile, startJoints);
        var distance = Distance(startToolPose, targetToolPose);
        if (distance <= ToolToleranceMillimeters)
        {
            return new ScaraCartesianMotionPlan(
                startToolPose,
                targetToolPose,
                ToolDistanceMillimeters: 0,
                ToolMotionProfile: null,
                Segments: Array.Empty<ScaraCartesianMotionSegment>());
        }

        var sampleCount = Math.Max(
            1,
            (int)Math.Ceiling(distance / maximumToolSegmentLengthMillimeters));
        var segmentDistance = distance / sampleCount;
        var segments = new List<ScaraCartesianMotionSegment>(sampleCount);
        var currentToolPose = startToolPose;
        var currentJoints = startJoints;

        for (var sample = 1; sample <= sampleCount; sample++)
        {
            var progress = sample / (double)sampleCount;
            var nextToolPose = Interpolate(startToolPose, targetToolPose, progress);
            var nextJoints = kinematics.InverseElbowDown(robotProfile, nextToolPose);
            if (currentJoints.MaximumJointDeltaTo(nextJoints) > JointToleranceDegrees)
            {
                segments.Add(new ScaraCartesianMotionSegment(
                    currentToolPose,
                    nextToolPose,
                    currentJoints,
                    nextJoints));
            }

            currentToolPose = nextToolPose;
            currentJoints = nextJoints;
        }

        if (segments.Count == 0)
        {
            throw new ImpossibleMovementException(
                "The SCARA tool movement is greater than zero, but it produced no joint movement.");
        }

        var toolVelocityLimit = CalculateToolVelocityLimit(segments, robotProfile);
        var toolAccelerationLimit = CalculateToolAccelerationLimit(segments, robotProfile);
        var effectiveToolVelocity = requestedToolVelocityMillimetersPerSecond is { } requested
            ? Math.Min(requested, toolVelocityLimit)
            : toolVelocityLimit;

        return new ScaraCartesianMotionPlan(
            startToolPose,
            targetToolPose,
            distance,
            new TrapezoidalMotionProfile(
                distance,
                effectiveToolVelocity,
                toolAccelerationLimit),
            segments.AsReadOnly());
    }

    private void EnsureElbowDownConfiguration(
        ScaraJointPosition startJoints,
        ScaraRobotProfile robotProfile)
    {
        var startToolPose = kinematics.Forward(robotProfile, startJoints);
        var elbowDownJoints = kinematics.InverseElbowDown(robotProfile, startToolPose);
        if (startJoints.MaximumJointDeltaTo(elbowDownJoints) > JointToleranceDegrees)
        {
            throw new InvalidRobotCommandException(
                "SCARA Cartesian G-code currently requires the elbow-down joint configuration. " +
                "Use HOME or an elbow-down SCARA joint command before G1 movement.");
        }
    }

    private static double CalculateToolVelocityLimit(
        IReadOnlyCollection<ScaraCartesianMotionSegment> segments,
        ScaraRobotProfile profile) =>
        CalculateToolLimit(
            segments,
            profile,
            joint => joint.MaximumVelocityDegreesPerSecond);

    private static double CalculateToolAccelerationLimit(
        IReadOnlyCollection<ScaraCartesianMotionSegment> segments,
        ScaraRobotProfile profile) =>
        CalculateToolLimit(
            segments,
            profile,
            joint => joint.MaximumAccelerationDegreesPerSecondSquared);

    private static double CalculateToolLimit(
        IReadOnlyCollection<ScaraCartesianMotionSegment> segments,
        ScaraRobotProfile profile,
        Func<ScaraJoint, double> selectJointLimit)
    {
        var toolLimit = double.PositiveInfinity;

        foreach (var segment in segments)
        {
            var toolDistance = Distance(segment.StartToolPose, segment.EndToolPose);
            foreach (var joint in profile.Joints)
            {
                var jointDelta = Math.Abs(
                    segment.EndJoints.GetCoordinate(joint.Id) -
                    segment.StartJoints.GetCoordinate(joint.Id));
                if (jointDelta <= JointToleranceDegrees)
                {
                    continue;
                }

                var jointDegreesPerMillimeter = jointDelta / toolDistance;
                toolLimit = Math.Min(
                    toolLimit,
                    selectJointLimit(joint) / jointDegreesPerMillimeter);
            }
        }

        return double.IsPositiveInfinity(toolLimit)
            ? throw new ImpossibleMovementException(
                "The SCARA Cartesian path produced no measurable joint constraint.")
            : toolLimit;
    }

    private static ScaraToolPose Interpolate(
        ScaraToolPose start,
        ScaraToolPose end,
        double progress) =>
        new(
            start.X + ((end.X - start.X) * progress),
            start.Y + ((end.Y - start.Y) * progress));

    private static double Distance(ScaraToolPose start, ScaraToolPose end)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }
}
