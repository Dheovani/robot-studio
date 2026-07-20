using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class DomainErrorModelTests
{
    [Fact]
    public void WaitCommand_WhenDurationIsNegative_ShouldThrowInvalidRobotCommandException()
    {
        var exception = Assert.Throws<InvalidRobotCommandException>(() =>
            new WaitCommand(TimeSpan.FromMilliseconds(-1)));

        Assert.Contains("WAIT duration cannot be negative", exception.Message);
        Assert.Contains("-1 ms", exception.Message);
        Assert.Contains("zero or greater", exception.Message);
    }

    [Fact]
    public void MoveToCommand_WhenRequestedVelocityIsNotPositive_ShouldThrowInvalidRobotCommandException()
    {
        var exception = Assert.Throws<InvalidRobotCommandException>(() =>
            new MoveToCommand(
                new CartesianPosition(X: 10, Y: 20, Z: 30),
                requestedVelocityMillimetersPerSecond: 0));

        Assert.Contains("MOVE requested velocity", exception.Message);
        Assert.Contains("0 mm/s", exception.Message);
        Assert.Contains("greater than zero", exception.Message);
    }

    [Fact]
    public void PositionOutOfRangeException_ShouldDescribeInvalidValueAndExpectedRange()
    {
        var exception = new PositionOutOfRangeException(
            AxisId.X,
            coordinateMillimeters: 301,
            minimumMillimeters: 0,
            maximumMillimeters: 300);

        Assert.Contains("301 mm", exception.Message);
        Assert.Contains("X-axis", exception.Message);
        Assert.Contains("0 mm to 300 mm", exception.Message);
    }

    [Fact]
    public void InvalidRobotStateTransitionException_ShouldDescribeCurrentAndNextStates()
    {
        var exception = new InvalidRobotStateTransitionException(RobotState.Faulted, RobotState.Moving);

        Assert.Contains("Faulted", exception.Message);
        Assert.Contains("Moving", exception.Message);
    }

    [Fact]
    public void ImpossibleMovementException_ShouldExposeReason()
    {
        var exception = new ImpossibleMovementException("No measurable robot component changed.");

        Assert.Equal("No measurable robot component changed.", exception.Reason);
        Assert.Contains("Movement cannot be planned", exception.Message);
        Assert.Contains(exception.Reason, exception.Message);
    }
}
