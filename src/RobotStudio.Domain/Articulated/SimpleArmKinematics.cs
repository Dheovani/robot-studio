namespace RobotStudio.Domain.Articulated;

using RobotStudio.Domain.Exceptions;

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

    public SimpleArmJointPosition InversePositiveBend(
        SimpleArmRobotProfile profile,
        SimpleArmToolPose pose)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (!double.IsFinite(pose.X) ||
            !double.IsFinite(pose.Y) ||
            !double.IsFinite(pose.OrientationDegrees))
        {
            throw new InvalidRobotCommandException(
                "Simple arm target pose coordinates and orientation must be finite values.");
        }

        var orientationRadians = DegreesToRadians(pose.OrientationDegrees);
        var wristX = pose.X - (profile.ThirdLinkLengthMillimeters * Math.Cos(orientationRadians));
        var wristY = pose.Y - (profile.ThirdLinkLengthMillimeters * Math.Sin(orientationRadians));
        var first = profile.FirstLinkLengthMillimeters;
        var second = profile.SecondLinkLengthMillimeters;
        var wristDistanceSquared = (wristX * wristX) + (wristY * wristY);
        var cosShoulder =
            (wristDistanceSquared - (first * first) - (second * second)) /
            (2 * first * second);

        if (cosShoulder < -1 || cosShoulder > 1)
        {
            throw new InvalidRobotCommandException(
                "Simple arm target pose is outside the reachable workspace. " +
                $"X={pose.X:0.###} mm, Y={pose.Y:0.###} mm, A={pose.OrientationDegrees:0.###} deg.");
        }

        var shoulderRadians = Math.Acos(Math.Clamp(cosShoulder, -1, 1));
        var baseRadians = Math.Atan2(wristY, wristX) -
                          Math.Atan2(
                              second * Math.Sin(shoulderRadians),
                              first + (second * Math.Cos(shoulderRadians)));
        var elbowRadians = orientationRadians - baseRadians - shoulderRadians;
        var joints = new SimpleArmJointPosition(
            NormalizeDegrees(RadiansToDegrees(baseRadians)),
            NormalizeDegrees(RadiansToDegrees(shoulderRadians)),
            NormalizeDegrees(RadiansToDegrees(elbowRadians)));

        profile.ValidatePosition(joints);
        return joints;
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;

    private static double RadiansToDegrees(double radians) =>
        radians * 180 / Math.PI;

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
