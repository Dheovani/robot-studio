namespace RobotStudio.Domain.Articulated;

public sealed class SimpleArmKinematics
{
    public SimpleArmToolPose Forward(
        SimpleArmRobotProfile profile,
        SimpleArmJointPosition joints)
    {
        ArgumentNullException.ThrowIfNull(profile);
        profile.ValidatePosition(joints);

        var baseRadians = DegreesToRadians(joints.BaseDegrees);
        var secondLinkAngle = baseRadians + DegreesToRadians(joints.ShoulderDegrees);
        var thirdLinkAngle = secondLinkAngle + DegreesToRadians(joints.ElbowDegrees);

        var x = (profile.FirstLinkLengthMillimeters * Math.Cos(baseRadians)) +
                (profile.SecondLinkLengthMillimeters * Math.Cos(secondLinkAngle)) +
                (profile.ThirdLinkLengthMillimeters * Math.Cos(thirdLinkAngle));
        var y = (profile.FirstLinkLengthMillimeters * Math.Sin(baseRadians)) +
                (profile.SecondLinkLengthMillimeters * Math.Sin(secondLinkAngle)) +
                (profile.ThirdLinkLengthMillimeters * Math.Sin(thirdLinkAngle));

        return new SimpleArmToolPose(
            x,
            y,
            NormalizeDegrees(joints.BaseDegrees + joints.ShoulderDegrees + joints.ElbowDegrees));
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;

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
}
