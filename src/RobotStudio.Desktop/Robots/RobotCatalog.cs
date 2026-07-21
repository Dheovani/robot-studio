namespace RobotStudio.Desktop.Robots;

public static class RobotCatalog
{
    public static readonly RobotFamilyDescriptor Cartesian = new(
        Id: "cartesian",
        Name: "Cartesian Robot",
        Description: "Linear X/Y/Z robot for introductory motion and scripting lessons.");

    public static readonly RobotFamilyDescriptor ArticulatedArm = new(
        Id: "articulated-arm",
        Name: "Articulated Arm",
        Description: "Joint-based robot planned for future kinematics lessons.");

    public static readonly RobotFamilyDescriptor Drone = new(
        Id: "drone",
        Name: "Drone",
        Description: "Flying robot planned for future spatial movement and orientation lessons.");

    public static IReadOnlyList<RobotTemplate> Templates { get; } =
    [
        new(
            Id: "cartesian-intro",
            Name: "Cartesian Robot",
            Family: Cartesian,
            Status: RobotAvailabilityStatus.Available,
            Description: "The first functional RobotStudio model. It supports deterministic simulation, DSL playback, and a didactic 3D viewer.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.ScriptExecution,
                RobotCapability.ThreeDimensionalView
            ],
            Viewer: new RobotViewerDescriptor(
                RobotViewerKind.CartesianThreeDimensional,
                "Cartesian 3D Viewer")),

        new(
            Id: "articulated-arm-planned",
            Name: "Articulated Arm",
            Family: ArticulatedArm,
            Status: RobotAvailabilityStatus.Planned,
            Description: "Planned model for joint-based motion, inverse kinematics, and arm mechanics.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.ThreeDimensionalView,
                RobotCapability.ManualControl
            ],
            Viewer: new RobotViewerDescriptor(
                RobotViewerKind.None,
                "Planned Viewer")),

        new(
            Id: "drone-planned",
            Name: "Drone",
            Family: Drone,
            Status: RobotAvailabilityStatus.Planned,
            Description: "Planned model for 3D position, orientation, flight paths, and state-based movement.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.ThreeDimensionalView,
                RobotCapability.ManualControl
            ],
            Viewer: new RobotViewerDescriptor(
                RobotViewerKind.None,
                "Planned Viewer"))
    ];
}
