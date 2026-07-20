using RobotStudio.Domain;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class RobotStateTransitionsTests
{
    [Fact]
    public void InitialState_ShouldBeIdle()
    {
        Assert.Equal(RobotState.Idle, RobotStateTransitions.InitialState);
    }

    [Theory]
    [InlineData(RobotState.Idle)]
    [InlineData(RobotState.Completed)]
    public void CanTransitionTo_WhenReadyStateMoves_ShouldReturnTrue(RobotState current)
    {
        var canTransition = RobotStateTransitions.CanTransitionTo(current, RobotState.Moving);

        Assert.True(canTransition);
    }

    [Theory]
    [InlineData(RobotState.Idle)]
    [InlineData(RobotState.Completed)]
    public void CanTransitionTo_WhenReadyStateWaits_ShouldReturnTrue(RobotState current)
    {
        var canTransition = RobotStateTransitions.CanTransitionTo(current, RobotState.Waiting);

        Assert.True(canTransition);
    }

    [Theory]
    [InlineData(RobotState.Idle)]
    [InlineData(RobotState.Moving)]
    [InlineData(RobotState.Homing)]
    [InlineData(RobotState.Waiting)]
    [InlineData(RobotState.Completed)]
    [InlineData(RobotState.Faulted)]
    public void CanTransitionTo_WhenNextStateIsHoming_ShouldReturnTrue(RobotState current)
    {
        var canTransition = RobotStateTransitions.CanTransitionTo(current, RobotState.Homing);

        Assert.True(canTransition);
    }

    [Theory]
    [InlineData(RobotState.Moving)]
    [InlineData(RobotState.Homing)]
    [InlineData(RobotState.Waiting)]
    public void CanTransitionTo_WhenActiveStateCompletes_ShouldReturnTrue(RobotState current)
    {
        var canTransition = RobotStateTransitions.CanTransitionTo(current, RobotState.Completed);

        Assert.True(canTransition);
    }

    [Theory]
    [InlineData(RobotState.Idle)]
    [InlineData(RobotState.Moving)]
    [InlineData(RobotState.Homing)]
    [InlineData(RobotState.Waiting)]
    [InlineData(RobotState.Completed)]
    public void CanTransitionTo_WhenStateFails_ShouldReturnTrue(RobotState current)
    {
        var canTransition = RobotStateTransitions.CanTransitionTo(current, RobotState.Faulted);

        Assert.True(canTransition);
    }

    [Theory]
    [InlineData(RobotState.Moving, true)]
    [InlineData(RobotState.Homing, true)]
    [InlineData(RobotState.Waiting, true)]
    [InlineData(RobotState.Idle, false)]
    [InlineData(RobotState.Completed, false)]
    [InlineData(RobotState.Faulted, false)]
    public void IsActive_ShouldIdentifyStatesThatAreExecutingWork(
        RobotState state,
        bool expected)
    {
        Assert.Equal(expected, RobotStateTransitions.IsActive(state));
    }

    [Theory]
    [InlineData(RobotState.Idle, true)]
    [InlineData(RobotState.Completed, true)]
    [InlineData(RobotState.Moving, false)]
    [InlineData(RobotState.Homing, false)]
    [InlineData(RobotState.Waiting, false)]
    [InlineData(RobotState.Faulted, false)]
    public void IsReadyForCommand_ShouldIdentifyStatesThatCanStartNormalCommands(
        RobotState state,
        bool expected)
    {
        Assert.Equal(expected, RobotStateTransitions.IsReadyForCommand(state));
    }

    [Theory]
    [InlineData(RobotState.Completed, true)]
    [InlineData(RobotState.Faulted, true)]
    [InlineData(RobotState.Idle, false)]
    [InlineData(RobotState.Moving, false)]
    [InlineData(RobotState.Homing, false)]
    [InlineData(RobotState.Waiting, false)]
    public void IsTerminalForCurrentCommand_ShouldIdentifyCommandEndStates(
        RobotState state,
        bool expected)
    {
        Assert.Equal(expected, RobotStateTransitions.IsTerminalForCurrentCommand(state));
    }

    [Theory]
    [InlineData(RobotState.Faulted, true)]
    [InlineData(RobotState.Idle, false)]
    [InlineData(RobotState.Moving, false)]
    [InlineData(RobotState.Homing, false)]
    [InlineData(RobotState.Waiting, false)]
    [InlineData(RobotState.Completed, false)]
    public void IsRecoverable_ShouldIdentifyFaultedState(
        RobotState state,
        bool expected)
    {
        Assert.Equal(expected, RobotStateTransitions.IsRecoverable(state));
    }

    [Theory]
    [InlineData(RobotState.Idle, RobotState.Completed)]
    [InlineData(RobotState.Faulted, RobotState.Moving)]
    [InlineData(RobotState.Faulted, RobotState.Waiting)]
    [InlineData(RobotState.Moving, RobotState.Waiting)]
    public void CanTransitionTo_WhenTransitionIsInvalid_ShouldReturnFalse(
        RobotState current,
        RobotState next)
    {
        var canTransition = RobotStateTransitions.CanTransitionTo(current, next);

        Assert.False(canTransition);
    }

    [Fact]
    public void EnsureCanTransitionTo_WhenTransitionIsInvalid_ShouldThrow()
    {
        var exception = Assert.Throws<InvalidRobotStateTransitionException>(() =>
            RobotStateTransitions.EnsureCanTransitionTo(RobotState.Faulted, RobotState.Moving));

        Assert.Equal(RobotState.Faulted, exception.Current);
        Assert.Equal(RobotState.Moving, exception.Next);
    }

    [Fact]
    public void EnsureCanTransitionTo_WhenTransitionIsValid_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            RobotStateTransitions.EnsureCanTransitionTo(RobotState.Completed, RobotState.Moving));

        Assert.Null(exception);
    }
}
