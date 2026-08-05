using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Simulation.Tests;

public sealed class DeltaPlaybackSamplerTests
{
    [Fact]
    public void Sample_ShouldReturnFramesAtFixedInterval()
    {
        var snapshot = new DeltaPlaybackSampler()
            .Sample(CreateMoveResult(), TimeSpan.FromMilliseconds(100));

        Assert.True(snapshot.FrameCount > 2);
        Assert.Equal(TimeSpan.Zero, snapshot.Frames[0].Time);
        Assert.Equal(snapshot.TotalDuration, snapshot.Frames[^1].Time);
    }

    [Fact]
    public void Sample_ShouldInterpolateActuatorPosition()
    {
        var snapshot = new DeltaPlaybackSampler()
            .Sample(CreateMoveResult(), TimeSpan.FromMilliseconds(100));

        Assert.Contains(
            snapshot.Frames,
            frame => frame.Actuators.CMillimeters is > 0 and < 90);
    }

    [Fact]
    public void Sample_ShouldPreserveCommandMetadata()
    {
        var snapshot = new DeltaPlaybackSampler()
            .Sample(CreateMoveResult(), TimeSpan.FromMilliseconds(100));

        Assert.Contains(
            snapshot.Frames,
            frame => frame.CommandName == nameof(DeltaMoveActuatorsCommand) &&
                     frame.CommandSource?.LineNumber == 2);
    }

    private static DeltaSimulationResult CreateMoveResult()
    {
        var profile = new DeltaRobotProfile(
            baseRadiusMillimeters: 140,
            toolZOffsetMillimeters: 0,
            actuatorA: new DeltaActuator(DeltaActuatorId.A, 0, 180, 120),
            actuatorB: new DeltaActuator(DeltaActuatorId.B, 0, 180, 100),
            actuatorC: new DeltaActuator(DeltaActuatorId.C, 0, 180, 90));
        var context = DeltaSimulationContext.Create(
            profile,
            new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 0, CMillimeters: 0));
        var sequence = new RobotCommandSequence(
        [
            new DeltaMoveActuatorsCommand(
                new DeltaActuatorPosition(AMillimeters: 30, BMillimeters: 60, CMillimeters: 90),
                requestedActuatorVelocityMillimetersPerSecond: 80,
                new RobotCommandSource(lineNumber: 2, text: "DELTA A=30 B=60 C=90 SPEED=80"))
        ]);

        return new DeltaSimulator().Execute(context, sequence);
    }
}
