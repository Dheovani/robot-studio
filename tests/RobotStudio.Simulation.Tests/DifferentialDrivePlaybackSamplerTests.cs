using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation.Tests;

public sealed class DifferentialDrivePlaybackSamplerTests
{
    [Fact]
    public void Sample_ShouldReturnFramesAtFixedInterval()
    {
        var result = CreateMoveResult();
        var sampler = new DifferentialDrivePlaybackSampler();

        var snapshot = sampler.Sample(result, TimeSpan.FromMilliseconds(100));

        Assert.True(snapshot.FrameCount > 2);
        Assert.Equal(TimeSpan.Zero, snapshot.Frames[0].Time);
        Assert.Equal(result.FinalContext.ElapsedTime, snapshot.Frames[^1].Time);
    }

    [Fact]
    public void Sample_ShouldInterpolatePosition()
    {
        var result = CreateMoveResult();
        var sampler = new DifferentialDrivePlaybackSampler();

        var snapshot = sampler.Sample(result, TimeSpan.FromMilliseconds(100));

        Assert.Contains(snapshot.Frames, frame => frame.Pose.X > 0 && frame.Pose.X < 100);
    }

    [Fact]
    public void Sample_DuringLinearAcceleration_ShouldUseTranslationProfile()
    {
        var snapshot = new DifferentialDrivePlaybackSampler().Sample(
            CreateMoveResult(),
            TimeSpan.FromMilliseconds(100));
        var acceleratingFrame = Assert.Single(
            snapshot.Frames,
            frame => frame.Time == TimeSpan.FromMilliseconds(100));

        Assert.Equal(2.5, acceleratingFrame.Pose.X, precision: 6);
        Assert.Equal(0, acceleratingFrame.Pose.HeadingDegrees, precision: 6);
        Assert.Equal(2.5, acceleratingFrame.Odometry.LeftWheelTravelMillimeters, precision: 6);
        Assert.Equal(2.5, acceleratingFrame.Odometry.RightWheelTravelMillimeters, precision: 6);
    }

    [Fact]
    public void Sample_WhenTranslationCompletes_ShouldRotateWithoutChangingPosition()
    {
        var result = CreateMoveResult();
        var snapshot = new DifferentialDrivePlaybackSampler().Sample(
            result,
            TimeSpan.FromMilliseconds(100));
        var rotationStart = result.Timeline[2];

        Assert.Equal(new DifferentialDrivePose(100, 0, 0), rotationStart.Pose);
        Assert.Contains(
            snapshot.Frames,
            frame => frame.Time > rotationStart.Time &&
                     frame.Pose.X == 100 &&
                     frame.Pose.HeadingDegrees is > 0 and < 90);
    }

    [Fact]
    public void Sample_ShouldPreserveMetadata()
    {
        var result = CreateMoveResult();
        var sampler = new DifferentialDrivePlaybackSampler();

        var snapshot = sampler.Sample(result, TimeSpan.FromMilliseconds(100));

        Assert.Contains(
            snapshot.Frames,
            frame => frame.CommandName == nameof(DifferentialDriveMoveCommand) &&
                     frame.CommandIndex == 0);
    }

    [Fact]
    public void Sample_Throws_WhenIntervalIsNotPositive()
    {
        var result = CreateMoveResult();
        var sampler = new DifferentialDrivePlaybackSampler();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sampler.Sample(result, TimeSpan.Zero));
    }

    private static DifferentialDriveSimulationResult CreateMoveResult()
    {
        var profile = new DifferentialDriveProfile(
            minimumXMillimeters: 0,
            maximumXMillimeters: 500,
            minimumYMillimeters: 0,
            maximumYMillimeters: 400,
            wheelBaseMillimeters: 120,
            wheelRadiusMillimeters: 30,
            maximumLinearVelocityMillimetersPerSecond: 250,
            maximumAngularVelocityDegreesPerSecond: 180,
            maximumLinearAccelerationMillimetersPerSecondSquared: 500,
            maximumAngularAccelerationDegreesPerSecondSquared: 360);
        var context = DifferentialDriveSimulationContext.Create(
            profile,
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0));
        var sequence = new RobotCommandSequence(
        [
            new DifferentialDriveMoveCommand(
                new DifferentialDrivePose(X: 100, Y: 0, HeadingDegrees: 90),
                requestedLinearVelocityMillimetersPerSecond: 100,
                requestedAngularVelocityDegreesPerSecond: 90)
        ]);

        return new DifferentialDriveSimulator().Execute(context, sequence);
    }
}
