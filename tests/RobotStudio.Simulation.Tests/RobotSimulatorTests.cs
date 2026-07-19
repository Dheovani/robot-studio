using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Simulation.Tests;

public sealed class RobotSimulatorTests
{
    [Fact]
    public void Execute_WhenCommandIsHome_ShouldMoveToOrigin()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 50, Y: 40, Z: 30));
        var sequence = new RobotCommandSequence([new HomeCommand()]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(new CartesianPosition(X: 0, Y: 0, Z: 0), result.FinalContext.CurrentPosition);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.True(result.FinalContext.ElapsedTime > TimeSpan.Zero);
    }

    [Fact]
    public void Execute_WhenCommandIsMove_ShouldMoveToTarget()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var target = new CartesianPosition(X: 10, Y: 20, Z: 30);
        var sequence = new RobotCommandSequence([new MoveToCommand(target)]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(target, result.FinalContext.CurrentPosition);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.True(result.FinalContext.ElapsedTime > TimeSpan.Zero);
    }

    [Fact]
    public void Execute_WhenMoveHasRequestedVelocity_ShouldUseRequestedVelocityInDuration()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var target = new CartesianPosition(X: 100, Y: 0, Z: 0);
        var sequence = new RobotCommandSequence([new MoveToCommand(target, requestedVelocityMillimetersPerSecond: 50)]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(TimeSpan.FromSeconds(2), result.FinalContext.ElapsedTime);
    }

    [Fact]
    public void Execute_WhenCommandIsWait_ShouldAdvanceTimeWithoutMoving()
    {
        var simulator = new RobotSimulator();
        var position = new CartesianPosition(X: 10, Y: 20, Z: 30);
        var context = SimulationContext.Create(CreateProfile(), position);
        var sequence = new RobotCommandSequence([new WaitCommand(TimeSpan.FromMilliseconds(500))]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(position, result.FinalContext.CurrentPosition);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.Equal(TimeSpan.FromMilliseconds(500), result.FinalContext.ElapsedTime);
    }

    [Fact]
    public void Execute_WhenSequenceHasMultipleCommands_ShouldExecuteInOrder()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var target = new CartesianPosition(X: 20, Y: 10, Z: 5);
        var sequence = new RobotCommandSequence(
        [
            new HomeCommand(),
            new MoveToCommand(target),
            new WaitCommand(TimeSpan.FromSeconds(1))
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(target, result.FinalContext.CurrentPosition);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.True(result.FinalContext.ElapsedTime > TimeSpan.FromSeconds(1));
        Assert.Contains(result.Timeline, step => step.Description == "Home command completed.");
        Assert.Contains(result.Timeline, step => step.Description == "Move command completed.");
        Assert.Contains(result.Timeline, step => step.Description == "Wait command completed.");
    }

    [Fact]
    public void Execute_WhenCommandFails_ShouldReturnFaultedResult()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var sequence = new RobotCommandSequence(
        [
            new MoveToCommand(new CartesianPosition(X: 301, Y: 0, Z: 0))
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.False(result.Succeeded);
        Assert.Equal(RobotState.Faulted, result.FinalContext.State);
        Assert.IsType<PositionOutOfRangeException>(result.Failure);
    }

    private static RobotProfile CreateProfile() =>
        RobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120),
            new Axis(AxisId.Y, 0, 200, 100),
            new Axis(AxisId.Z, 0, 150, 80));
}
