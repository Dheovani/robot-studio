using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal enum MechanicalTeachingViewMode
{
    Assembled,
    DriveSystem,
    MotionAxes,
    ExplodedAssembly
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

internal sealed record MechanicalExplodedPartOffset(
    RobotPartId PartId,
    Vector3 TranslationMillimeters);

internal sealed record MechanicalRevoluteJointPivot(
    RobotPartId PartId,
    Vector3 PivotMillimeters);

internal sealed record MechanicalTeachingViewOption(
    MechanicalTeachingViewMode Mode,
    string Name,
    string Description,
    IReadOnlyList<string> DemonstrationIds);

internal static class MechanicalTeachingViewCatalog
{
    public static IReadOnlyList<MechanicalTeachingViewOption> Options { get; } =
    [
        new(
            MechanicalTeachingViewMode.Assembled,
            "Assembled machine",
            "Packaged technical model with its assembled components and authored materials.",
            ["coordinated-axis-tour", "individual-axis-inspection"]),
        new(
            MechanicalTeachingViewMode.DriveSystem,
            "Drive system",
            "The packaged model becomes transparent around its highlighted rails, belts, lead screws, and motors.",
            ["coordinated-axis-tour", "individual-axis-inspection"]),
        new(
            MechanicalTeachingViewMode.MotionAxes,
            "Motion axes",
            "Directional overlays identify the machine motion: X in red, Y in green, and Z in blue.",
            ["coordinated-axis-tour", "individual-axis-inspection"]),
        new(
            MechanicalTeachingViewMode.ExplodedAssembly,
            "Exploded assembly",
            "Controlled offsets separate the major assemblies while preserving their parent-child relationships and animation.",
            ["assembly-sequence"])
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

    public static IReadOnlyList<MechanicalExplodedPartOffset> ExplodedOffsets { get; } =
    [
        new(new RobotPartId("controller"), new Vector3(100, -60, 30)),
        new(new RobotPartId("y-bed-carriage"), new Vector3(0, -130, 70)),
        new(new RobotPartId("z-gantry"), new Vector3(0, 0, 100)),
        new(new RobotPartId("x-tool-carriage"), new Vector3(120, 0, 0)),
        new(new RobotPartId("tool"), new Vector3(0, -60, -40))
    ];

    public static bool ShouldGhost(RobotPartKind kind) =>
        kind is RobotPartKind.Base or
            RobotPartKind.Structure or
            RobotPartKind.Carriage;

    public static bool CanSelect(
        RobotPartKind kind,
        bool isSelectable,
        MechanicalTeachingViewMode mode) =>
        isSelectable &&
        (mode != MechanicalTeachingViewMode.DriveSystem || !ShouldGhost(kind));
}
