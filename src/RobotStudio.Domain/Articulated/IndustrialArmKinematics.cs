namespace RobotStudio.Domain.Articulated;

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

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

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
