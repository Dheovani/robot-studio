using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Simulation;

public sealed class CartesianPathObstructedException : InvalidOperationException
{
    public CartesianPathObstructedException(CartesianPathCollision collision)
        : base(CreateMessage(collision))
    {
        ArgumentNullException.ThrowIfNull(collision);

        ObstacleId = collision.Obstacle.Id;
        CollisionPosition = collision.Position;
        TrajectoryFraction = collision.TrajectoryFraction;
    }

    public string ObstacleId { get; }

    public CartesianPosition CollisionPosition { get; }

    public double TrajectoryFraction { get; }

    private static string CreateMessage(CartesianPathCollision collision)
    {
        ArgumentNullException.ThrowIfNull(collision);

        return
            $"Cartesian path is obstructed by '{collision.Obstacle.Id}' at " +
            $"X={collision.Position.X:0.###}, Y={collision.Position.Y:0.###}, " +
            $"Z={collision.Position.Z:0.###} mm.";
    }
}
