using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion;

public sealed class SimpleArmCartesianMotionPlanner
{
    public const double DefaultMaximumToolSegmentLengthMillimeters = 2;
    public const double DefaultMaximumOrientationSegmentDegrees = 1;

    private const double ToolToleranceMillimeters = 0.000_001;
    private const double JointToleranceDegrees = 0.000_001;

    private readonly SimpleArmKinematics kinematics;

    public SimpleArmCartesianMotionPlanner()
        : this(new SimpleArmKinematics())
    {
    }

    public SimpleArmCartesianMotionPlanner(SimpleArmKinematics kinematics)
    {
        ArgumentNullException.ThrowIfNull(kinematics);
        this.kinematics = kinematics;
    }

    public SimpleArmCartesianMotionPlan PlanLinearMove(
        SimpleArmJointPosition startJoints,
        SimpleArmToolPose targetToolPose,
        SimpleArmRobotProfile robotProfile,
        double? requestedToolVelocityMillimetersPerSecond = null,
        double maximumToolSegmentLengthMillimeters = DefaultMaximumToolSegmentLengthMillimeters,
        double maximumOrientationSegmentDegrees = DefaultMaximumOrientationSegmentDegrees)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedToolVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedToolVelocityMillimetersPerSecond),
                "Requested Simple Arm tool velocity must be greater than zero.");
        }

        ValidateSamplingLimit(maximumToolSegmentLengthMillimeters, nameof(maximumToolSegmentLengthMillimeters));
        ValidateSamplingLimit(maximumOrientationSegmentDegrees, nameof(maximumOrientationSegmentDegrees));
        robotProfile.ValidatePosition(startJoints);
        EnsurePositiveBendConfiguration(startJoints, robotProfile);

        var startToolPose = kinematics.Forward(robotProfile, startJoints);
        var normalizedTarget = targetToolPose with
        {
            OrientationDegrees = NormalizeDegrees(targetToolPose.OrientationDegrees)
        };
        var distance = Distance(startToolPose, normalizedTarget);
        var orientationTravel = ShortestAngularDelta(
            startToolPose.OrientationDegrees,
            normalizedTarget.OrientationDegrees);

        if (distance <= ToolToleranceMillimeters && Math.Abs(orientationTravel) <= JointToleranceDegrees)
        {
            return new SimpleArmCartesianMotionPlan(
                startToolPose,
                normalizedTarget,
                0,
                0,
                ProgressMotionProfile: null,
                Segments: Array.Empty<SimpleArmCartesianMotionSegment>());
        }

        var sampleCount = Math.Max(
            1,
            Math.Max(
                (int)Math.Ceiling(distance / maximumToolSegmentLengthMillimeters),
                (int)Math.Ceiling(Math.Abs(orientationTravel) / maximumOrientationSegmentDegrees)));
        var segments = new List<SimpleArmCartesianMotionSegment>(sampleCount);
        var currentPose = startToolPose;
        var currentJoints = startJoints;
        var currentProgress = 0d;

        for (var sampleIndex = 1; sampleIndex <= sampleCount; sampleIndex++)
        {
            var progress = sampleIndex / (double)sampleCount;
            var nextPose = Interpolate(startToolPose, normalizedTarget, orientationTravel, progress);
            var nextJoints = kinematics.InversePositiveBend(robotProfile, nextPose);
            segments.Add(new SimpleArmCartesianMotionSegment(
                currentProgress,
                progress,
                currentPose,
                nextPose,
                currentJoints,
                nextJoints));
            currentPose = nextPose;
            currentJoints = nextJoints;
            currentProgress = progress;
        }

        var maximumProgressVelocity = CalculateProgressLimit(
            segments,
            robotProfile,
            joint => joint.MaximumVelocityDegreesPerSecond);
        var maximumProgressAcceleration = CalculateProgressLimit(
            segments,
            robotProfile,
            joint => joint.MaximumAccelerationDegreesPerSecondSquared);

        if (requestedToolVelocityMillimetersPerSecond is { } requested && distance > ToolToleranceMillimeters)
        {
            maximumProgressVelocity = Math.Min(maximumProgressVelocity, requested / distance);
        }

        return new SimpleArmCartesianMotionPlan(
            startToolPose,
            normalizedTarget,
            distance,
            Math.Abs(orientationTravel),
            new TrapezoidalMotionProfile(
                distance: 1,
                maximumProgressVelocity,
                maximumProgressAcceleration),
            segments.AsReadOnly());
    }

    public static SimpleArmToolPose Interpolate(
        SimpleArmToolPose start,
        SimpleArmToolPose end,
        double progress)
    {
        var orientationTravel = ShortestAngularDelta(
            start.OrientationDegrees,
            end.OrientationDegrees);
        return Interpolate(start, end, orientationTravel, Math.Clamp(progress, 0, 1));
    }

    private void EnsurePositiveBendConfiguration(
        SimpleArmJointPosition startJoints,
        SimpleArmRobotProfile robotProfile)
    {
        var startPose = kinematics.Forward(robotProfile, startJoints);
        var expectedJoints = kinematics.InversePositiveBend(robotProfile, startPose);
        if (startJoints.MaximumJointDeltaTo(expectedJoints) > JointToleranceDegrees)
        {
            throw new InvalidRobotCommandException(
                "Simple Arm Cartesian G-code currently requires the deterministic positive-bend configuration. " +
                "Use HOME or a compatible joint command before G1 movement.");
        }
    }

    private static double CalculateProgressLimit(
        IReadOnlyCollection<SimpleArmCartesianMotionSegment> segments,
        SimpleArmRobotProfile profile,
        Func<SimpleArmJoint, double> selectJointLimit)
    {
        var progressLimit = double.PositiveInfinity;

        foreach (var segment in segments)
        {
            var progressDelta = segment.EndProgress - segment.StartProgress;
            foreach (var joint in profile.Joints)
            {
                var jointDelta = Math.Abs(
                    segment.EndJoints.GetCoordinate(joint.Id) -
                    segment.StartJoints.GetCoordinate(joint.Id));
                if (jointDelta <= JointToleranceDegrees)
                {
                    continue;
                }

                progressLimit = Math.Min(
                    progressLimit,
                    selectJointLimit(joint) / (jointDelta / progressDelta));
            }
        }

        return double.IsPositiveInfinity(progressLimit)
            ? throw new ImpossibleMovementException(
                "The Simple Arm Cartesian path produced no measurable joint constraint.")
            : progressLimit;
    }

    private static SimpleArmToolPose Interpolate(
        SimpleArmToolPose start,
        SimpleArmToolPose end,
        double orientationTravel,
        double progress) =>
        new(
            start.X + ((end.X - start.X) * progress),
            start.Y + ((end.Y - start.Y) * progress),
            NormalizeDegrees(start.OrientationDegrees + (orientationTravel * progress)));

    private static double Distance(SimpleArmToolPose start, SimpleArmToolPose end)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }

    private static double ShortestAngularDelta(double start, double end) =>
        NormalizeDegrees(end - start);

    private static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360;
        if (normalized > 180)
        {
            normalized -= 360;
        }

        if (normalized < -180)
        {
            normalized += 360;
        }

        return normalized;
    }

    private static void ValidateSamplingLimit(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Simple Arm Cartesian sampling limits must be finite values greater than zero.");
        }
    }
}
