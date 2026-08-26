using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class DeltaMechanicalTeachingViewCatalog
{
    public static IReadOnlyList<MechanicalTeachingViewOption> Options { get; } =
    [
        new(
            MechanicalTeachingViewMode.Assembled,
            "Assembled linear Delta",
            "Overhead parallel robot with three linear actuators, six links, a moving platform, and a compact tool.",
            ["pick-and-place", "individual-actuator-inspection"]),
        new(
            MechanicalTeachingViewMode.DriveSystem,
            "Drive system",
            "Transparent frame structures expose the three servomotors, linear rails, carriages, and paired links.",
            ["pick-and-place", "individual-actuator-inspection"]),
        new(
            MechanicalTeachingViewMode.MotionAxes,
            "Actuator and TCP axes",
            "Guides compare the three vertical actuator directions with the moving platform coordinate frame.",
            ["pick-and-place", "individual-actuator-inspection"]),
        new(
            MechanicalTeachingViewMode.ExplodedAssembly,
            "Exploded assembly",
            "Separates the fixed frame, actuator towers, parallel links, moving platform, and tool by mechanical group.",
            ["assembly-sequence"])
    ];

    public static IReadOnlyList<MechanicalMotionAxisGuide> MotionAxes { get; } =
    [
        new(MechanicalMotionAxis.Z, new(0, 3.2f, 2.8f), new(0, 3.2f, 5.9f), new RobotPartId("actuator-a")),
        new(MechanicalMotionAxis.Z, new(-2.77f, -1.6f, 2.8f), new(-2.77f, -1.6f, 5.9f), new RobotPartId("actuator-b")),
        new(MechanicalMotionAxis.Z, new(2.77f, -1.6f, 2.8f), new(2.77f, -1.6f, 5.9f), new RobotPartId("actuator-c")),
        new(MechanicalMotionAxis.X, new(-1.2f, 0, 1.65f), new(1.2f, 0, 1.65f), new RobotPartId("platform")),
        new(MechanicalMotionAxis.Y, new(0, -1.2f, 1.65f), new(0, 1.2f, 1.65f), new RobotPartId("platform")),
        new(MechanicalMotionAxis.Z, new(0, 0, 0.55f), new(0, 0, 2.25f), new RobotPartId("platform"))
    ];

    public static IReadOnlyList<MechanicalExplodedPartOffset> ExplodedOffsets { get; } =
    [
        new(new RobotPartId("controller"), new Vector3(0, -140, 20)),
        new(new RobotPartId("actuator-a"), new Vector3(0, 120, 50)),
        new(new RobotPartId("actuator-b"), new Vector3(-105, -60, 50)),
        new(new RobotPartId("actuator-c"), new Vector3(105, -60, 50)),
        new(new RobotPartId("link-a-left"), new Vector3(-35, 55, 30)),
        new(new RobotPartId("link-a-right"), new Vector3(35, 55, 30)),
        new(new RobotPartId("link-b-left"), new Vector3(-65, -20, 30)),
        new(new RobotPartId("link-b-right"), new Vector3(-35, -65, 30)),
        new(new RobotPartId("link-c-left"), new Vector3(35, -65, 30)),
        new(new RobotPartId("link-c-right"), new Vector3(65, -20, 30)),
        new(new RobotPartId("platform"), new Vector3(0, 0, -110)),
        new(new RobotPartId("tool"), new Vector3(0, 0, -90))
    ];
}
