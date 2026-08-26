using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class SimpleArmMechanicalTeachingViewCatalog
{
    public static IReadOnlyList<MechanicalTeachingViewOption> Options { get; } =
    [
        new(
            MechanicalTeachingViewMode.Assembled,
            "Assembled desktop arm",
            "Compact serial arm with a rotating base, shoulder, elbow, wrist, and parallel gripper.",
            ["reach-and-transfer", "individual-joint-inspection"]),
        new(
            MechanicalTeachingViewMode.DriveSystem,
            "Drive system",
            "Transparent covers expose the base, shoulder, and elbow servos, reductions, and structural links.",
            ["reach-and-transfer", "individual-joint-inspection"]),
        new(
            MechanicalTeachingViewMode.MotionAxes,
            "Joint axes",
            "Guides identify vertical base yaw and the parallel shoulder and elbow pitch axes.",
            ["reach-and-transfer", "individual-joint-inspection"]),
        new(
            MechanicalTeachingViewMode.ExplodedAssembly,
            "Exploded assembly",
            "Separates the rotating base, joint drives, links, wrist, and gripper by mechanical group.",
            ["assembly-sequence"])
    ];

    public static IReadOnlyList<MechanicalMotionAxisGuide> MotionAxes { get; } =
    [
        new(MechanicalMotionAxis.Z, new(0, 0, 0.4f), new(0, 0, 4.2f), new RobotPartId("turntable")),
        new(MechanicalMotionAxis.Y, new(0, -2.1f, 2.2f), new(0, 2.1f, 2.2f), new RobotPartId("turntable")),
        new(MechanicalMotionAxis.Y, new(2.7f, -1.7f, 4.6f), new(2.7f, 1.7f, 4.6f), new RobotPartId("upper-arm"))
    ];

    public static IReadOnlyList<MechanicalExplodedPartOffset> ExplodedOffsets { get; } =
    [
        new(new RobotPartId("controller"), new Vector3(-130, 0, 0)),
        new(new RobotPartId("base-motor"), new Vector3(0, 0, -100)),
        new(new RobotPartId("turntable"), new Vector3(0, 0, 100)),
        new(new RobotPartId("shoulder-joint"), new Vector3(0, -120, 0)),
        new(new RobotPartId("upper-arm"), new Vector3(0, 120, 80)),
        new(new RobotPartId("elbow-joint"), new Vector3(0, -120, 80)),
        new(new RobotPartId("forearm"), new Vector3(0, 120, 60)),
        new(new RobotPartId("wrist"), new Vector3(100, 0, -50)),
        new(new RobotPartId("tool"), new Vector3(150, 0, -110))
    ];
}
