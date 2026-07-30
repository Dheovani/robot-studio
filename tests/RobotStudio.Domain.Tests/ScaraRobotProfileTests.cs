using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class ScaraRobotProfileTests
{
    [Fact]
    public void ValidatePosition_DoesNotThrow_WhenJointsAreInsideLimits()
    {
        var profile = CreateProfile();
        var position = new ScaraJointPosition(ShoulderDegrees: 30, ElbowDegrees: 45);

        var exception = Record.Exception(() => profile.ValidatePosition(position));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidatePosition_Throws_WhenShoulderIsOutsideLimits()
    {
        var profile = CreateProfile();
        var position = new ScaraJointPosition(ShoulderDegrees: 181, ElbowDegrees: 45);

        var exception = Assert.Throws<InvalidRobotCommandException>(() => profile.ValidatePosition(position));

        Assert.Contains("Shoulder", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Forward_ShouldCalculateToolPoseFromJointPosition()
    {
        var profile = CreateProfile();
        var kinematics = new ScaraKinematics();

        var pose = kinematics.Forward(profile, new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0));

        Assert.Equal(300, pose.X, precision: 6);
        Assert.Equal(0, pose.Y, precision: 6);
    }

    [Fact]
    public void InverseElbowDown_ShouldCalculateReachableJointPosition()
    {
        var profile = CreateProfile();
        var kinematics = new ScaraKinematics();

        var joints = kinematics.InverseElbowDown(profile, new ScaraToolPose(X: 100, Y: 200));
        var pose = kinematics.Forward(profile, joints);

        Assert.Equal(100, pose.X, precision: 6);
        Assert.Equal(200, pose.Y, precision: 6);
    }

    [Fact]
    public void InverseElbowDown_Throws_WhenPoseIsOutsideReachableWorkspace()
    {
        var profile = CreateProfile();
        var kinematics = new ScaraKinematics();

        var exception = Assert.Throws<InvalidRobotCommandException>(() =>
            kinematics.InverseElbowDown(profile, new ScaraToolPose(X: 400, Y: 0)));

        Assert.Contains("outside the reachable workspace", exception.Message, StringComparison.Ordinal);
    }

    private static ScaraRobotProfile CreateProfile() =>
        new(
            firstLinkLengthMillimeters: 180,
            secondLinkLengthMillimeters: 120,
            shoulderJoint: new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120),
            elbowJoint: new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100));
}
