using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class CartesianVisualStateSamplerTests
{
    [Fact]
    public void SampleAt_WhenTimeIsDuringMovement_ShouldReturnVisualState()
    {
        var result = CreateMoveSimulation();
        var visualSampler = new CartesianVisualStateSampler();

        var visualState = visualSampler.SampleAt(result, TimeSpan.FromSeconds(1));

        Assert.Equal(TimeSpan.FromSeconds(1), visualState.Time);
        Assert.Equal(RobotState.Moving, visualState.State);
        Assert.Equal(new VisualVector3(50, 0, 0), visualState.Position);
        Assert.Equal(0, visualState.CommandIndex);
        Assert.Equal(nameof(MoveToCommand), visualState.CommandName);
    }

    [Fact]
    public void SampleAt_WhenCommandHasSource_ShouldPreserveCommandSource()
    {
        var source = new RobotCommandSource(1, "MOVE X=100 Y=0 Z=0 SPEED=50");
        var result = CreateMoveSimulation(source);
        var visualSampler = new CartesianVisualStateSampler();

        var visualState = visualSampler.SampleAt(result, TimeSpan.FromSeconds(1));

        Assert.Equal(source, visualState.CommandSource);
    }

    [Fact]
    public void Constructor_WhenTimelineSamplerIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CartesianVisualStateSampler(null!, new CartesianVisualStateMapper()));
    }

    [Fact]
    public void Constructor_WhenVisualStateMapperIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CartesianVisualStateSampler(new SimulationTimelineSampler(), null!));
    }

    private static SimulationResult CreateMoveSimulation(RobotCommandSource? source = null)
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var sequence = new RobotCommandSequence(
        [
            new MoveToCommand(
                new CartesianPosition(X: 100, Y: 0, Z: 0),
                requestedVelocityMillimetersPerSecond: 50,
                source: source)
        ]);

        return simulator.Execute(context, sequence);
    }

    private static RobotProfile CreateProfile() =>
        RobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));
}
