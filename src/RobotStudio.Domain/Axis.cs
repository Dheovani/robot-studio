namespace RobotStudio.Domain;

public sealed record Axis
{
    public Axis(
        AxisId id,
        double minimumMillimeters,
        double maximumMillimeters,
        double maximumVelocityMillimetersPerSecond)
    {
        if (maximumMillimeters <= minimumMillimeters)
        {
            throw new ArgumentException("Axis maximum limit must be greater than its minimum limit.");
        }

        if (maximumVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentException("Axis maximum velocity must be greater than zero.");
        }

        Id = id;
        MinimumMillimeters = minimumMillimeters;
        MaximumMillimeters = maximumMillimeters;
        MaximumVelocityMillimetersPerSecond = maximumVelocityMillimetersPerSecond;
    }

    public AxisId Id { get; }

    public double MinimumMillimeters { get; }

    public double MaximumMillimeters { get; }

    public double MaximumVelocityMillimetersPerSecond { get; }

    public void ValidateCoordinate(double coordinateMillimeters)
    {
        if (coordinateMillimeters < MinimumMillimeters || coordinateMillimeters > MaximumMillimeters)
        {
            throw new PositionOutOfRangeException(
                Id,
                coordinateMillimeters,
                MinimumMillimeters,
                MaximumMillimeters);
        }
    }
}
