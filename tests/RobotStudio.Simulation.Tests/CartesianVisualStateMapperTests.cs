using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class CartesianVisualStateMapperTests
{
    [Fact]
    public void Map_WhenSampleIsCartesian_ShouldCopyPositionInMillimeters()
    {
        var mapper = new CartesianVisualStateMapper();
        var sample = new SimulationSample(
            TimeSpan.FromSeconds(1),
            RobotState.Moving,
            new CartesianPosition(X: 10, Y: 20, Z: 30),
            CommandIndex: 0,
            CommandName: nameof(MoveToCommand),
            CommandSource: null);

        var visualState = mapper.Map(sample);

        Assert.Equal(new VisualVector3(10, 20, 30), visualState.Position);
    }

    [Fact]
    public void Map_WhenSampleHasStateAndCommandMetadata_ShouldPreserveMetadata()
    {
        var mapper = new CartesianVisualStateMapper();
        var source = new RobotCommandSource(2, "MOVE X=10 Y=20 Z=30");
        var sample = new SimulationSample(
            TimeSpan.FromSeconds(1),
            RobotState.Moving,
            new CartesianPosition(X: 10, Y: 20, Z: 30),
            CommandIndex: 0,
            CommandName: nameof(MoveToCommand),
            CommandSource: source);

        var visualState = mapper.Map(sample);

        Assert.Equal(sample.Time, visualState.Time);
        Assert.Equal(sample.State, visualState.State);
        Assert.Equal(sample.CommandIndex, visualState.CommandIndex);
        Assert.Equal(sample.CommandName, visualState.CommandName);
        Assert.Equal(source, visualState.CommandSource);
    }

    [Fact]
    public void Map_WhenSampleIsNull_ShouldThrow()
    {
        var mapper = new CartesianVisualStateMapper();

        Assert.Throws<ArgumentNullException>(() => mapper.Map(null!));
    }
}
