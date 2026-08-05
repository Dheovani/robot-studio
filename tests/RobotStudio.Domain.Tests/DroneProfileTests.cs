using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class DroneProfileTests
{
    [Fact]
    public void ValidatePosition_WhenPoseIsInsideFlightVolume_ShouldNotThrow()
    {
        var profile = CreateProfile();
        var pose = new DronePose(
            XMillimeters: 120,
            YMillimeters: 80,
            ZMillimeters: 40,
            YawDegrees: 450);

        profile.ValidatePosition(pose);
    }

    [Fact]
    public void ValidatePosition_WhenZIsOutsideFlightVolume_ShouldThrow()
    {
        var profile = CreateProfile();
        var pose = new DronePose(
            XMillimeters: 120,
            YMillimeters: 80,
            ZMillimeters: 251,
            YawDegrees: 0);

        Assert.Throws<PositionOutOfRangeException>(() => profile.ValidatePosition(pose));
    }

    [Fact]
    public void AngularDistanceDegreesTo_WhenYawCrossesZero_ShouldUseShortestRotation()
    {
        var start = new DronePose(0, 0, 0, YawDegrees: 350);
        var end = new DronePose(0, 0, 0, YawDegrees: 10);

        Assert.Equal(20, start.AngularDistanceDegreesTo(end));
    }

    [Fact]
    public void Validate_WhenDroneCommandTargetIsOutsideLimits_ShouldThrow()
    {
        var profile = CreateProfile();
        var command = new DroneMoveCommand(
            new DronePose(
                XMillimeters: 0,
                YMillimeters: 0,
                ZMillimeters: 251,
                YawDegrees: 0));

        Assert.Throws<PositionOutOfRangeException>(() => RobotCommandValidator.Validate(command, profile));
    }

    private static DroneProfile CreateProfile() =>
        new(
            minimumXMillimeters: 0,
            maximumXMillimeters: 500,
            minimumYMillimeters: 0,
            maximumYMillimeters: 400,
            minimumZMillimeters: 0,
            maximumZMillimeters: 250,
            maximumLinearVelocityMillimetersPerSecond: 180,
            maximumYawVelocityDegreesPerSecond: 120);
}
