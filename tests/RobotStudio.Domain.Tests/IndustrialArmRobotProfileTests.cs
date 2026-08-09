using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class IndustrialArmRobotProfileTests
{
    [Fact]
    public void Constructor_WhenLinkCollisionRadiusIsInvalid_ShouldThrow()
    {
        var joints = CreateProfile().Joints;

        Assert.Throws<ArgumentException>(() =>
            new IndustrialArmRobotProfile(100, 180, 140, 80, 0, joints));
    }

    [Fact]
    public void ValidatePosition_WhenAllSixJointsAreInsideLimits_ShouldNotThrow()
    {
        CreateProfile().ValidatePosition(new IndustrialArmJointPosition(30, 20, -40, 90, 15, -120));
    }

    [Fact]
    public void ValidatePosition_WhenOneJointExceedsItsLimit_ShouldIdentifyTheJoint()
    {
        var exception = Assert.Throws<InvalidRobotCommandException>(
            () => CreateProfile().ValidatePosition(new IndustrialArmJointPosition(0, 0, 0, 0, 121, 0)));

        Assert.Contains(nameof(IndustrialArmJointId.J5WristPitch), exception.Message);
    }

    [Fact]
    public void JointConstructor_WhenMaximumAccelerationIsNotPositive_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new IndustrialArmJoint(IndustrialArmJointId.J1Base, -180, 180, 120, 0));
    }

    [Fact]
    public void Forward_WhenJointsAreAtHome_ShouldPlaceToolAtHorizontalReach()
    {
        var pose = new IndustrialArmKinematics().Forward(CreateProfile(), IndustrialArmJointPosition.Home);

        Assert.Equal(400, pose.XMillimeters, precision: 6);
        Assert.Equal(0, pose.YMillimeters, precision: 6);
        Assert.Equal(100, pose.ZMillimeters, precision: 6);
        Assert.Equal(0, pose.RollDegrees, precision: 6);
        Assert.Equal(0, pose.PitchDegrees, precision: 6);
        Assert.Equal(0, pose.YawDegrees, precision: 6);
    }

    [Fact]
    public void Constructor_WhenJointDefinitionIsMissing_ShouldThrow()
    {
        var joints = CreateProfile().Joints
            .Where(joint => joint.Id != IndustrialArmJointId.J6ToolRoll)
            .ToArray();

        var exception = Assert.Throws<ArgumentException>(() =>
            new IndustrialArmRobotProfile(100, 180, 140, 80, 12, joints));

        Assert.Contains("each joint J1 through J6 exactly once", exception.Message);
    }

    [Fact]
    public void Constructor_WhenJointDefinitionIsDuplicated_ShouldThrow()
    {
        var joints = CreateProfile().Joints.ToArray();
        joints[^1] = new IndustrialArmJoint(IndustrialArmJointId.J5WristPitch, -120, 120, 110, 220);

        Assert.Throws<ArgumentException>(() =>
            new IndustrialArmRobotProfile(100, 180, 140, 80, 12, joints));
    }

    [Fact]
    public void MaximumJointDeltaTo_ShouldConsiderEveryIndustrialArmJoint()
    {
        var target = new IndustrialArmJointPosition(10, -20, 30, -40, 50, -120);

        var maximumDelta = IndustrialArmJointPosition.Home.MaximumJointDeltaTo(target);

        Assert.Equal(120, maximumDelta);
    }

    internal static IndustrialArmRobotProfile CreateProfile() =>
        new(
            baseHeightMillimeters: 100,
            upperArmLengthMillimeters: 180,
            forearmLengthMillimeters: 140,
            wristLengthMillimeters: 80,
            linkCollisionRadiusMillimeters: 12,
            joints:
            [
                new(IndustrialArmJointId.J1Base, -180, 180, 120, 240),
                new(IndustrialArmJointId.J2Shoulder, -120, 120, 100, 200),
                new(IndustrialArmJointId.J3Elbow, -150, 150, 90, 180),
                new(IndustrialArmJointId.J4WristRoll, -180, 180, 160, 320),
                new(IndustrialArmJointId.J5WristPitch, -120, 120, 110, 220),
                new(IndustrialArmJointId.J6ToolRoll, -360, 360, 200, 400)
            ]);
}
