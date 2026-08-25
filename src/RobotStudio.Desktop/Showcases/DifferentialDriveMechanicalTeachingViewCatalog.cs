using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class DifferentialDriveMechanicalTeachingViewCatalog
{
    public static IReadOnlyList<MechanicalTeachingViewOption> Options { get; } =
    [
        new(
            MechanicalTeachingViewMode.Assembled,
            "Assembled service robot",
            "Complete round service robot with its chassis, drive units, electronics, and support caster.",
            ["drive-and-turn-tour", "turning-comparison"]),
        new(
            MechanicalTeachingViewMode.DriveSystem,
            "Drive system",
            "The chassis and cover become transparent to expose both motors, encoders, wheels, battery, and controller.",
            ["drive-and-turn-tour", "turning-comparison"]),
        new(
            MechanicalTeachingViewMode.MotionAxes,
            "Body frame",
            "Moving X/Y guides identify the rover body frame while its heading changes.",
            ["drive-and-turn-tour", "turning-comparison"]),
        new(
            MechanicalTeachingViewMode.ExplodedAssembly,
            "Exploded assembly",
            "Separates the cover, electronics, caster, and left/right drive units from the chassis.",
            ["assembly-sequence"])
    ];

    public static IReadOnlyList<MechanicalMotionAxisGuide> MotionAxes { get; } =
    [
        new(MechanicalMotionAxis.X, new(0, 0, 2.8f), new(4.2f, 0, 2.8f), new RobotPartId("base")),
        new(MechanicalMotionAxis.Y, new(0, 0, 2.8f), new(0, 3.4f, 2.8f), new RobotPartId("base"))
    ];

    public static IReadOnlyList<MechanicalExplodedPartOffset> ExplodedOffsets { get; } =
    [
        new(new RobotPartId("bumper"), new Vector3(0, 0, -70)),
        new(new RobotPartId("upper-shell"), new Vector3(0, 0, 180)),
        new(new RobotPartId("controller"), new Vector3(120, 0, 70)),
        new(new RobotPartId("battery"), new Vector3(-120, 0, 65)),
        new(new RobotPartId("left-motor"), new Vector3(0, -100, 0)),
        new(new RobotPartId("right-motor"), new Vector3(0, 100, 0)),
        new(new RobotPartId("caster"), new Vector3(-75, 0, -35))
    ];
}
