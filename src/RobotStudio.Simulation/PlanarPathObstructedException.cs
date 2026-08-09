namespace RobotStudio.Simulation;

public sealed class PlanarPathObstructedException : InvalidOperationException
{
    public PlanarPathObstructedException(PlanarPathCollision collision)
        : base(CreateMessage(collision))
    {
        ArgumentNullException.ThrowIfNull(collision);

        ObstacleId = collision.Obstacle.Id;
        RobotPose = collision.RobotPose;
        ContactXMillimeters = collision.ContactXMillimeters;
        ContactYMillimeters = collision.ContactYMillimeters;
        TrajectoryFraction = collision.TrajectoryFraction;
    }

    public string ObstacleId { get; }

    public Domain.Mobile.DifferentialDrivePose RobotPose { get; }

    public double ContactXMillimeters { get; }

    public double ContactYMillimeters { get; }

    public double TrajectoryFraction { get; }

    private static string CreateMessage(PlanarPathCollision collision)
    {
        ArgumentNullException.ThrowIfNull(collision);

        return
            $"Differential drive path is obstructed by '{collision.Obstacle.Id}' near " +
            $"X={collision.ContactXMillimeters:0.###}, Y={collision.ContactYMillimeters:0.###} mm. " +
            $"The robot center would be at X={collision.RobotPose.X:0.###}, " +
            $"Y={collision.RobotPose.Y:0.###} mm.";
    }
}
