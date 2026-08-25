using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class XYPlotterMechanicalTeachingViewCatalog
{
    public static IReadOnlyList<MechanicalTeachingViewOption> Options { get; } =
    [
        new(
            MechanicalTeachingViewMode.Assembled,
            "Assembled plotter",
            "Complete two-axis pen plotter with its paper bed, moving bridge, carriage, and pen mechanism.",
            ["rectangular-path-tour", "individual-axis-inspection"]),
        new(
            MechanicalTeachingViewMode.DriveSystem,
            "Drive system",
            "Structural parts become transparent so the X/Y rails, belts, and motors remain visible.",
            ["rectangular-path-tour", "individual-axis-inspection"]),
        new(
            MechanicalTeachingViewMode.MotionAxes,
            "Motion axes",
            "Directional overlays identify X carriage travel in red and Y bridge travel in green.",
            ["rectangular-path-tour", "individual-axis-inspection"]),
        new(
            MechanicalTeachingViewMode.ExplodedAssembly,
            "Exploded assembly",
            "Separates the controller, drawing bed, Y bridge, X carriage, and pen-lift assembly.",
            ["assembly-sequence"])
    ];

    public static IReadOnlyList<MechanicalMotionAxisGuide> MotionAxes { get; } =
    [
        new(
            MechanicalMotionAxis.X,
            new(-3.5f, -1.5f, 2.6f),
            new(3.5f, -1.5f, 2.6f),
            new RobotPartId("y-gantry")),
        new(MechanicalMotionAxis.Y, new(-4.8f, -3.3f, 1.1f), new(-4.8f, 2.8f, 1.1f))
    ];

    public static IReadOnlyList<MechanicalExplodedPartOffset> ExplodedOffsets { get; } =
    [
        new(new RobotPartId("controller"), new Vector3(65, 0, 70)),
        new(new RobotPartId("paper-bed"), new Vector3(0, -110, 60)),
        new(new RobotPartId("y-gantry"), new Vector3(0, 0, 100)),
        new(new RobotPartId("x-carriage"), new Vector3(120, 0, 0)),
        new(new RobotPartId("pen-lift"), new Vector3(0, -70, -35))
    ];
}
