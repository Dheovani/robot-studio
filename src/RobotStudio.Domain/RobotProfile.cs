namespace RobotStudio.Domain;

public sealed record RobotProfile(Axis XAxis, Axis YAxis, Axis ZAxis)
{
    public static RobotProfile CreateCartesian(
        Axis xAxis,
        Axis yAxis,
        Axis zAxis) => new(xAxis, yAxis, zAxis);

    public IReadOnlyList<Axis> Axes { get; } = new[] { XAxis, YAxis, ZAxis };

    public void ValidatePosition(CartesianPosition position)
    {
        foreach (var axis in Axes)
        {
            axis.ValidateCoordinate(position.GetCoordinate(axis.Id));
        }
    }

    public Axis GetAxis(AxisId axis) => axis switch
    {
        AxisId.X => XAxis,
        AxisId.Y => YAxis,
        AxisId.Z => ZAxis,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "Unknown axis.")
    };
}
