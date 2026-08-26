using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class IndustrialArmMechanicalTeachingViewCatalog
{
    public static IReadOnlyList<MechanicalTeachingViewOption> Options { get; } =
    [
        new(
            MechanicalTeachingViewMode.Assembled,
            "Assembled industrial arm",
            "Six-axis serial manipulator with a rotating base, load-bearing arm, compact wrist, and parallel gripper.",
            ["coordinated-pick-tour", "wrist-orientation-tour"]),
        new(
            MechanicalTeachingViewMode.DriveSystem,
            "Joint drive system",
            "Transparent covers expose the six rotary joints, motor housings, reduction stages, and serial load path.",
            ["coordinated-pick-tour", "wrist-orientation-tour"]),
        new(
            MechanicalTeachingViewMode.MotionAxes,
            "Six joint axes",
            "Guides identify base yaw, shoulder and elbow pitch, forearm roll, wrist bend, and tool roll.",
            ["coordinated-pick-tour", "wrist-orientation-tour"]),
        new(
            MechanicalTeachingViewMode.ExplodedAssembly,
            "Exploded assembly",
            "Separates the pedestal, shoulder, upper arm, elbow, forearm, three-axis wrist, and gripper.",
            ["assembly-sequence"])
    ];

    public static IReadOnlyList<MechanicalMotionAxisGuide> MotionAxes { get; } =
    [
        new(MechanicalMotionAxis.Z, new(0, 0, 0.3f), new(0, 0, 2.8f), new RobotPartId("j1-turntable")),
        new(MechanicalMotionAxis.Y, new(0, -1.8f, 2.2f), new(0, 1.8f, 2.2f), new RobotPartId("j1-turntable")),
        new(MechanicalMotionAxis.Y, new(1.4f, -1.6f, 5.2f), new(1.4f, 1.6f, 5.2f), new RobotPartId("upper-arm")),
        new(MechanicalMotionAxis.X, new(3.35f, 0, 5.2f), new(4.65f, 0, 5.2f), new RobotPartId("forearm")),
        new(MechanicalMotionAxis.Y, new(4.65f, -1.2f, 5.2f), new(4.65f, 1.2f, 5.2f), new RobotPartId("wrist-roll-housing")),
        new(MechanicalMotionAxis.X, new(4.9f, 0, 5.2f), new(5.8f, 0, 5.2f), new RobotPartId("wrist-bend-housing"))
    ];

    public static IReadOnlyList<MechanicalExplodedPartOffset> ExplodedOffsets { get; } =
    [
        new(new RobotPartId("controller"), new Vector3(-150, 0, 0)),
        new(new RobotPartId("j1-turntable"), new Vector3(0, 0, 100)),
        new(new RobotPartId("j2-shoulder"), new Vector3(0, -140, 20)),
        new(new RobotPartId("upper-arm"), new Vector3(0, 130, 80)),
        new(new RobotPartId("j3-elbow"), new Vector3(0, -140, 80)),
        new(new RobotPartId("forearm"), new Vector3(0, 140, 60)),
        new(new RobotPartId("j4-wrist-roll"), new Vector3(100, 0, 0)),
        new(new RobotPartId("j5-wrist-bend"), new Vector3(105, -90, 0)),
        new(new RobotPartId("j6-tool-roll"), new Vector3(120, 90, 0)),
        new(new RobotPartId("tool"), new Vector3(170, 0, 0))
    ];
}
