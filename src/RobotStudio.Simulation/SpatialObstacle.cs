namespace RobotStudio.Simulation;

public sealed record SpatialObstacle
{
    public SpatialObstacle(string id, SpatialPoint minimum, SpatialPoint maximum)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Obstacle ID cannot be empty.", nameof(id));
        }

        if (!CoordinatesAreFinite(minimum) || !CoordinatesAreFinite(maximum))
        {
            throw new ArgumentException("Obstacle coordinates must be finite numbers.");
        }

        if (maximum.X <= minimum.X || maximum.Y <= minimum.Y || maximum.Z <= minimum.Z)
        {
            throw new ArgumentException("Obstacle maximum coordinates must be greater than minimum coordinates on every axis.");
        }

        Id = id.Trim();
        Minimum = minimum;
        Maximum = maximum;
    }

    public string Id { get; }

    public SpatialPoint Minimum { get; }

    public SpatialPoint Maximum { get; }

    private static bool CoordinatesAreFinite(SpatialPoint point) =>
        double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z);
}
