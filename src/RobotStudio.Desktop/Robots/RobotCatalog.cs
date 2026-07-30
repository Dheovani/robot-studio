namespace RobotStudio.Desktop.Robots;

public static class RobotCatalog
{
    public static readonly RobotFamilyDescriptor Cartesian = new(
        Id: "cartesian",
        Name: "Cartesian",
        Description: "Linear robots that move along orthogonal axes.");

    public static readonly RobotFamilyDescriptor Mobile = new(
        Id: "mobile",
        Name: "Mobile",
        Description: "Ground robots that move through an environment.");

    public static readonly RobotFamilyDescriptor Articulated = new(
        Id: "articulated",
        Name: "Articulated",
        Description: "Joint-based robots used to teach kinematics and arm motion.");

    public static readonly RobotFamilyDescriptor Parallel = new(
        Id: "parallel",
        Name: "Parallel",
        Description: "Robots whose end effector is moved by parallel link mechanisms.");

    public static readonly RobotFamilyDescriptor Aerial = new(
        Id: "aerial",
        Name: "Aerial",
        Description: "Flying robots that combine position, orientation, and attitude control.");

    public static IReadOnlyList<RobotTemplate> Templates { get; } =
    [
        new(
            Id: "cartesian-intro",
            Name: "Cartesian Robot",
            Family: Cartesian,
            Status: RobotAvailabilityStatus.Available,
            Complexity: RobotComplexityLevel.Introductory,
            Description: "The first functional RobotStudio model. It teaches linear X/Y/Z motion, deterministic simulation, DSL playback, and a didactic 3D viewer.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.Dsl,
                RobotCapability.ThreeDimensionalView,
                RobotCapability.ManualControl,
                RobotCapability.Playback,
                RobotCapability.PathPlanning,
                RobotCapability.WorkspaceVisualization
            ],
            Viewer: new RobotViewerDescriptor(
                RobotViewerKind.CartesianThreeDimensional,
                "Cartesian 3D Viewer")),

        new(
            Id: "xy-plotter",
            Name: "XY Plotter",
            Family: Cartesian,
            Status: RobotAvailabilityStatus.Available,
            Complexity: RobotComplexityLevel.Beginner,
            Description: "Two-axis drawing robot for teaching planar movement, path drawing, and command sequencing before full 3D motion.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.Dsl,
                RobotCapability.ThreeDimensionalView,
                RobotCapability.ManualControl,
                RobotCapability.Playback,
                RobotCapability.PathDrawing,
                RobotCapability.PathPlanning
            ],
            Viewer: new RobotViewerDescriptor(
                RobotViewerKind.XYPlotterTwoDimensional,
                "XY Plotter Viewer")),

        new(
            Id: "differential-drive",
            Name: "Differential Drive Robot",
            Family: Mobile,
            Status: RobotAvailabilityStatus.Available,
            Complexity: RobotComplexityLevel.Intermediate,
            Description: "Mobile robot model for teaching wheel-based movement, heading, turning behavior, and navigation-oriented simulation.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.Dsl,
                RobotCapability.ThreeDimensionalView,
                RobotCapability.ManualControl,
                RobotCapability.Playback,
                RobotCapability.PathPlanning,
                RobotCapability.Odometry
            ],
            Viewer: new RobotViewerDescriptor(
                RobotViewerKind.DifferentialDriveTwoDimensional,
                "Differential Drive 2D Viewer")),

        new(
            Id: "scara",
            Name: "SCARA Robot",
            Family: Articulated,
            Status: RobotAvailabilityStatus.Available,
            Complexity: RobotComplexityLevel.Intermediate,
            Description: "Selective-compliance arm for introducing joint motion, planar kinematics, and workspace limits.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.Dsl,
                RobotCapability.ThreeDimensionalView,
                RobotCapability.ManualControl,
                RobotCapability.Playback,
                RobotCapability.ForwardKinematics,
                RobotCapability.InverseKinematics,
                RobotCapability.WorkspaceVisualization
            ],
            Viewer: new RobotViewerDescriptor(
                RobotViewerKind.ScaraThreeDimensional,
                "SCARA 3D Viewer")),

        new(
            Id: "simple-articulated-arm",
            Name: "Simple Articulated Arm",
            Family: Articulated,
            Status: RobotAvailabilityStatus.Available,
            Complexity: RobotComplexityLevel.Advanced,
            Description: "Joint-based arm for teaching links, joints, forward kinematics, and coordinated motion.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.Dsl,
                RobotCapability.TwoDimensionalView,
                RobotCapability.ManualControl,
                RobotCapability.Playback,
                RobotCapability.ForwardKinematics,
                RobotCapability.WorkspaceVisualization
            ],
            Viewer: new RobotViewerDescriptor(
                RobotViewerKind.SimpleArmThreeDimensional,
                "Simple Arm 3D Viewer")),

        new(
            Id: "delta-planned",
            Name: "Delta Robot",
            Family: Parallel,
            Status: RobotAvailabilityStatus.Planned,
            Complexity: RobotComplexityLevel.Advanced,
            Description: "Planned parallel robot for teaching constrained workspaces, fast end-effector motion, and parallel mechanism architecture.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.Dsl,
                RobotCapability.ThreeDimensionalView,
                RobotCapability.Playback,
                RobotCapability.ForwardKinematics,
                RobotCapability.InverseKinematics,
                RobotCapability.WorkspaceVisualization
            ],
            Viewer: PlannedViewer()),

        new(
            Id: "drone-planned",
            Name: "Drone",
            Family: Aerial,
            Status: RobotAvailabilityStatus.Planned,
            Complexity: RobotComplexityLevel.Advanced,
            Description: "Planned aerial robot for teaching 3D position, orientation, attitude control, flight paths, and state-based movement.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.Dsl,
                RobotCapability.ThreeDimensionalView,
                RobotCapability.ManualControl,
                RobotCapability.Playback,
                RobotCapability.PathPlanning,
                RobotCapability.AttitudeControl
            ],
            Viewer: PlannedViewer()),

        new(
            Id: "six-dof-industrial-arm-planned",
            Name: "6-DOF Industrial Arm",
            Family: Articulated,
            Status: RobotAvailabilityStatus.Planned,
            Complexity: RobotComplexityLevel.Expert,
            Description: "Planned industrial arm for advanced lessons about six-degree-of-freedom motion, tooling, kinematics, and production-style robot architecture.",
            Capabilities:
            [
                RobotCapability.Simulation,
                RobotCapability.Dsl,
                RobotCapability.ThreeDimensionalView,
                RobotCapability.ManualControl,
                RobotCapability.Playback,
                RobotCapability.ForwardKinematics,
                RobotCapability.InverseKinematics,
                RobotCapability.WorkspaceVisualization,
                RobotCapability.FutureGCode,
                RobotCapability.HardwareCommunication
            ],
            Viewer: PlannedViewer())
    ];

    public static bool CanOpen(RobotTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return template.Status == RobotAvailabilityStatus.Available &&
               template.Viewer.Kind != RobotViewerKind.None;
    }

    private static RobotViewerDescriptor PlannedViewer() =>
        new(RobotViewerKind.None, "Planned Viewer");
}
