using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class ScaraMechanicalTeachingViewCatalog
{
    public static IReadOnlyList<MechanicalTeachingViewOption> Options { get; } =
    [
        new(
            MechanicalTeachingViewMode.Assembled,
            "Assembled SCARA",
            "Compact selective-compliance arm with two planar joints, a vertical wrist, and a parallel gripper.",
            ["pick-and-place-cycle", "individual-joint-inspection"]),
        new(
            MechanicalTeachingViewMode.DriveSystem,
            "Drive system",
            "Transparent covers expose the shoulder and elbow servos, reductions, rigid links, and vertical actuator.",
            ["pick-and-place-cycle", "individual-joint-inspection"]),
        new(
            MechanicalTeachingViewMode.MotionAxes,
            "Joint and tool axes",
            "Planar X/Y guides establish the work plane while the blue Z guide follows the vertical tool actuator.",
            ["pick-and-place-cycle", "individual-joint-inspection"]),
        new(
            MechanicalTeachingViewMode.ExplodedAssembly,
            "Exploded assembly",
            "Separates the controller, drive units, links, covers, vertical actuator, and gripper by mechanical group.",
            ["assembly-sequence"])
    ];

    public static IReadOnlyList<MechanicalMotionAxisGuide> MotionAxes { get; } =
    [
        new(MechanicalMotionAxis.X, new(-1.2f, 0, 0.4f), new(7.2f, 0, 0.4f)),
        new(MechanicalMotionAxis.Y, new(0, -3.4f, 0.4f), new(0, 3.4f, 0.4f)),
        new(MechanicalMotionAxis.Z, new(6.15f, 0, 1.4f), new(6.15f, 0, 5.8f), new RobotPartId("z-actuator"))
    ];

    public static IReadOnlyList<MechanicalExplodedPartOffset> ExplodedOffsets { get; } =
    [
        new(new RobotPartId("controller"), new Vector3(-130, 0, 0)),
        new(new RobotPartId("shoulder-motor"), new Vector3(0, 0, -120)),
        new(new RobotPartId("shoulder-transmission"), new Vector3(0, 0, 110)),
        new(new RobotPartId("first-link"), new Vector3(0, -130, 80)),
        new(new RobotPartId("elbow-joint"), new Vector3(0, 120, 110)),
        new(new RobotPartId("second-link"), new Vector3(0, -120, 70)),
        new(new RobotPartId("z-actuator"), new Vector3(100, 0, -100)),
        new(new RobotPartId("tool"), new Vector3(100, 0, -190))
    ];
}
