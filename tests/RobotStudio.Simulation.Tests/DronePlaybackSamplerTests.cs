using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Commands;
using RobotStudio.Simulation;

namespace RobotStudio.Simulation.Tests;

public sealed class DronePlaybackSamplerTests
{
    [Fact]
    public void Sample_WhenDroneMoves_ShouldReturnFramesAtFixedIntervals()
    {
        var snapshot = new DronePlaybackSampler()
            .Sample(CreateMoveResult(), TimeSpan.FromMilliseconds(100));

        Assert.True(snapshot.FrameCount > 2);
        Assert.Equal(TimeSpan.Zero, snapshot.Frames[0].Time);
        Assert.True(snapshot.TotalDuration > TimeSpan.Zero);
    }

    [Fact]
    public void Sample_WhenDroneMoves_ShouldInterpolatePoseAndAttitude()
    {
        var snapshot = new DronePlaybackSampler()
            .Sample(CreateMoveResult(), TimeSpan.FromMilliseconds(100));

        Assert.Contains(
            snapshot.Frames,
            frame => frame.Pose.XMillimeters > 0 &&
                     frame.Pose.XMillimeters < 120 &&
                     frame.Pose.RollDegrees > 0 &&
                     frame.Pose.RollDegrees < 20 &&
                     frame.Pose.YawDegrees > 0 &&
                     frame.Pose.YawDegrees < 90);
    }

    [Fact]
    public void Sample_DuringAcceleration_ShouldSynchronizeTranslationAndYawProfiles()
    {
        var result = CreateMoveResult();
        var snapshot = new DronePlaybackSampler().Sample(
            result,
            TimeSpan.FromMilliseconds(100));
        var acceleratingFrame = Assert.Single(
            snapshot.Frames,
            frame => frame.Time == TimeSpan.FromMilliseconds(100));

        Assert.NotNull(result.Timeline[1].TranslationProfile);
        Assert.NotNull(result.Timeline[1].AttitudeProfile);
        Assert.NotNull(result.Timeline[1].YawProfile);
        Assert.InRange(acceleratingFrame.Pose.XMillimeters, 0, 3);
        Assert.InRange(acceleratingFrame.Pose.YawDegrees, 0, 3);
        Assert.InRange(acceleratingFrame.Pose.RollDegrees, 0, 1);
    }

    [Fact]
    public void Sample_WhenDroneMoves_ShouldPreserveCommandMetadata()
    {
        var snapshot = new DronePlaybackSampler()
            .Sample(CreateMoveResult(), TimeSpan.FromMilliseconds(100));

        Assert.Contains(
            snapshot.Frames,
            frame => frame.CommandName == nameof(DroneMoveCommand) &&
                     frame.CommandIndex == 0);
    }

    private static DroneSimulationResult CreateMoveResult()
    {
        var profile = new DroneProfile(
            minimumXMillimeters: 0,
            maximumXMillimeters: 500,
            minimumYMillimeters: 0,
            maximumYMillimeters: 400,
            minimumZMillimeters: 0,
            maximumZMillimeters: 250,
            maximumLinearVelocityMillimetersPerSecond: 180,
            maximumYawVelocityDegreesPerSecond: 120,
            maximumLinearAccelerationMillimetersPerSecondSquared: 360,
            maximumYawAccelerationDegreesPerSecondSquared: 240,
            maximumTiltDegrees: 45,
            maximumAttitudeVelocityDegreesPerSecond: 180,
            maximumAttitudeAccelerationDegreesPerSecondSquared: 360);
        var context = DroneSimulationContext.Create(profile, new DronePose(0, 0, 0, 0));
        var sequence = new RobotCommandSequence(
            [new DroneMoveCommand(
                new DronePose(120, 80, 40, YawDegrees: 90, RollDegrees: 20, PitchDegrees: -10),
                requestedLinearVelocityMillimetersPerSecond: 120,
                requestedYawVelocityDegreesPerSecond: 90,
                requestedAttitudeVelocityDegreesPerSecond: 60)]);

        return new DroneSimulator().Execute(context, sequence);
    }
}
