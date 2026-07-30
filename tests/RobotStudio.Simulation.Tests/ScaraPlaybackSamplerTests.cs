using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class ScaraPlaybackSamplerTests
{
    [Fact]
    public void Sample_ShouldReturnFramesAtFixedInterval()
    {
        var result = CreateMoveResult();
        var sampler = new ScaraPlaybackSampler();

        var snapshot = sampler.Sample(result, TimeSpan.FromMilliseconds(100));

        Assert.True(snapshot.FrameCount > 2);
        Assert.Equal(TimeSpan.Zero, snapshot.Frames[0].Time);
        Assert.Equal(result.FinalContext.ElapsedTime, snapshot.Frames[^1].Time);
    }

    [Fact]
    public void Sample_ShouldInterpolateJointPosition()
    {
        var result = CreateMoveResult();
        var sampler = new ScaraPlaybackSampler();

        var snapshot = sampler.Sample(result, TimeSpan.FromMilliseconds(100));

        Assert.Contains(
            snapshot.Frames,
            frame => frame.Joints.ShoulderDegrees > 0 && frame.Joints.ShoulderDegrees < 60);
    }

    [Fact]
    public void Sample_ShouldPreserveCommandMetadata()
    {
        var result = CreateMoveResult();
        var sampler = new ScaraPlaybackSampler();

        var snapshot = sampler.Sample(result, TimeSpan.FromMilliseconds(100));

        Assert.Contains(
            snapshot.Frames,
            frame => frame.CommandName == nameof(ScaraMoveJointsCommand) &&
                     frame.CommandIndex == 0);
    }

    [Fact]
    public void Sample_Throws_WhenIntervalIsNotPositive()
    {
        var result = CreateMoveResult();
        var sampler = new ScaraPlaybackSampler();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sampler.Sample(result, TimeSpan.Zero));
    }

    private static ScaraSimulationResult CreateMoveResult()
    {
        var profile = new ScaraRobotProfile(
            firstLinkLengthMillimeters: 180,
            secondLinkLengthMillimeters: 120,
            shoulderJoint: new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120),
            elbowJoint: new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100));
        var context = ScaraSimulationContext.Create(
            profile,
            new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0));
        var sequence = new RobotCommandSequence(
        [
            new ScaraMoveJointsCommand(
                new ScaraJointPosition(ShoulderDegrees: 60, ElbowDegrees: 30),
                requestedJointVelocityDegreesPerSecond: 60)
        ]);

        return new ScaraSimulator().Execute(context, sequence);
    }
}
