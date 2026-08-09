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
    public void Execute_WhenMoveHasRequestedVelocity_ShouldIncludeAccelerationInDuration()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var target = new CartesianPosition(X: 100, Y: 0, Z: 0);
        var sequence = new RobotCommandSequence([new MoveToCommand(target, requestedVelocityMillimetersPerSecond: 50)]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.InRange(result.FinalContext.ElapsedTime.TotalSeconds, 2.2083, 2.2084);
        Assert.NotNull(result.Timeline[1].MotionProfile);
    }

    [Fact]
    public void Execute_WhenMoveHasZeroDistance_ShouldCompleteWithoutAdvancingTime()
    {
        var simulator = new RobotSimulator();
        var position = new CartesianPosition(X: 10, Y: 20, Z: 30);
        var context = SimulationContext.Create(CreateProfile(), position);
        var sequence = new RobotCommandSequence([new MoveToCommand(position)]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(position, result.FinalContext.CurrentPosition);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.Equal(TimeSpan.Zero, result.FinalContext.ElapsedTime);
        AssertTimelineStep(result.Timeline[1], 0, nameof(MoveToCommand), RobotState.Moving);
        AssertTimelineStep(result.Timeline[2], 0, nameof(MoveToCommand), RobotState.Completed);
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
    public void Execute_WhenSequenceHasMultipleCommands_ShouldRecordCommandSourceInTimeline()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var sequence = new RobotCommandSequence(
        [
            new HomeCommand(),
            new MoveToCommand(new CartesianPosition(X: 20, Y: 10, Z: 5)),
            new WaitCommand(TimeSpan.FromSeconds(1))
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.Null(result.Timeline[0].CommandIndex);
        Assert.Null(result.Timeline[0].CommandName);
        AssertTimelineStep(result.Timeline[1], 0, nameof(HomeCommand), RobotState.Homing);
        AssertTimelineStep(result.Timeline[2], 0, nameof(HomeCommand), RobotState.Completed);
        AssertTimelineStep(result.Timeline[3], 1, nameof(MoveToCommand), RobotState.Moving);
        AssertTimelineStep(result.Timeline[4], 1, nameof(MoveToCommand), RobotState.Completed);
        AssertTimelineStep(result.Timeline[5], 2, nameof(WaitCommand), RobotState.Waiting);
        AssertTimelineStep(result.Timeline[6], 2, nameof(WaitCommand), RobotState.Completed);
    }

    [Fact]
    public void Execute_WhenSequenceHasMultipleCommands_ShouldRecordExactStateTransitionsInOrder()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var sequence = new RobotCommandSequence(
        [
            new HomeCommand(),
            new MoveToCommand(new CartesianPosition(X: 20, Y: 10, Z: 5)),
            new WaitCommand(TimeSpan.FromSeconds(1))
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.Equal(
            [
                RobotState.Idle,
                RobotState.Homing,
                RobotState.Completed,
                RobotState.Moving,
                RobotState.Completed,
                RobotState.Waiting,
                RobotState.Completed
            ],
            result.Timeline.Select(step => step.State));
    }

    [Fact]
    public void Execute_WhenCommandHasSource_ShouldRecordCommandSourceInTimeline()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var source = new RobotCommandSource(3, "MOVE X=100 Y=0 Z=0");
        var sequence = new RobotCommandSequence(
        [
            new MoveToCommand(
                new CartesianPosition(X: 100, Y: 0, Z: 0),
                source: source)
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.Equal(source, result.Timeline[1].CommandSource);
        Assert.Equal(source, result.Timeline[2].CommandSource);
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
        Assert.Equal(0, result.Timeline[^1].CommandIndex);
        Assert.Equal(nameof(MoveToCommand), result.Timeline[^1].CommandName);
    }

    [Fact]
    public void Execute_WhenCommandFails_ShouldPreserveLastValidPosition()
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            CreateProfile(),
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var lastValidPosition = new CartesianPosition(X: 100, Y: 0, Z: 0);
        var sequence = new RobotCommandSequence(
        [
            new MoveToCommand(lastValidPosition),
            new MoveToCommand(new CartesianPosition(X: 301, Y: 0, Z: 0))
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.False(result.Succeeded);
        Assert.Equal(RobotState.Faulted, result.FinalContext.State);
        Assert.Equal(lastValidPosition, result.FinalContext.CurrentPosition);
        Assert.Equal(lastValidPosition, result.Timeline[^1].Position);
        Assert.Equal(1, result.Timeline[^1].CommandIndex);
        Assert.Equal(nameof(MoveToCommand), result.Timeline[^1].CommandName);
    }

    [Fact]
    public void Execute_WhenResettingFault_ShouldPreservePositionAndElapsedTime()
    {
        var position = new CartesianPosition(X: 100, Y: 50, Z: 20);
        var failedResult = new RobotSimulator().Execute(
            SimulationContext.Create(CreateProfile(), new CartesianPosition(0, 0, 0)),
            new RobotCommandSequence(
            [
                new MoveToCommand(position),
                new MoveToCommand(new CartesianPosition(301, 0, 0))
            ]));
        var source = new RobotCommandSource(4, "RESET");

        var result = new RobotSimulator().Execute(
            failedResult.FinalContext,
            new RobotCommandSequence([new ResetFaultCommand(source)]));

        Assert.True(result.Succeeded);
        Assert.Equal(RobotState.Idle, result.FinalContext.State);
        Assert.Equal(position, result.FinalContext.CurrentPosition);
        Assert.Equal(failedResult.FinalContext.ElapsedTime, result.FinalContext.ElapsedTime);
        Assert.Equal(source, result.Timeline[^1].CommandSource);
    }

    [Fact]
    public void Create_WhenInitialPositionIsOutsideCartesianRobotProfile_ShouldThrow()
    {
        var profile = CreateProfile();

        Assert.Throws<PositionOutOfRangeException>(() =>
            SimulationContext.Create(
                profile,
                new CartesianPosition(X: 301, Y: 0, Z: 0)));
    }

    private static void AssertTimelineStep(
        SimulationStep step,
        int commandIndex,
        string commandName,
        RobotState state)
    {
        Assert.Equal(commandIndex, step.CommandIndex);
        Assert.Equal(commandName, step.CommandName);
        Assert.Equal(state, step.State);
    }

    private static CartesianRobotProfile CreateProfile() =>
        CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));
}
