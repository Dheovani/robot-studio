using RobotStudio.Domain;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class AxisTests
{
    [Fact]
    public void Constructor_WhenMaximumLimitIsNotGreaterThanMinimumLimit_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new Axis(
                AxisId.X,
                minimumMillimeters: 100,
                maximumMillimeters: 100,
                maximumVelocityMillimetersPerSecond: 120,
                maximumAccelerationMillimetersPerSecondSquared: 240));
    }

    [Fact]
    public void Constructor_WhenMaximumVelocityIsNotPositive_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new Axis(
                AxisId.X,
                minimumMillimeters: 0,
                maximumMillimeters: 100,
                maximumVelocityMillimetersPerSecond: 0,
                maximumAccelerationMillimetersPerSecondSquared: 240));
    }

    [Fact]
    public void Constructor_WhenMaximumAccelerationIsNotPositive_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new Axis(
                AxisId.X,
                minimumMillimeters: 0,
                maximumMillimeters: 100,
                maximumVelocityMillimetersPerSecond: 120,
                maximumAccelerationMillimetersPerSecondSquared: 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void ValidateCoordinate_WhenCoordinateIsAtBoundary_ShouldNotThrow(double coordinateMillimeters)
    {
        var axis = new Axis(AxisId.X, 0, 100, 120, 240);

        var exception = Record.Exception(() => axis.ValidateCoordinate(coordinateMillimeters));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(-0.001)]
    [InlineData(100.001)]
    public void ValidateCoordinate_WhenCoordinateIsOutsideBoundary_ShouldThrow(double coordinateMillimeters)
    {
        var axis = new Axis(AxisId.X, 0, 100, 120, 240);

        Assert.Throws<PositionOutOfRangeException>(() => axis.ValidateCoordinate(coordinateMillimeters));
    }
}
