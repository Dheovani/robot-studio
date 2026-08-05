using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Domain.Tests;

public sealed class DeltaRobotProfileTests
{
    [Fact]
    public void ValidatePosition_WhenActuatorsAreInsideLimits_ShouldNotThrow()
    {
        var profile = CreateProfile();
        var position = new DeltaActuatorPosition(AMillimeters: 20, BMillimeters: 40, CMillimeters: 60);

        var exception = Record.Exception(() => profile.ValidatePosition(position));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidatePosition_WhenActuatorIsOutsideLimits_ShouldThrow()
    {
        var profile = CreateProfile();
        var position = new DeltaActuatorPosition(AMillimeters: 20, BMillimeters: 181, CMillimeters: 60);

        Assert.Throws<InvalidRobotCommandException>(() =>
            profile.ValidatePosition(position));
    }

    [Fact]
    public void Forward_ShouldMapActuatorDifferencesToToolPose()
    {
        var kinematics = new DeltaKinematics();
        var profile = CreateProfile();

        var pose = kinematics.Forward(profile, new DeltaActuatorPosition(AMillimeters: 30, BMillimeters: 60, CMillimeters: 0));

        Assert.True(pose.XMillimeters > 0);
        Assert.Equal(0, pose.YMillimeters, precision: 6);
        Assert.Equal(-30, pose.ZMillimeters, precision: 6);
    }

    [Fact]
    public void Validate_WhenDeltaCommandTargetIsOutsideLimits_ShouldThrow()
    {
        var profile = CreateProfile();
        var command = new DeltaMoveActuatorsCommand(
            new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 181, CMillimeters: 0));

        Assert.Throws<InvalidRobotCommandException>(() =>
            RobotCommandValidator.Validate(command, profile));
    }

    private static DeltaRobotProfile CreateProfile() =>
        new(
            baseRadiusMillimeters: 140,
            toolZOffsetMillimeters: 0,
            actuatorA: new DeltaActuator(DeltaActuatorId.A, 0, 180, 120),
            actuatorB: new DeltaActuator(DeltaActuatorId.B, 0, 180, 100),
            actuatorC: new DeltaActuator(DeltaActuatorId.C, 0, 180, 90));
}
