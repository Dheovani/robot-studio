using System.Numerics;

namespace RobotStudio.Visualization;

public enum RobotOverlayKind
{
    CoordinateGrid,
    CoordinateAxis,
    CoordinateOrigin,
    WorkspaceBoundary,
    Trajectory,
    StartPosition,
    EndPosition,
    AxisLabel,
    PhysicalLimit,
    CollisionBounds
}

public enum RobotOverlayAxis
{
    X,
    Y,
    Z
}

public abstract record RobotOverlayPrimitive(
    RobotOverlayKind Kind,
    RobotPartId? AttachedPartId = null);

public sealed record RobotOverlayLine(
    RobotOverlayKind Kind,
    Vector3 Start,
    Vector3 End,
    float Thickness,
    RobotOverlayAxis? Axis = null,
    RobotPartId? AttachedPartId = null)
    : RobotOverlayPrimitive(Kind, AttachedPartId);

public sealed record RobotOverlayPolyline(
    RobotOverlayKind Kind,
    IReadOnlyList<Vector3> Points,
    float Thickness,
    RobotPartId? AttachedPartId = null)
    : RobotOverlayPrimitive(Kind, AttachedPartId);

public sealed record RobotOverlayPoint(
    RobotOverlayKind Kind,
    Vector3 Position,
    float Size,
    RobotOverlayAxis? Axis = null,
    RobotPartId? AttachedPartId = null)
    : RobotOverlayPrimitive(Kind, AttachedPartId);

public sealed record RobotOverlayBox(
    RobotOverlayKind Kind,
    Vector3 Center,
    Vector3 Size,
    float EdgeThickness,
    RobotPartId? AttachedPartId = null)
    : RobotOverlayPrimitive(Kind, AttachedPartId);

public sealed record RobotOverlayLabel(
    RobotOverlayKind Kind,
    string Text,
    Vector3 Position,
    RobotOverlayAxis? Axis = null,
    RobotPartId? AttachedPartId = null)
    : RobotOverlayPrimitive(Kind, AttachedPartId);

public sealed class RobotOverlayScene
{
    public RobotOverlayScene(IEnumerable<RobotOverlayPrimitive> primitives)
    {
        ArgumentNullException.ThrowIfNull(primitives);
        var snapshot = primitives.Select(Snapshot).ToArray();
        foreach (var primitive in snapshot)
        {
            Validate(primitive);
        }

        Primitives = Array.AsReadOnly(snapshot);
    }

    public IReadOnlyList<RobotOverlayPrimitive> Primitives { get; }

    private static RobotOverlayPrimitive Snapshot(RobotOverlayPrimitive primitive) =>
        primitive is RobotOverlayPolyline polyline
            ? polyline with { Points = Array.AsReadOnly(polyline.Points.ToArray()) }
            : primitive;

    private static void Validate(RobotOverlayPrimitive primitive)
    {
        ArgumentNullException.ThrowIfNull(primitive);
        if (!Enum.IsDefined(primitive.Kind))
        {
            throw new ArgumentException("Overlay primitives must use a known semantic kind.", nameof(primitive));
        }

        switch (primitive)
        {
            case RobotOverlayLine line when
                !IsFinite(line.Start) || !IsFinite(line.End) ||
                line.Thickness <= 0 || !float.IsFinite(line.Thickness) ||
                Vector3.DistanceSquared(line.Start, line.End) <= 0.000001f:
                throw InvalidPrimitive();
            case RobotOverlayPolyline polyline when
                polyline.Points is null || polyline.Points.Count < 2 ||
                polyline.Points.Any(point => !IsFinite(point)) ||
                polyline.Thickness <= 0 || !float.IsFinite(polyline.Thickness):
                throw InvalidPrimitive();
            case RobotOverlayPoint point when
                !IsFinite(point.Position) || point.Size <= 0 || !float.IsFinite(point.Size):
                throw InvalidPrimitive();
            case RobotOverlayBox box when
                !IsFinite(box.Center) || !IsFinite(box.Size) ||
                box.Size.X <= 0 || box.Size.Y <= 0 || box.Size.Z <= 0 ||
                box.EdgeThickness <= 0 || !float.IsFinite(box.EdgeThickness):
                throw InvalidPrimitive();
            case RobotOverlayLabel label when
                string.IsNullOrWhiteSpace(label.Text) || !IsFinite(label.Position):
                throw InvalidPrimitive();
        }
    }

    private static ArgumentException InvalidPrimitive() =>
        new("Overlay geometry must contain finite, non-zero values.", "primitive");

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
}
