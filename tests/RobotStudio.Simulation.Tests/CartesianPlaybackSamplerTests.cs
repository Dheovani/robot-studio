using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class CartesianPlaybackSamplerTests
{
    [Fact]
    public void Sample_WhenIntervalDoesNotLandOnFinalTime_ShouldIncludeFinalFrame()
    {
        var result = CreateMoveSimulation();
        var playbackSampler = new CartesianPlaybackSampler();

        var frames = playbackSampler.Sample(result, TimeSpan.FromMilliseconds(750));

        Assert.Collection(
            frames,
            frame => AssertFrame(frame, TimeSpan.Zero, RobotState.Moving, new VisualVector3(0, 0, 0)),
            frame => AssertFrame(frame, TimeSpan.FromMilliseconds(750), RobotState.Moving, new VisualVector3(37.5, 0, 0)),
            frame => AssertFrame(frame, TimeSpan.FromMilliseconds(1500), RobotState.Moving, new VisualVector3(75, 0, 0)),
            frame => AssertFrame(frame, TimeSpan.FromSeconds(2), RobotState.Completed, new VisualVector3(100, 0, 0)));
    }

    [Fact]
    public void Sample_WhenIntervalLandsOnFinalTime_ShouldNotDuplicateFinalFrame()
    {
        var result = CreateMoveSimulation();
        var playbackSampler = new CartesianPlaybackSampler();

        var frames = playbackSampler.Sample(result, TimeSpan.FromSeconds(1));

        Assert.Equal(
            [TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)],
            frames.Select(frame => frame.Time));
    }

    [Fact]
    public void Sample_WhenSimulationDurationIsZero_ShouldReturnSingleFrame()
    {
        var result = CreateZeroDurationSimulation();
        var playbackSampler = new CartesianPlaybackSampler();

        var frames = playbackSampler.Sample(result, TimeSpan.FromMilliseconds(100));

        var frame = Assert.Single(frames);
        AssertFrame(frame, TimeSpan.Zero, RobotState.Completed, new VisualVector3(0, 0, 0));
    }

    [Fact]
    public void Sample_WhenIntervalIsZero_ShouldThrow()
    {
        var result = CreateMoveSimulation();
        var playbackSampler = new CartesianPlaybackSampler();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            playbackSampler.Sample(result, TimeSpan.Zero));
    }

    [Fact]
    public void Constructor_WhenVisualStateSamplerIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CartesianPlaybackSampler(null!));
    }

    private static SimulationResult CreateMoveSimulation()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var sequence = new RobotCommandSequence(
        [
            new MoveToCommand(
                new CartesianPosition(X: 100, Y: 0, Z: 0),
                requestedVelocityMillimetersPerSecond: 50)
        ]);

        return simulator.Execute(context, sequence);
    }

    private static SimulationResult CreateZeroDurationSimulation()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var sequence = new RobotCommandSequence(
        [
            new MoveToCommand(new CartesianPosition(X: 0, Y: 0, Z: 0))
        ]);

        return simulator.Execute(context, sequence);
    }

    private static void AssertFrame(
        RobotVisualState frame,
        TimeSpan expectedTime,
        RobotState expectedState,
        VisualVector3 expectedPosition)
    {
        Assert.Equal(expectedTime, frame.Time);
        Assert.Equal(expectedState, frame.State);
        Assert.Equal(expectedPosition, frame.Position);
    }

    private static CartesianRobotProfile CreateProfile() =>
        CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));
}
