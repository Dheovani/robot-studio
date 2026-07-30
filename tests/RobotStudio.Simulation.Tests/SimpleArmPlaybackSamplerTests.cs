using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class SimpleArmPlaybackSamplerTests
{
    [Fact]
    public void Sample_WhenMovementHasDuration_ShouldReturnFrames()
    {
        var sampler = new SimpleArmPlaybackSampler();

        var snapshot = sampler.Sample(CreateMoveResult(), TimeSpan.FromMilliseconds(100));

        Assert.True(snapshot.FrameCount > 1);
        Assert.True(snapshot.TotalDuration > TimeSpan.Zero);
    }

    [Fact]
    public void Sample_WhenMovementIsBetweenSteps_ShouldInterpolateJoints()
    {
        var sampler = new SimpleArmPlaybackSampler();

        var snapshot = sampler.Sample(CreateMoveResult(), TimeSpan.FromMilliseconds(100));

        Assert.Contains(
            snapshot.Frames,
            frame => frame.Joints.BaseDegrees is > 0 and < 60);
    }

    [Fact]
    public void Sample_ShouldPreserveCommandMetadata()
    {
        var sampler = new SimpleArmPlaybackSampler();

        var snapshot = sampler.Sample(CreateMoveResult(), TimeSpan.FromMilliseconds(100));

        Assert.Contains(
            snapshot.Frames,
            frame => frame.CommandName == nameof(SimpleArmMoveJointsCommand) &&
                     frame.CommandSource?.LineNumber == 2);
    }

    private static SimpleArmSimulationResult CreateMoveResult()
    {
        var profile = new SimpleArmRobotProfile(
            firstLinkLengthMillimeters: 120,
            secondLinkLengthMillimeters: 90,
            thirdLinkLengthMillimeters: 60,
            baseJoint: new SimpleArmJoint(SimpleArmJointId.Base, -180, 180, 100),
            shoulderJoint: new SimpleArmJoint(SimpleArmJointId.Shoulder, -120, 120, 90),
            elbowJoint: new SimpleArmJoint(SimpleArmJointId.Elbow, -150, 150, 80));
        var context = SimpleArmSimulationContext.Create(
            profile,
            new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0));
        var sequence = new RobotCommandSequence(
        [
            new SimpleArmMoveJointsCommand(
                new SimpleArmJointPosition(BaseDegrees: 60, ShoulderDegrees: 30, ElbowDegrees: -20),
                requestedJointVelocityDegreesPerSecond: 80,
                new RobotCommandSource(lineNumber: 2, text: "ARM BASE=60 SHOULDER=30 ELBOW=-20 SPEED=80"))
        ]);

        return new SimpleArmSimulator().Execute(context, sequence);
    }
}
