namespace RobotStudio.Domain;

public readonly record struct CartesianPosition(double X, double Y, double Z)
{
    public double GetCoordinate(AxisId axis) => axis switch
    {
        AxisId.X => X,
        AxisId.Y => Y,
        AxisId.Z => Z,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "Unknown axis.")
    };

    public double DistanceTo(CartesianPosition other)
    {
        var deltaX = other.X - X;
        var deltaY = other.Y - Y;
        var deltaZ = other.Z - Z;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
    }
}
