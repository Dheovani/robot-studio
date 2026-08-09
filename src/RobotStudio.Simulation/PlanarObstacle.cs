namespace RobotStudio.Simulation;

public sealed record PlanarObstacle
{
    public PlanarObstacle(
        string id,
        double minimumXMillimeters,
        double maximumXMillimeters,
        double minimumYMillimeters,
        double maximumYMillimeters)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Obstacle ID cannot be empty.", nameof(id));
        }

        if (!double.IsFinite(minimumXMillimeters) ||
            !double.IsFinite(maximumXMillimeters) ||
            !double.IsFinite(minimumYMillimeters) ||
            !double.IsFinite(maximumYMillimeters))
        {
            throw new ArgumentException("Obstacle coordinates must be finite numbers.");
        }

        if (maximumXMillimeters <= minimumXMillimeters ||
            maximumYMillimeters <= minimumYMillimeters)
        {
            throw new ArgumentException(
                "Obstacle maximum coordinates must be greater than minimum coordinates on both axes.");
        }

        Id = id.Trim();
        MinimumXMillimeters = minimumXMillimeters;
        MaximumXMillimeters = maximumXMillimeters;
        MinimumYMillimeters = minimumYMillimeters;
        MaximumYMillimeters = maximumYMillimeters;
    }

    public string Id { get; }

    public double MinimumXMillimeters { get; }

    public double MaximumXMillimeters { get; }

    public double MinimumYMillimeters { get; }

    public double MaximumYMillimeters { get; }
}
