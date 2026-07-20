using RobotStudio.Domain;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class CartesianRobotProfileTests
{
    [Fact]
    public void ValidatePosition_DoesNotThrow_WhenPositionIsInsideAxisLimits()
    {
        var profile = CreateProfile();
        var position = new CartesianPosition(X: 100, Y: 50, Z: 25);

        var exception = Record.Exception(() => profile.ValidatePosition(position));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidatePosition_DoesNotThrow_WhenPositionIsAtMinimumAxisLimits()
    {
        var profile = CreateProfile();
        var position = new CartesianPosition(X: 0, Y: 0, Z: 0);

        var exception = Record.Exception(() => profile.ValidatePosition(position));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidatePosition_DoesNotThrow_WhenPositionIsAtMaximumAxisLimits()
    {
        var profile = CreateProfile();
        var position = new CartesianPosition(X: 300, Y: 200, Z: 150);

        var exception = Record.Exception(() => profile.ValidatePosition(position));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidatePosition_Throws_WhenPositionIsOutsideAxisLimits()
    {
        var profile = CreateProfile();
        var position = new CartesianPosition(X: 301, Y: 50, Z: 25);

        var exception = Assert.Throws<PositionOutOfRangeException>(() => profile.ValidatePosition(position));

        Assert.Equal(AxisId.X, exception.Axis);
    }

    private static CartesianRobotProfile CreateProfile() =>
        CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));
}
