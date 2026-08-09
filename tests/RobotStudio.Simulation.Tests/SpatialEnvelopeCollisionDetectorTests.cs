namespace RobotStudio.Simulation.Tests;

public sealed class SpatialEnvelopeCollisionDetectorTests
{
    [Fact]
    public void FindFirstSweptEnvelopeCollision_WhenPathCrossesExpandedObstacle_ShouldReturnEntry()
    {
        var collision = SpatialEnvelopeCollisionDetector.FindFirstSweptEnvelopeCollision(
            new SpatialPoint(0, 50, 50),
            new SpatialPoint(200, 50, 50),
            20,
            new SpatialSimulationEnvironment(
                [new SpatialObstacle("box", new SpatialPoint(100, 40, 40), new SpatialPoint(120, 60, 60))]),
            "Body");

        Assert.NotNull(collision);
        Assert.Equal("Body", collision.ComponentId);
        Assert.Equal(80, collision.ComponentPosition.X);
        Assert.Equal(0.4, collision.TrajectoryFraction, precision: 10);
    }

    [Fact]
    public void LinkEnvelopeIntersects_WhenLinkIsClear_ShouldReturnFalse()
    {
        var intersects = SpatialEnvelopeCollisionDetector.LinkEnvelopeIntersects(
            new SpatialPoint(0, 0, 0),
            new SpatialPoint(100, 0, 0),
            10,
            new SpatialObstacle("box", new SpatialPoint(40, 30, 30), new SpatialPoint(60, 50, 50)));

        Assert.False(intersects);
    }
}
