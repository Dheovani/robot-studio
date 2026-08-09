using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public sealed class ScaraPathObstructedException : InvalidOperationException
{
    public ScaraPathObstructedException(ScaraPathCollision collision)
        : base(CreateMessage(collision))
    {
        ArgumentNullException.ThrowIfNull(collision);

        ObstacleId = collision.Obstacle.Id;
        Link = collision.Link;
        Joints = collision.Joints;
        TrajectoryFraction = collision.TrajectoryFraction;
    }

    public string ObstacleId { get; }

    public ScaraLinkId Link { get; }

    public ScaraJointPosition Joints { get; }

    public double TrajectoryFraction { get; }

    private static string CreateMessage(ScaraPathCollision collision)
    {
        ArgumentNullException.ThrowIfNull(collision);

        return
            $"SCARA {collision.Link} is obstructed by '{collision.Obstacle.Id}' near " +
            $"shoulder={collision.Joints.ShoulderDegrees:0.###} degrees and " +
            $"elbow={collision.Joints.ElbowDegrees:0.###} degrees.";
    }
}
