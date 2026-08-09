using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation.Tests;

public sealed class SimulationTimelineSamplerTests
{
    [Fact]
    public void SampleAt_WhenTimeIsBeforeFirstStep_ShouldReturnInitialPosition()
    {
        var result = CreateMoveSimulation();
        var sampler = new SimulationTimelineSampler();

        var sample = sampler.SampleAt(result, TimeSpan.FromSeconds(-1));

        Assert.Equal(TimeSpan.FromSeconds(-1), sample.Time);
        Assert.Equal(RobotState.Idle, sample.State);
        Assert.Equal(new CartesianPosition(X: 0, Y: 0, Z: 0), sample.Position);
        Assert.Null(sample.CommandIndex);
        Assert.Null(sample.CommandName);
    }

    [Fact]
    public void SampleAt_WhenTimeIsDuringMovement_ShouldReturnInterpolatedPosition()
    {
        var result = CreateMoveSimulation();
        var sampler = new SimulationTimelineSampler();

        var sample = sampler.SampleAt(result, result.FinalContext.ElapsedTime / 2);

        Assert.Equal(RobotState.Moving, sample.State);
        Assert.Equal(50, sample.Position.X, precision: 4);
        Assert.Equal(0, sample.Position.Y);
        Assert.Equal(0, sample.Position.Z);
        Assert.Equal(0, sample.CommandIndex);
        Assert.Equal(nameof(MoveToCommand), sample.CommandName);
    }

    [Fact]
    public void SampleAt_WhenTimeIsEarlyInMovement_ShouldUseAccelerationAwareProgress()
    {
        var result = CreateMoveSimulation();
        var sampler = new SimulationTimelineSampler();

        var sample = sampler.SampleAt(result, TimeSpan.FromMilliseconds(100));

        Assert.InRange(sample.Position.X, 1.19, 1.21);
        Assert.True(sample.Position.X < 5);
        Assert.InRange(sample.VelocityMillimetersPerSecond, 23.99, 24.01);
        Assert.Equal(240, sample.AccelerationMillimetersPerSecondSquared);
        Assert.Equal(MotionProfilePhase.Acceleration, sample.MotionProfilePhase);
    }

    [Fact]
    public void SampleAt_WhenMovementCompletes_ShouldExposeCompletedProfileMetrics()
    {
        var result = CreateMoveSimulation();
        var sampler = new SimulationTimelineSampler();

        var sample = sampler.SampleAt(result, result.FinalContext.ElapsedTime);

        Assert.Equal(0, sample.VelocityMillimetersPerSecond);
        Assert.Equal(0, sample.AccelerationMillimetersPerSecondSquared);
        Assert.Equal(MotionProfilePhase.Completed, sample.MotionProfilePhase);
    }

    [Fact]
    public void SampleAt_WhenInterpolatingCommandWithSource_ShouldPreserveCommandSource()
    {
        var source = new RobotCommandSource(2, "MOVE X=100 Y=0 Z=0 SPEED=50");
        var result = CreateMoveSimulation(source);
        var sampler = new SimulationTimelineSampler();

        var sample = sampler.SampleAt(result, TimeSpan.FromSeconds(1));

        Assert.Equal(source, sample.CommandSource);
    }

    [Fact]
    public void SampleAt_WhenTimeIsDuringWait_ShouldKeepPosition()
    {
        var result = CreateMoveAndWaitSimulation();
        var sampler = new SimulationTimelineSampler();

        var sample = sampler.SampleAt(result, TimeSpan.FromSeconds(2.5));

        Assert.Equal(RobotState.Waiting, sample.State);
        Assert.Equal(new CartesianPosition(X: 100, Y: 0, Z: 0), sample.Position);
        Assert.Equal(1, sample.CommandIndex);
        Assert.Equal(nameof(WaitCommand), sample.CommandName);
    }

    [Fact]
    public void SampleAt_WhenTimeIsAfterFinalStep_ShouldReturnFinalPosition()
    {
        var result = CreateMoveSimulation();
        var sampler = new SimulationTimelineSampler();

        var sample = sampler.SampleAt(result, TimeSpan.FromSeconds(10));

        Assert.Equal(RobotState.Completed, sample.State);
        Assert.Equal(new CartesianPosition(X: 100, Y: 0, Z: 0), sample.Position);
        Assert.Equal(0, sample.CommandIndex);
        Assert.Equal(nameof(MoveToCommand), sample.CommandName);
    }

    [Fact]
    public void SampleAt_WhenTimeIsExactlyOnStep_ShouldReturnLatestStepAtThatTime()
    {
        var result = CreateMoveSimulation();
        var sampler = new SimulationTimelineSampler();

        var sample = sampler.SampleAt(result, TimeSpan.Zero);

        Assert.Equal(RobotState.Moving, sample.State);
        Assert.Equal(0, sample.CommandIndex);
        Assert.Equal(nameof(MoveToCommand), sample.CommandName);
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

    private static SimulationResult CreateMoveAndWaitSimulation()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var sequence = new RobotCommandSequence(
        [
            new MoveToCommand(
                new CartesianPosition(X: 100, Y: 0, Z: 0),
                requestedVelocityMillimetersPerSecond: 50),
            new WaitCommand(TimeSpan.FromSeconds(1))
        ]);

        return simulator.Execute(context, sequence);
    }

    private static CartesianRobotProfile CreateProfile() =>
        CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));
}
