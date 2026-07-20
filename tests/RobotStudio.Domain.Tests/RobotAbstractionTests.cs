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
    public void RobotProfile_ShouldImplementRobotProfileForCartesianPosition()
    {
        var profile = RobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120),
            new Axis(AxisId.Y, 0, 200, 100),
            new Axis(AxisId.Z, 0, 150, 80));

        Assert.IsAssignableFrom<IRobotProfile<CartesianPosition>>(profile);
    }
}
