using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class SimpleArmRobotProfileTests
{
    [Fact]
    public void Constructor_WhenLinkCollisionRadiusIsInvalid_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => CreateProfile(linkCollisionRadius: double.NaN));
    }

    [Fact]
    public void ValidatePosition_WhenJointsAreInsideLimits_ShouldNotThrow()
    {
        var profile = CreateProfile();
        var position = new SimpleArmJointPosition(BaseDegrees: 30, ShoulderDegrees: 45, ElbowDegrees: -20);

        profile.ValidatePosition(position);
    }

    [Fact]
    public void ValidatePosition_WhenJointIsOutsideLimits_ShouldThrow()
    {
        var profile = CreateProfile();
        var position = new SimpleArmJointPosition(BaseDegrees: 181, ShoulderDegrees: 45, ElbowDegrees: -20);

        Assert.Throws<InvalidRobotCommandException>(() => profile.ValidatePosition(position));
    }

    [Fact]
    public void JointConstructor_WhenMaximumAccelerationIsNotPositive_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new SimpleArmJoint(SimpleArmJointId.Base, -180, 180, 100, 0));
    }

    [Fact]
    public void Forward_WhenAllJointsAreZero_ShouldPlaceToolAtTotalReach()
    {
        var profile = CreateProfile();
        var kinematics = new SimpleArmKinematics();

        var pose = kinematics.Forward(profile, new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0));

        Assert.Equal(270, pose.X, precision: 6);
        Assert.Equal(0, pose.Y, precision: 6);
        Assert.Equal(0, pose.OrientationDegrees, precision: 6);
    }

    [Fact]
    public void Forward_WhenAnglesAccumulate_ShouldCalculateToolPose()
    {
        var profile = CreateProfile();
        var kinematics = new SimpleArmKinematics();

        var pose = kinematics.Forward(profile, new SimpleArmJointPosition(BaseDegrees: 90, ShoulderDegrees: -90, ElbowDegrees: 90));

        Assert.Equal(90, pose.X, precision: 6);
        Assert.Equal(180, pose.Y, precision: 6);
        Assert.Equal(90, pose.OrientationDegrees, precision: 6);
    }

    [Fact]
    public void InversePositiveBend_WhenPoseComesFromPositiveBendJoints_ShouldRecoverJoints()
    {
        var profile = CreateProfile();
        var kinematics = new SimpleArmKinematics();
        var expected = new SimpleArmJointPosition(10, 60, -30);

        var result = kinematics.InversePositiveBend(
            profile,
            kinematics.Forward(profile, expected));

        Assert.Equal(expected.BaseDegrees, result.BaseDegrees, precision: 6);
        Assert.Equal(expected.ShoulderDegrees, result.ShoulderDegrees, precision: 6);
        Assert.Equal(expected.ElbowDegrees, result.ElbowDegrees, precision: 6);
    }

    [Fact]
    public void InversePositiveBend_WhenPoseIsUnreachable_ShouldThrow()
    {
        Assert.Throws<InvalidRobotCommandException>(() =>
            new SimpleArmKinematics().InversePositiveBend(
                CreateProfile(),
                new SimpleArmToolPose(500, 0, 0)));
    }

    [Fact]
    public void Validate_WhenSimpleArmCommandTargetIsOutsideLimits_ShouldThrow()
    {
        var profile = CreateProfile();
        var command = new SimpleArmMoveJointsCommand(
            new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 121, ElbowDegrees: 0));

        Assert.Throws<InvalidRobotCommandException>(() => RobotCommandValidator.Validate(command, profile));
    }

    private static SimpleArmRobotProfile CreateProfile(double linkCollisionRadius = 10) =>
        new(
            firstLinkLengthMillimeters: 120,
            secondLinkLengthMillimeters: 90,
            thirdLinkLengthMillimeters: 60,
            linkCollisionRadiusMillimeters: linkCollisionRadius,
            baseJoint: new SimpleArmJoint(SimpleArmJointId.Base, -180, 180, 100, 200),
            shoulderJoint: new SimpleArmJoint(SimpleArmJointId.Shoulder, -120, 120, 90, 180),
            elbowJoint: new SimpleArmJoint(SimpleArmJointId.Elbow, -150, 150, 80, 160));
}
