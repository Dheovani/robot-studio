using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal enum MechanicalTeachingViewMode
{
    Assembled,
    DriveSystem
}

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
            "Structural parts become transparent so rails, belts, lead screws, and motors remain visible.")
    ];

    public static bool ShouldGhost(RobotPartKind kind) =>
        kind is RobotPartKind.Base or
            RobotPartKind.Structure or
            RobotPartKind.Carriage or
            RobotPartKind.Controller;
}
