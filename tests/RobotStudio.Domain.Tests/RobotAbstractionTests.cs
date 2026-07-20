using RobotStudio.Domain;

namespace RobotStudio.Domain.Tests;

public sealed class RobotAbstractionTests
{
    [Fact]
    public void CartesianPosition_ShouldImplementRobotPosition()
    {
        var position = new CartesianPosition(X: 1, Y: 2, Z: 3);

        Assert.IsAssignableFrom<IRobotPosition>(position);
    }

    [Fact]
    public void CartesianRobotProfile_ShouldImplementRobotProfileForCartesianPosition()
    {
        var profile = CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));

        Assert.IsAssignableFrom<IRobotProfile<CartesianPosition>>(profile);
    }
}
