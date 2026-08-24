using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion;

public sealed class IndustrialArmCartesianMotionPlanner
{
    public const double DefaultMaximumToolSegmentLengthMillimeters = 2;
    public const double DefaultMaximumOrientationSegmentDegrees = 1;

    private const double PositionToleranceMillimeters = 0.000_001;
    private const double JointToleranceDegrees = 0.000_001;

    private readonly IndustrialArmKinematics kinematics;

    public IndustrialArmCartesianMotionPlanner()
        : this(new IndustrialArmKinematics())
    {
    }

    public IndustrialArmCartesianMotionPlanner(IndustrialArmKinematics kinematics)
    {
        ArgumentNullException.ThrowIfNull(kinematics);
        this.kinematics = kinematics;
    }

    public IndustrialArmCartesianMotionPlan PlanLinearMove(
        IndustrialArmJointPosition startJoints,
        IndustrialArmToolPose targetToolPose,
        IndustrialArmRobotProfile robotProfile,
        double? requestedToolVelocityMillimetersPerSecond = null,
        IndustrialArmConfiguration configuration = IndustrialArmConfiguration.PositiveElbowWristNeutral,
        double maximumToolSegmentLengthMillimeters = DefaultMaximumToolSegmentLengthMillimeters,
        double maximumOrientationSegmentDegrees = DefaultMaximumOrientationSegmentDegrees)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedToolVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedToolVelocityMillimetersPerSecond),
                "Requested industrial arm tool velocity must be greater than zero.");
        }

        ValidateSamplingLimit(maximumToolSegmentLengthMillimeters, nameof(maximumToolSegmentLengthMillimeters));
        ValidateSamplingLimit(maximumOrientationSegmentDegrees, nameof(maximumOrientationSegmentDegrees));
        robotProfile.ValidatePosition(startJoints);
        EnsureConfiguration(startJoints, robotProfile, configuration);

        var startPose = kinematics.Forward(robotProfile, startJoints);
        var targetPose = NormalizeOrientation(targetToolPose);
        var distance = Distance(startPose, targetPose);
        var rollTravel = ShortestAngularDelta(startPose.RollDegrees, targetPose.RollDegrees);
        var pitchTravel = ShortestAngularDelta(startPose.PitchDegrees, targetPose.PitchDegrees);
        var yawTravel = ShortestAngularDelta(startPose.YawDegrees, targetPose.YawDegrees);
        var maximumOrientationTravel = new[]
        {
            Math.Abs(rollTravel),
            Math.Abs(pitchTravel),
            Math.Abs(yawTravel)
        }.Max();

        if (distance <= PositionToleranceMillimeters &&
            maximumOrientationTravel <= JointToleranceDegrees)
        {
            return new IndustrialArmCartesianMotionPlan(
                startPose,
                targetPose,
                0,
                0,
                configuration,
                ProgressMotionProfile: null,
                Segments: Array.Empty<IndustrialArmCartesianMotionSegment>());
        }

        var sampleCount = Math.Max(
            1,
            Math.Max(
                (int)Math.Ceiling(distance / maximumToolSegmentLengthMillimeters),
                (int)Math.Ceiling(maximumOrientationTravel / maximumOrientationSegmentDegrees)));
        var segments = new List<IndustrialArmCartesianMotionSegment>(sampleCount);
        var currentProgress = 0d;
        var currentPose = startPose;
        var currentJoints = startJoints;

        for (var sampleIndex = 1; sampleIndex <= sampleCount; sampleIndex++)
        {
            var progress = sampleIndex / (double)sampleCount;
            var nextPose = Interpolate(startPose, targetPose, progress);
            var nextJoints = kinematics.Inverse(robotProfile, nextPose, configuration);
            segments.Add(new IndustrialArmCartesianMotionSegment(
                currentProgress,
                progress,
                currentPose,
                nextPose,
                currentJoints,
                nextJoints));
            currentProgress = progress;
            currentPose = nextPose;
            currentJoints = nextJoints;
        }

        var progressVelocity = CalculateProgressLimit(
            segments,
            robotProfile,
            joint => joint.MaximumVelocityDegreesPerSecond);
        var progressAcceleration = CalculateProgressLimit(
            segments,
            robotProfile,
            joint => joint.MaximumAccelerationDegreesPerSecondSquared);
        if (requestedToolVelocityMillimetersPerSecond is { } requested &&
            distance > PositionToleranceMillimeters)
        {
            progressVelocity = Math.Min(progressVelocity, requested / distance);
        }

        return new IndustrialArmCartesianMotionPlan(
            startPose,
            targetPose,
            distance,
            maximumOrientationTravel,
            configuration,
            new TrapezoidalMotionProfile(1, progressVelocity, progressAcceleration),
            segments.AsReadOnly());
    }

    public static IndustrialArmToolPose Interpolate(
        IndustrialArmToolPose start,
        IndustrialArmToolPose end,
        double progress)
    {
        var t = Math.Clamp(progress, 0, 1);
        return new IndustrialArmToolPose(
            Lerp(start.XMillimeters, end.XMillimeters, t),
            Lerp(start.YMillimeters, end.YMillimeters, t),
            Lerp(start.ZMillimeters, end.ZMillimeters, t),
            InterpolateDegrees(start.RollDegrees, end.RollDegrees, t),
            InterpolateDegrees(start.PitchDegrees, end.PitchDegrees, t),
            InterpolateDegrees(start.YawDegrees, end.YawDegrees, t));
    }

    private void EnsureConfiguration(
        IndustrialArmJointPosition startJoints,
        IndustrialArmRobotProfile profile,
        IndustrialArmConfiguration configuration)
    {
        var pose = kinematics.Forward(profile, startJoints);
        var expected = kinematics.Inverse(profile, pose, configuration);
        if (startJoints.MaximumJointDeltaTo(expected) > JointToleranceDegrees)
        {
            throw new InvalidRobotCommandException(
                "Industrial arm Cartesian G-code currently requires the PositiveElbowWristNeutral configuration. " +
                "Use HOME or a compatible joint command before G1 movement.");
        }
    }

    private static double CalculateProgressLimit(
        IReadOnlyCollection<IndustrialArmCartesianMotionSegment> segments,
        IndustrialArmRobotProfile profile,
        Func<IndustrialArmJoint, double> selectJointLimit)
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
                "The industrial arm Cartesian path produced no measurable joint constraint.")
            : progressLimit;
    }

    private static IndustrialArmToolPose NormalizeOrientation(IndustrialArmToolPose pose) =>
        pose with
        {
            RollDegrees = NormalizeDegrees(pose.RollDegrees),
            PitchDegrees = NormalizeDegrees(pose.PitchDegrees),
            YawDegrees = NormalizeDegrees(pose.YawDegrees)
        };

    private static double Distance(IndustrialArmToolPose start, IndustrialArmToolPose end)
    {
        var x = end.XMillimeters - start.XMillimeters;
        var y = end.YMillimeters - start.YMillimeters;
        var z = end.ZMillimeters - start.ZMillimeters;
        return Math.Sqrt((x * x) + (y * y) + (z * z));
    }

    private static double Lerp(double start, double end, double progress) =>
        start + ((end - start) * progress);

    private static double InterpolateDegrees(double start, double end, double progress) =>
        NormalizeDegrees(start + (ShortestAngularDelta(start, end) * progress));

    private static double ShortestAngularDelta(double start, double end) =>
        NormalizeDegrees(end - start);

    private static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360;
        return normalized switch
        {
            > 180 => normalized - 360,
            < -180 => normalized + 360,
            _ => normalized
        };
    }

    private static void ValidateSamplingLimit(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Industrial arm Cartesian sampling limits must be finite and greater than zero.");
        }
    }
}
