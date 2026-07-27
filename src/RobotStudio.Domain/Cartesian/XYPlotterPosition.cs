namespace RobotStudio.Domain.Cartesian;

public readonly record struct XYPlotterPosition(double X, double Y) : IRobotPosition
{
    public double GetCoordinate(AxisId axis) => axis switch
    {
        AxisId.X => X,
        AxisId.Y => Y,
        AxisId.Z => throw new ArgumentOutOfRangeException(
            nameof(axis),
            axis,
            "An XY plotter does not expose a Z axis."),
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "Unknown axis.")
    };

    public CartesianPosition ToCartesianPosition(double drawingPlaneZMillimeters = 0) =>
        new(X, Y, drawingPlaneZMillimeters);

    public double DistanceTo(XYPlotterPosition other)
    {
        var deltaX = other.X - X;
        var deltaY = other.Y - Y;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
}
