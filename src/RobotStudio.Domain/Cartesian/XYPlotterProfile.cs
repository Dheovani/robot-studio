using RobotStudio.Domain;

namespace RobotStudio.Domain.Cartesian;

public sealed record XYPlotterProfile(Axis XAxis, Axis YAxis) : IRobotProfile<XYPlotterPosition>
{
    public static XYPlotterProfile Create(
        Axis xAxis,
        Axis yAxis) => new(xAxis, yAxis);

    public IReadOnlyList<Axis> Axes { get; } = new[] { XAxis, YAxis };

    public void ValidatePosition(XYPlotterPosition position)
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
        AxisId.Z => throw new ArgumentOutOfRangeException(
            nameof(axis),
            axis,
            "An XY plotter profile does not contain a Z axis."),
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "Unknown axis.")
    };

    public CartesianRobotProfile ToCartesianProfile(double drawingPlaneZMillimeters = 0) =>
        CartesianRobotProfile.CreateCartesian(
            XAxis,
            YAxis,
            new Axis(
                AxisId.Z,
                drawingPlaneZMillimeters,
                drawingPlaneZMillimeters + 1,
                Math.Min(XAxis.MaximumVelocityMillimetersPerSecond, YAxis.MaximumVelocityMillimetersPerSecond),
                Math.Min(
                    XAxis.MaximumAccelerationMillimetersPerSecondSquared,
                    YAxis.MaximumAccelerationMillimetersPerSecondSquared)));
}
