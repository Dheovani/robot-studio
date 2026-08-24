namespace RobotStudio.Domain.Articulated;

using RobotStudio.Domain.Exceptions;

public sealed class IndustrialArmKinematics
{
    public IndustrialArmToolPose Forward(
        IndustrialArmRobotProfile profile,
        IndustrialArmJointPosition joints)
    {
        ArgumentNullException.ThrowIfNull(profile);
        profile.ValidatePosition(joints);

        var yaw = DegreesToRadians(joints.J1Degrees);
        var shoulder = DegreesToRadians(joints.J2Degrees);
        var elbow = shoulder + DegreesToRadians(joints.J3Degrees);
        var wristPitch = elbow + DegreesToRadians(joints.J5Degrees);

        var radialDistance =
            (profile.UpperArmLengthMillimeters * Math.Cos(shoulder)) +
            (profile.ForearmLengthMillimeters * Math.Cos(elbow)) +
            (profile.WristLengthMillimeters * Math.Cos(wristPitch));
        var height = profile.BaseHeightMillimeters +
            (profile.UpperArmLengthMillimeters * Math.Sin(shoulder)) +
            (profile.ForearmLengthMillimeters * Math.Sin(elbow)) +
            (profile.WristLengthMillimeters * Math.Sin(wristPitch));

        return new IndustrialArmToolPose(
            radialDistance * Math.Cos(yaw),
            radialDistance * Math.Sin(yaw),
            height,
            NormalizeDegrees(joints.J4Degrees + joints.J6Degrees),
            NormalizeDegrees(joints.J2Degrees + joints.J3Degrees + joints.J5Degrees),
            NormalizeDegrees(joints.J1Degrees));
    }

    public IndustrialArmJointPosition Inverse(
        IndustrialArmRobotProfile profile,
        IndustrialArmToolPose pose,
        IndustrialArmConfiguration configuration = IndustrialArmConfiguration.PositiveElbowWristNeutral)
    {
        ArgumentNullException.ThrowIfNull(profile);
        EnsureFinite(pose);

        if (configuration != IndustrialArmConfiguration.PositiveElbowWristNeutral)
        {
            throw new ArgumentOutOfRangeException(
                nameof(configuration),
                configuration,
                "Unsupported industrial arm configuration.");
        }

        var yawDegrees = NormalizeDegrees(pose.YawDegrees);
        var yawRadians = DegreesToRadians(yawDegrees);
        var radialDistance = Math.Sqrt(
            (pose.XMillimeters * pose.XMillimeters) +
            (pose.YMillimeters * pose.YMillimeters));
        var expectedX = radialDistance * Math.Cos(yawRadians);
        var expectedY = radialDistance * Math.Sin(yawRadians);
        if (Math.Abs(expectedX - pose.XMillimeters) > 0.000_1 ||
            Math.Abs(expectedY - pose.YMillimeters) > 0.000_1)
        {
            throw new InvalidRobotCommandException(
                "The introductory 6-DOF arm couples TCP yaw C to the J1 base azimuth. " +
                $"Position X={pose.XMillimeters:0.###}, Y={pose.YMillimeters:0.###} is incompatible with C={pose.YawDegrees:0.###} deg.");
        }

        var pitchDegrees = NormalizeDegrees(pose.PitchDegrees);
        var pitchRadians = DegreesToRadians(pitchDegrees);
        var wristRadial = radialDistance -
            (profile.WristLengthMillimeters * Math.Cos(pitchRadians));
        var wristHeight = pose.ZMillimeters - profile.BaseHeightMillimeters -
            (profile.WristLengthMillimeters * Math.Sin(pitchRadians));
        var upper = profile.UpperArmLengthMillimeters;
        var forearm = profile.ForearmLengthMillimeters;
        var wristDistanceSquared = (wristRadial * wristRadial) + (wristHeight * wristHeight);
        var cosElbow =
            (wristDistanceSquared - (upper * upper) - (forearm * forearm)) /
            (2 * upper * forearm);

        if (cosElbow < -1 || cosElbow > 1)
        {
            throw new InvalidRobotCommandException(
                "Industrial arm target pose is outside the reachable workspace. " +
                $"X={pose.XMillimeters:0.###}, Y={pose.YMillimeters:0.###}, Z={pose.ZMillimeters:0.###} mm.");
        }

        var elbowRadians = Math.Acos(Math.Clamp(cosElbow, -1, 1));
        var shoulderRadians = Math.Atan2(wristHeight, wristRadial) -
                              Math.Atan2(
                                  forearm * Math.Sin(elbowRadians),
                                  upper + (forearm * Math.Cos(elbowRadians)));
        var elbowDegrees = RadiansToDegrees(elbowRadians);
        var shoulderDegrees = RadiansToDegrees(shoulderRadians);
        var wristPitchDegrees = NormalizeDegrees(
            pitchDegrees - shoulderDegrees - elbowDegrees);
        var joints = new IndustrialArmJointPosition(
            yawDegrees,
            NormalizeDegrees(shoulderDegrees),
            NormalizeDegrees(elbowDegrees),
            J4Degrees: 0,
            wristPitchDegrees,
            NormalizeDegrees(pose.RollDegrees));

        profile.ValidatePosition(joints);
        return joints;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    private static double RadiansToDegrees(double radians) => radians * 180 / Math.PI;

    private static void EnsureFinite(IndustrialArmToolPose pose)
    {
        if (!double.IsFinite(pose.XMillimeters) ||
            !double.IsFinite(pose.YMillimeters) ||
            !double.IsFinite(pose.ZMillimeters) ||
            !double.IsFinite(pose.RollDegrees) ||
            !double.IsFinite(pose.PitchDegrees) ||
            !double.IsFinite(pose.YawDegrees))
        {
            throw new InvalidRobotCommandException(
                "Industrial arm target pose coordinates and orientation must be finite values.");
        }
    }

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
}
