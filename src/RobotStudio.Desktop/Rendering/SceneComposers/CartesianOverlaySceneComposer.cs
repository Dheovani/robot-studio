using System.Numerics;
using RobotStudio.Simulation;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Rendering.SceneComposers;

internal static class CartesianOverlaySceneComposer
{
    private const float GridSpacingMillimeters = 25;
    private const float GridLineThicknessMillimeters = 1.2f;
    private const float AxisLineThicknessMillimeters = 4;
    private const float PathLineThicknessMillimeters = 5;
    private const float PositionMarkerSizeMillimeters = 14;
    private const float AxisLabelOffsetMillimeters = 24;
    private const int MaximumPathPointCount = 140;

    public static RobotOverlayScene Compose(CartesianPlaybackSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var bounds = snapshot.WorkspaceBounds;
        var minimum = ToVector(bounds.Minimum);
        var maximum = ToVector(bounds.Maximum);
        var center = ToVector(bounds.Center);
        var size = ToVector(bounds.Size);
        var origin = Vector3.Clamp(Vector3.Zero, minimum, maximum);
        var primitives = new List<RobotOverlayPrimitive>();

        AddGrid(primitives, minimum, maximum);
        AddCoordinateSystem(primitives, minimum, maximum, origin);
        primitives.Add(new RobotOverlayBox(
            RobotOverlayKind.WorkspaceBoundary,
            center,
            size,
            Math.Max(1, Math.Min(size.X, Math.Min(size.Y, size.Z)) * 0.008f)));
        AddTrajectory(primitives, snapshot);
        AddPositionMarkers(primitives, snapshot);
        AddPhysicalLimits(primitives, minimum, maximum, origin);

        return new RobotOverlayScene(primitives);
    }

    private static void AddGrid(
        ICollection<RobotOverlayPrimitive> primitives,
        Vector3 minimum,
        Vector3 maximum)
    {
        var z = minimum.Z - GridLineThicknessMillimeters;
        for (var x = minimum.X; x <= maximum.X; x += GridSpacingMillimeters)
        {
            primitives.Add(new RobotOverlayLine(
                RobotOverlayKind.CoordinateGrid,
                new Vector3(x, minimum.Y, z),
                new Vector3(x, maximum.Y, z),
                GridLineThicknessMillimeters));
        }

        for (var y = minimum.Y; y <= maximum.Y; y += GridSpacingMillimeters)
        {
            primitives.Add(new RobotOverlayLine(
                RobotOverlayKind.CoordinateGrid,
                new Vector3(minimum.X, y, z),
                new Vector3(maximum.X, y, z),
                GridLineThicknessMillimeters));
        }
    }

    private static void AddCoordinateSystem(
        ICollection<RobotOverlayPrimitive> primitives,
        Vector3 minimum,
        Vector3 maximum,
        Vector3 origin)
    {
        AddAxis(primitives, RobotOverlayAxis.X,
            new Vector3(minimum.X, origin.Y, origin.Z),
            new Vector3(maximum.X, origin.Y, origin.Z));
        AddAxis(primitives, RobotOverlayAxis.Y,
            new Vector3(origin.X, minimum.Y, origin.Z),
            new Vector3(origin.X, maximum.Y, origin.Z));
        AddAxis(primitives, RobotOverlayAxis.Z,
            new Vector3(origin.X, origin.Y, minimum.Z),
            new Vector3(origin.X, origin.Y, maximum.Z));

        primitives.Add(new RobotOverlayLabel(
            RobotOverlayKind.AxisLabel,
            "X",
            new Vector3(maximum.X + AxisLabelOffsetMillimeters, origin.Y, origin.Z),
            RobotOverlayAxis.X));
        primitives.Add(new RobotOverlayLabel(
            RobotOverlayKind.AxisLabel,
            "Y",
            new Vector3(origin.X, maximum.Y + AxisLabelOffsetMillimeters, origin.Z),
            RobotOverlayAxis.Y));
        primitives.Add(new RobotOverlayLabel(
            RobotOverlayKind.AxisLabel,
            "Z",
            new Vector3(origin.X, origin.Y, maximum.Z + AxisLabelOffsetMillimeters),
            RobotOverlayAxis.Z));
    }

    private static void AddAxis(
        ICollection<RobotOverlayPrimitive> primitives,
        RobotOverlayAxis axis,
        Vector3 start,
        Vector3 end) =>
        primitives.Add(new RobotOverlayLine(
            RobotOverlayKind.CoordinateAxis,
            start,
            end,
            AxisLineThicknessMillimeters,
            axis));

    private static void AddTrajectory(
        ICollection<RobotOverlayPrimitive> primitives,
        CartesianPlaybackSnapshot snapshot)
    {
        var step = Math.Max(1, snapshot.Poses.Count / MaximumPathPointCount);
        var points = new List<Vector3>();
        for (var index = 0; index < snapshot.Poses.Count; index += step)
        {
            AddIfDistinct(points, ToVector(snapshot.Poses[index].ToolCenterPoint));
        }

        AddIfDistinct(points, ToVector(snapshot.Poses[^1].ToolCenterPoint));
        if (points.Count >= 2)
        {
            primitives.Add(new RobotOverlayPolyline(
                RobotOverlayKind.Trajectory,
                points.AsReadOnly(),
                PathLineThicknessMillimeters));
        }
    }

    private static void AddPositionMarkers(
        ICollection<RobotOverlayPrimitive> primitives,
        CartesianPlaybackSnapshot snapshot)
    {
        primitives.Add(new RobotOverlayPoint(
            RobotOverlayKind.StartPosition,
            ToVector(snapshot.Poses[0].ToolCenterPoint),
            PositionMarkerSizeMillimeters));
        primitives.Add(new RobotOverlayPoint(
            RobotOverlayKind.EndPosition,
            ToVector(snapshot.Poses[^1].ToolCenterPoint),
            PositionMarkerSizeMillimeters));
    }

    private static void AddPhysicalLimits(
        ICollection<RobotOverlayPrimitive> primitives,
        Vector3 minimum,
        Vector3 maximum,
        Vector3 origin)
    {
        foreach (var (axis, start, end) in new[]
        {
            (RobotOverlayAxis.X, new Vector3(minimum.X, origin.Y, origin.Z), new Vector3(maximum.X, origin.Y, origin.Z)),
            (RobotOverlayAxis.Y, new Vector3(origin.X, minimum.Y, origin.Z), new Vector3(origin.X, maximum.Y, origin.Z)),
            (RobotOverlayAxis.Z, new Vector3(origin.X, origin.Y, minimum.Z), new Vector3(origin.X, origin.Y, maximum.Z))
        })
        {
            primitives.Add(new RobotOverlayPoint(RobotOverlayKind.PhysicalLimit, start, 6, axis));
            primitives.Add(new RobotOverlayPoint(RobotOverlayKind.PhysicalLimit, end, 6, axis));
        }
    }

    private static void AddIfDistinct(ICollection<Vector3> points, Vector3 point)
    {
        if (points.Count == 0 || points.Last() != point)
        {
            points.Add(point);
        }
    }

    private static Vector3 ToVector(VisualVector3 value) =>
        new((float)value.XMillimeters, (float)value.YMillimeters, (float)value.ZMillimeters);
}
