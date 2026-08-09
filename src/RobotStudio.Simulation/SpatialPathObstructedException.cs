namespace RobotStudio.Simulation;

public sealed class SpatialPathObstructedException : InvalidOperationException
{
    public SpatialPathObstructedException(string robotFamily, SpatialCollision collision)
        : base(CreateMessage(robotFamily, collision))
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(robotFamily);
        ArgumentNullException.ThrowIfNull(collision);
        RobotFamily = robotFamily;
        ObstacleId = collision.Obstacle.Id;
        ComponentId = collision.ComponentId;
        ComponentPosition = collision.ComponentPosition;
        TrajectoryFraction = collision.TrajectoryFraction;
    }

    public string RobotFamily { get; }
    public string ObstacleId { get; }
    public string ComponentId { get; }
    public SpatialPoint ComponentPosition { get; }
    public double TrajectoryFraction { get; }

    private static string CreateMessage(string robotFamily, SpatialCollision collision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(robotFamily);
        ArgumentNullException.ThrowIfNull(collision);
        return $"{robotFamily} component '{collision.ComponentId}' is obstructed by '{collision.Obstacle.Id}' near " +
               $"X={collision.ComponentPosition.X:0.###}, Y={collision.ComponentPosition.Y:0.###}, Z={collision.ComponentPosition.Z:0.###} mm.";
    }
}
