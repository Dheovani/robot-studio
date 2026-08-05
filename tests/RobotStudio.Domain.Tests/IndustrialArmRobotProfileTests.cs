using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class IndustrialArmRobotProfileTests
{
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

    internal static IndustrialArmRobotProfile CreateProfile() =>
        new(
            baseHeightMillimeters: 100,
            upperArmLengthMillimeters: 180,
            forearmLengthMillimeters: 140,
            wristLengthMillimeters: 80,
            joints:
            [
                new(IndustrialArmJointId.J1Base, -180, 180, 120),
                new(IndustrialArmJointId.J2Shoulder, -120, 120, 100),
                new(IndustrialArmJointId.J3Elbow, -150, 150, 90),
                new(IndustrialArmJointId.J4WristRoll, -180, 180, 160),
                new(IndustrialArmJointId.J5WristPitch, -120, 120, 110),
                new(IndustrialArmJointId.J6ToolRoll, -360, 360, 200)
            ]);
}
