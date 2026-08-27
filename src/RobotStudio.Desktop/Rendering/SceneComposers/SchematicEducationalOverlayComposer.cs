using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Rendering.SceneComposers;

internal static class SchematicEducationalOverlayComposer
{
    private const int WorkspaceCircleSegments = 72;

    public static RobotOverlayScene ComposePlanar(
        float reach,
        float gridSpacing,
        float floorZ,
        float gridThickness,
        float boundaryThickness,
        IEnumerable<Vector3> trajectory,
        float trajectoryThickness)
    {
        if (!float.IsFinite(reach) || reach <= 0 ||
            !float.IsFinite(gridSpacing) || gridSpacing <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reach));
        }

        var primitives = new List<RobotOverlayPrimitive>();
        for (var offset = -reach; offset <= reach; offset += gridSpacing)
        {
            primitives.Add(new RobotOverlayLine(
                RobotOverlayKind.CoordinateGrid,
                new Vector3(-reach, offset, floorZ),
                new Vector3(reach, offset, floorZ),
                gridThickness));
            primitives.Add(new RobotOverlayLine(
                RobotOverlayKind.CoordinateGrid,
                new Vector3(offset, -reach, floorZ),
                new Vector3(offset, reach, floorZ),
                gridThickness));
        }

        primitives.Add(new RobotOverlayPolyline(
            RobotOverlayKind.WorkspaceBoundary,
            CreateCircle(reach, floorZ),
            boundaryThickness));
        AddCoordinateSystem(
            primitives,
            new Vector3(-reach, -reach, floorZ),
            new Vector3(reach, reach, floorZ),
            includeZ: false);
        primitives.Add(new RobotOverlayPoint(
            RobotOverlayKind.CoordinateOrigin,
            new Vector3(0, 0, floorZ - 2),
            24));
        AddTrajectory(primitives, trajectory, trajectoryThickness);

        return new RobotOverlayScene(primitives);
    }

    public static RobotOverlayScene ComposeBox(
        Vector3 minimum,
        Vector3 maximum,
        float gridSpacing,
        float gridThickness,
        float boundaryThickness,
        IEnumerable<Vector3> trajectory,
        float trajectoryThickness)
    {
        if (!IsFinite(minimum) || !IsFinite(maximum) ||
            minimum.X >= maximum.X || minimum.Y >= maximum.Y || minimum.Z >= maximum.Z)
        {
            throw new ArgumentException("Workspace bounds must be finite and increasing.", nameof(maximum));
        }

        var primitives = new List<RobotOverlayPrimitive>();
        for (var x = minimum.X; x <= maximum.X; x += gridSpacing)
        {
            primitives.Add(new RobotOverlayLine(
                RobotOverlayKind.CoordinateGrid,
                new Vector3(x, minimum.Y, minimum.Z),
                new Vector3(x, maximum.Y, minimum.Z),
                gridThickness));
        }

        for (var y = minimum.Y; y <= maximum.Y; y += gridSpacing)
        {
            primitives.Add(new RobotOverlayLine(
                RobotOverlayKind.CoordinateGrid,
                new Vector3(minimum.X, y, minimum.Z),
                new Vector3(maximum.X, y, minimum.Z),
                gridThickness));
        }

        primitives.Add(new RobotOverlayBox(
            RobotOverlayKind.WorkspaceBoundary,
            (minimum + maximum) / 2,
            maximum - minimum,
            boundaryThickness));
        AddCoordinateSystem(primitives, minimum, maximum, includeZ: true);
        primitives.Add(new RobotOverlayPoint(
            RobotOverlayKind.CoordinateOrigin,
            Vector3.Clamp(Vector3.Zero, minimum, maximum),
            24));
        AddTrajectory(primitives, trajectory, trajectoryThickness);

        return new RobotOverlayScene(primitives);
    }

    public static RobotOverlayScene ComposeRectangularPlanar(
        Vector2 minimum,
        Vector2 maximum,
        float floorZ,
        float gridSpacing,
        float gridThickness,
        float boundaryThickness,
        IEnumerable<Vector3> trajectory,
        float trajectoryThickness)
    {
        if (!float.IsFinite(minimum.X) || !float.IsFinite(minimum.Y) ||
            !float.IsFinite(maximum.X) || !float.IsFinite(maximum.Y) ||
            minimum.X >= maximum.X || minimum.Y >= maximum.Y)
        {
            throw new ArgumentException("Planar workspace bounds must be finite and increasing.", nameof(maximum));
        }

        var primitives = new List<RobotOverlayPrimitive>();
        for (var x = minimum.X; x <= maximum.X; x += gridSpacing)
        {
            primitives.Add(new RobotOverlayLine(
                RobotOverlayKind.CoordinateGrid,
                new Vector3(x, minimum.Y, floorZ),
                new Vector3(x, maximum.Y, floorZ),
                gridThickness));
        }

        for (var y = minimum.Y; y <= maximum.Y; y += gridSpacing)
        {
            primitives.Add(new RobotOverlayLine(
                RobotOverlayKind.CoordinateGrid,
                new Vector3(minimum.X, y, floorZ),
                new Vector3(maximum.X, y, floorZ),
                gridThickness));
        }

        primitives.Add(new RobotOverlayPolyline(
            RobotOverlayKind.WorkspaceBoundary,
            Array.AsReadOnly(new[]
            {
                new Vector3(minimum, floorZ),
                new Vector3(maximum.X, minimum.Y, floorZ),
                new Vector3(maximum, floorZ),
                new Vector3(minimum.X, maximum.Y, floorZ),
                new Vector3(minimum, floorZ)
            }),
            boundaryThickness));
        AddCoordinateSystem(
            primitives,
            new Vector3(minimum, floorZ),
            new Vector3(maximum, floorZ),
            includeZ: false);
        primitives.Add(new RobotOverlayPoint(
            RobotOverlayKind.CoordinateOrigin,
            Vector3.Clamp(Vector3.Zero, new Vector3(minimum, floorZ), new Vector3(maximum, floorZ)),
            12));
        AddTrajectory(primitives, trajectory, trajectoryThickness);

        return new RobotOverlayScene(primitives);
    }

    private static void AddCoordinateSystem(
        ICollection<RobotOverlayPrimitive> primitives,
        Vector3 minimum,
        Vector3 maximum,
        bool includeZ)
    {
        const float axisThickness = 3;
        const float labelOffset = 20;
        var origin = Vector3.Clamp(Vector3.Zero, minimum, maximum);
        var axes = new List<(RobotOverlayAxis Axis, Vector3 Start, Vector3 End, Vector3 Label)>
        {
            (RobotOverlayAxis.X,
                new Vector3(minimum.X, origin.Y, origin.Z),
                new Vector3(maximum.X, origin.Y, origin.Z),
                new Vector3(maximum.X + labelOffset, origin.Y, origin.Z)),
            (RobotOverlayAxis.Y,
                new Vector3(origin.X, minimum.Y, origin.Z),
                new Vector3(origin.X, maximum.Y, origin.Z),
                new Vector3(origin.X, maximum.Y + labelOffset, origin.Z))
        };
        if (includeZ)
        {
            axes.Add((
                RobotOverlayAxis.Z,
                new Vector3(origin.X, origin.Y, minimum.Z),
                new Vector3(origin.X, origin.Y, maximum.Z),
                new Vector3(origin.X, origin.Y, maximum.Z + labelOffset)));
        }

        foreach (var axis in axes)
        {
            primitives.Add(new RobotOverlayLine(
                RobotOverlayKind.CoordinateAxis,
                axis.Start,
                axis.End,
                axisThickness,
                axis.Axis));
            primitives.Add(new RobotOverlayLabel(
                RobotOverlayKind.AxisLabel,
                axis.Axis.ToString(),
                axis.Label,
                axis.Axis));
            primitives.Add(new RobotOverlayPoint(
                RobotOverlayKind.PhysicalLimit,
                axis.Start,
                6,
                axis.Axis));
            primitives.Add(new RobotOverlayPoint(
                RobotOverlayKind.PhysicalLimit,
                axis.End,
                6,
                axis.Axis));
        }
    }

    private static void AddTrajectory(
        ICollection<RobotOverlayPrimitive> primitives,
        IEnumerable<Vector3> trajectory,
        float thickness)
    {
        ArgumentNullException.ThrowIfNull(trajectory);
        var points = trajectory.ToArray();
        if (points.Length >= 2)
        {
            primitives.Add(new RobotOverlayPolyline(
                RobotOverlayKind.Trajectory,
                Array.AsReadOnly(points),
                thickness));
        }
    }

    private static IReadOnlyList<Vector3> CreateCircle(float radius, float z)
    {
        var points = new Vector3[WorkspaceCircleSegments + 1];
        for (var index = 0; index <= WorkspaceCircleSegments; index++)
        {
            var angle = 2 * MathF.PI * index / WorkspaceCircleSegments;
            points[index] = new Vector3(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius, z);
        }

        return Array.AsReadOnly(points);
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
}
