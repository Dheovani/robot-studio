using RobotStudio.Domain;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class RobotStateTransitionsTests
{
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
