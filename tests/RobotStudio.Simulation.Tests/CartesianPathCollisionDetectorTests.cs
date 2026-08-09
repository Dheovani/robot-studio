using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Simulation.Tests;

public sealed class CartesianPathCollisionDetectorTests
{
    [Fact]
    public void FindFirstCollision_WhenPathCrossesObstacle_ShouldReturnEntryPoint()
    {
        var obstacle = CreateObstacle("fixture", 40, 60, 40, 60, 40, 60);
        var environment = new CartesianSimulationEnvironment([obstacle]);

        var collision = CartesianPathCollisionDetector.FindFirstCollision(
            new CartesianPosition(0, 0, 0),
            new CartesianPosition(100, 100, 100),
            environment);

        Assert.NotNull(collision);
        Assert.Equal(obstacle, collision.Obstacle);
        Assert.Equal(new CartesianPosition(40, 40, 40), collision.Position);
        Assert.Equal(0.4, collision.TrajectoryFraction, precision: 10);
    }

    [Fact]
    public void FindFirstCollision_WhenMultipleObstaclesIntersect_ShouldReturnNearestObstacle()
    {
        var environment = new CartesianSimulationEnvironment(
        [
            CreateObstacle("far", 70, 80, -10, 10, -10, 10),
            CreateObstacle("near", 20, 30, -10, 10, -10, 10)
        ]);

        var collision = CartesianPathCollisionDetector.FindFirstCollision(
            new CartesianPosition(0, 0, 0),
            new CartesianPosition(100, 0, 0),
            environment);

        Assert.NotNull(collision);
        Assert.Equal("near", collision.Obstacle.Id);
        Assert.Equal(0.2, collision.TrajectoryFraction, precision: 10);
    }

    [Fact]
    public void FindFirstCollision_WhenPathIsClear_ShouldReturnNull()
    {
        var environment = new CartesianSimulationEnvironment(
            [CreateObstacle("fixture", 40, 60, 40, 60, 40, 60)]);

        var collision = CartesianPathCollisionDetector.FindFirstCollision(
            new CartesianPosition(0, 0, 0),
            new CartesianPosition(100, 20, 20),
            environment);

        Assert.Null(collision);
    }

    [Fact]
    public void FindFirstCollision_WhenPathTouchesObstacleBoundary_ShouldReturnCollision()
    {
        var obstacle = CreateObstacle("fixture", 40, 60, 40, 60, 40, 60);

        var collision = CartesianPathCollisionDetector.FindFirstCollision(
            new CartesianPosition(0, 40, 50),
            new CartesianPosition(100, 40, 50),
            new CartesianSimulationEnvironment([obstacle]));

        Assert.NotNull(collision);
        Assert.Equal(new CartesianPosition(40, 40, 50), collision.Position);
    }

    [Fact]
    public void FindFirstCollision_WhenStationaryPositionIsInsideObstacle_ShouldReturnImmediateCollision()
    {
        var position = new CartesianPosition(50, 50, 50);

        var collision = CartesianPathCollisionDetector.FindFirstCollision(
            position,
            position,
            new CartesianSimulationEnvironment(
                [CreateObstacle("fixture", 40, 60, 40, 60, 40, 60)]));

        Assert.NotNull(collision);
        Assert.Equal(0, collision.TrajectoryFraction);
        Assert.Equal(position, collision.Position);
    }

    [Fact]
    public void Constructor_WhenObstacleIdsAreDuplicated_ShouldThrow()
    {
        var obstacles = new[]
        {
            CreateObstacle("Fixture", 10, 20, 10, 20, 10, 20),
            CreateObstacle("fixture", 30, 40, 30, 40, 30, 40)
        };

        var exception = Assert.Throws<ArgumentException>(() =>
            new CartesianSimulationEnvironment(obstacles));

        Assert.Contains("must be unique", exception.Message);
    }

    [Fact]
    public void Constructor_WhenObstacleHasNoPositiveVolume_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new CartesianObstacle(
                "flat",
                new CartesianPosition(10, 10, 10),
                new CartesianPosition(10, 20, 20)));

        Assert.Contains("greater than minimum", exception.Message);
    }

    [Fact]
    public void Constructor_WhenObstacleCoordinateIsNotFinite_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new CartesianObstacle(
                "invalid",
                new CartesianPosition(0, 0, 0),
                new CartesianPosition(double.NaN, 20, 20)));

        Assert.Contains("finite numbers", exception.Message);
    }

    private static CartesianObstacle CreateObstacle(
        string id,
        double minimumX,
        double maximumX,
        double minimumY,
        double maximumY,
        double minimumZ,
        double maximumZ) =>
        new(
            id,
            new CartesianPosition(minimumX, minimumY, minimumZ),
            new CartesianPosition(maximumX, maximumY, maximumZ));
}
