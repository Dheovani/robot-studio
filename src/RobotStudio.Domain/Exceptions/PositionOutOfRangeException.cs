namespace RobotStudio.Domain.Exceptions;

public sealed class PositionOutOfRangeException : InvalidOperationException
{
    public PositionOutOfRangeException(
        AxisId axis,
        double coordinateMillimeters,
        double minimumMillimeters,
        double maximumMillimeters)
        : base(
            $"Coordinate {coordinateMillimeters:0.###} mm is outside the {axis}-axis limits " +
            $"({minimumMillimeters:0.###} mm to {maximumMillimeters:0.###} mm).")
    {
        Axis = axis;
        CoordinateMillimeters = coordinateMillimeters;
        MinimumMillimeters = minimumMillimeters;
        MaximumMillimeters = maximumMillimeters;
    }

    public AxisId Axis { get; }

    public double CoordinateMillimeters { get; }

    public double MinimumMillimeters { get; }

    public double MaximumMillimeters { get; }
}
