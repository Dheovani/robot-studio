using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation.Tests;

public sealed class CircularFootprintCollisionDetectorTests
{
    [Fact]
    public void FindFirstCollision_WhenCircularFootprintHitsObstacleSide_ShouldReturnCenterAndContact()
    {
        var obstacle = CreateObstacle("wall", 100, 140, 100, 140);

        var collision = CircularFootprintCollisionDetector.FindFirstCollision(
            new DifferentialDrivePose(0, 120, 0),
            new DifferentialDrivePose(200, 120, 0),
            radiusMillimeters: 20,
            new PlanarSimulationEnvironment([obstacle]));

        Assert.NotNull(collision);
        Assert.Equal(new DifferentialDrivePose(80, 120, 0), collision.RobotPose);
        Assert.Equal(100, collision.ContactXMillimeters);
        Assert.Equal(120, collision.ContactYMillimeters);
        Assert.Equal(0.4, collision.TrajectoryFraction, precision: 10);
    }

    [Fact]
    public void FindFirstCollision_WhenFootprintTouchesRoundedCorner_ShouldReturnCollision()
    {
        var collision = CircularFootprintCollisionDetector.FindFirstCollision(
            new DifferentialDrivePose(0, 80, 0),
            new DifferentialDrivePose(200, 80, 0),
            radiusMillimeters: 20,
            new PlanarSimulationEnvironment([CreateObstacle("fixture", 100, 140, 100, 140)]));

        Assert.NotNull(collision);
        Assert.Equal(100, collision.RobotPose.X, precision: 10);
        Assert.Equal(80, collision.RobotPose.Y, precision: 10);
    }

    [Fact]
    public void FindFirstCollision_WhenPathPassesOutsideRoundedCorner_ShouldReturnNull()
    {
        var collision = CircularFootprintCollisionDetector.FindFirstCollision(
            new DifferentialDrivePose(0, 75, 0),
            new DifferentialDrivePose(200, 75, 0),
            radiusMillimeters: 20,
            new PlanarSimulationEnvironment([CreateObstacle("fixture", 100, 140, 100, 140)]));

        Assert.Null(collision);
    }

    [Fact]
    public void FindFirstCollision_WhenStartingFootprintOverlapsObstacle_ShouldReturnImmediateCollision()
    {
        var start = new DifferentialDrivePose(90, 120, 45);

        var collision = CircularFootprintCollisionDetector.FindFirstCollision(
            start,
            new DifferentialDrivePose(50, 120, 45),
            radiusMillimeters: 20,
            new PlanarSimulationEnvironment([CreateObstacle("fixture", 100, 140, 100, 140)]));

        Assert.NotNull(collision);
        Assert.Equal(0, collision.TrajectoryFraction);
        Assert.Equal(start, collision.RobotPose);
    }

    [Fact]
    public void FindFirstCollision_WhenRadiusIsInvalid_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CircularFootprintCollisionDetector.FindFirstCollision(
                new DifferentialDrivePose(0, 0, 0),
                new DifferentialDrivePose(100, 0, 0),
                radiusMillimeters: 0,
                PlanarSimulationEnvironment.Empty));
    }

    private static PlanarObstacle CreateObstacle(
        string id,
        double minimumX,
        double maximumX,
        double minimumY,
        double maximumY) =>
        new(id, minimumX, maximumX, minimumY, maximumY);
}
