using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Articulated;

public sealed class ScaraKinematics
{
    public ScaraToolPose Forward(
        ScaraRobotProfile profile,
        ScaraJointPosition joints)
    {
        ArgumentNullException.ThrowIfNull(profile);
        profile.ValidatePosition(joints);

        var shoulderRadians = DegreesToRadians(joints.ShoulderDegrees);
        var elbowRadians = DegreesToRadians(joints.ElbowDegrees);
        var secondLinkAngle = shoulderRadians + elbowRadians;

        return new ScaraToolPose(
            (profile.FirstLinkLengthMillimeters * Math.Cos(shoulderRadians)) +
            (profile.SecondLinkLengthMillimeters * Math.Cos(secondLinkAngle)),
            (profile.FirstLinkLengthMillimeters * Math.Sin(shoulderRadians)) +
            (profile.SecondLinkLengthMillimeters * Math.Sin(secondLinkAngle)));
    }

    public ScaraJointPosition InverseElbowDown(
        ScaraRobotProfile profile,
        ScaraToolPose pose)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var distanceSquared = (pose.X * pose.X) + (pose.Y * pose.Y);
        var first = profile.FirstLinkLengthMillimeters;
        var second = profile.SecondLinkLengthMillimeters;
        var cosElbow = (distanceSquared - (first * first) - (second * second)) / (2 * first * second);

        if (cosElbow < -1 || cosElbow > 1)
        {
            throw new InvalidRobotCommandException(
                $"SCARA target pose is outside the reachable workspace. X={pose.X:0.###} mm, Y={pose.Y:0.###} mm.");
        }

        var elbowRadians = Math.Acos(cosElbow);
        var shoulderRadians = Math.Atan2(pose.Y, pose.X) -
                              Math.Atan2(second * Math.Sin(elbowRadians), first + (second * Math.Cos(elbowRadians)));
        var joints = new ScaraJointPosition(
            RadiansToDegrees(shoulderRadians),
            RadiansToDegrees(elbowRadians));

        profile.ValidatePosition(joints);
        return joints;
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;

    private static double RadiansToDegrees(double radians) =>
        radians * 180 / Math.PI;
}
