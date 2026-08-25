using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal enum MechanicalTeachingViewMode
{
    Assembled,
    DriveSystem,
    MotionAxes
}

internal enum MechanicalMotionAxis
{
    X,
    Y,
    Z
}

internal sealed record MechanicalMotionAxisGuide(
    MechanicalMotionAxis Axis,
    Vector3 Start,
    Vector3 End,
    RobotPartId? AttachedPartId = null);

internal sealed record MechanicalTeachingViewOption(
    MechanicalTeachingViewMode Mode,
    string Name,
    string Description);

internal static class MechanicalTeachingViewCatalog
{
    public static IReadOnlyList<MechanicalTeachingViewOption> Options { get; } =
    [
        new(
            MechanicalTeachingViewMode.Assembled,
            "Assembled machine",
            "Packaged technical model with its assembled components and authored materials."),
        new(
            MechanicalTeachingViewMode.DriveSystem,
            "Drive system",
            "The packaged model becomes transparent around its highlighted rails, belts, lead screws, and motors."),
        new(
            MechanicalTeachingViewMode.MotionAxes,
            "Motion axes",
            "Directional overlays identify the machine motion: X in red, Y in green, and Z in blue.")
    ];

    public static IReadOnlyList<MechanicalMotionAxisGuide> MotionAxes { get; } =
    [
        new(
            MechanicalMotionAxis.X,
            new(-3.4f, 1.05f, 6.25f),
            new(3.4f, 1.05f, 6.25f),
            new RobotPartId("z-gantry")),
        new(MechanicalMotionAxis.Y, new(-3.35f, -3.35f, 1.75f), new(-3.35f, 1.85f, 1.75f)),
        new(MechanicalMotionAxis.Z, new(-4.7f, 2.65f, 1.1f), new(-4.7f, 2.65f, 7.45f))
    ];

    public static bool ShouldGhost(RobotPartKind kind) =>
        kind is RobotPartKind.Base or
            RobotPartKind.Structure or
            RobotPartKind.Carriage or
            RobotPartKind.Controller;
}
