using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class DroneMechanicalTeachingViewCatalog
{
    public static IReadOnlyList<MechanicalTeachingViewOption> Options { get; } =
    [
        new(
            MechanicalTeachingViewMode.Assembled,
            "Assembled quadcopter",
            "X-frame aerial robot with four visible motor and propeller assemblies, landing gear, camera, and protective shell.",
            ["flight-and-attitude-tour", "motor-pair-inspection"]),
        new(
            MechanicalTeachingViewMode.DriveSystem,
            "Avionics and power",
            "Transparent body panels expose the battery, power distribution, flight controller, IMU, and motor wiring path.",
            ["flight-and-attitude-tour", "motor-pair-inspection"]),
        new(
            MechanicalTeachingViewMode.MotionAxes,
            "Body and attitude axes",
            "Moving X/Y/Z guides identify the aircraft body frame while roll, pitch, and yaw change its orientation.",
            ["flight-and-attitude-tour", "motor-pair-inspection"]),
        new(
            MechanicalTeachingViewMode.ExplodedAssembly,
            "Exploded assembly",
            "Separates the shell, avionics, battery, arms, propulsion units, camera, landing gear, and body frame.",
            ["assembly-sequence"])
    ];

    public static IReadOnlyList<MechanicalMotionAxisGuide> MotionAxes { get; } =
    [
        new(MechanicalMotionAxis.X, new(-2.2f, 0, 0.45f), new(2.2f, 0, 0.45f), new RobotPartId("airframe")),
        new(MechanicalMotionAxis.Y, new(0, -2.2f, 0.45f), new(0, 2.2f, 0.45f), new RobotPartId("airframe")),
        new(MechanicalMotionAxis.Z, new(0, 0, -1.2f), new(0, 0, 2.1f), new RobotPartId("airframe"))
    ];

    public static IReadOnlyList<MechanicalExplodedPartOffset> ExplodedOffsets { get; } =
    [
        new(new RobotPartId("shell"), new Vector3(0, 0, 120)),
        new(new RobotPartId("battery"), new Vector3(0, 0, -110)),
        new(new RobotPartId("flight-controller"), new Vector3(0, 0, 75)),
        new(new RobotPartId("camera"), new Vector3(0, -90, -50)),
        new(new RobotPartId("landing-gear"), new Vector3(0, 0, -90)),
        new(new RobotPartId("arm-front-left"), new Vector3(-90, -90, 20)),
        new(new RobotPartId("arm-front-right"), new Vector3(90, -90, 20)),
        new(new RobotPartId("arm-rear-left"), new Vector3(-90, 90, 20)),
        new(new RobotPartId("arm-rear-right"), new Vector3(90, 90, 20))
    ];
}
