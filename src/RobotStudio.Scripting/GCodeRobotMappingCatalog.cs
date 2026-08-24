namespace RobotStudio.Scripting;

public enum GCodeRobotTarget
{
    CartesianRobot,
    XYPlotter,
    DifferentialDriveRobot,
    ScaraRobot,
    SimpleArticulatedArm,
    DeltaRobot,
    Drone,
    IndustrialArm6Dof
}

public enum GCodeRobotMappingStatus
{
    Available,
    Planned,
    NotApplicable
}

public sealed record GCodeRobotMappingDescriptor(
    GCodeRobotTarget Target,
    GCodeRobotMappingStatus Status,
    IReadOnlyList<char> ToolSpaceWords,
    string Rationale);

public static class GCodeRobotMappingCatalog
{
    public static IReadOnlyList<GCodeRobotMappingDescriptor> All { get; } =
    [
        Available(
            GCodeRobotTarget.CartesianRobot,
            ['X', 'Y', 'Z'],
            "X, Y, and Z map directly to the robot's linear axes."),
        Available(
            GCodeRobotTarget.XYPlotter,
            ['X', 'Y'],
            "X and Y map directly to the plotter axes while Z remains fixed by its profile."),
        NotApplicable(
            GCodeRobotTarget.DifferentialDriveRobot,
            "CNC tool-space G-code does not represent wheel motion, heading, or odometry clearly."),
        Available(
            GCodeRobotTarget.ScaraRobot,
            ['X', 'Y'],
            "X/Y tool-space paths use sampled linear planning and deterministic elbow-down inverse kinematics."),
        Planned(
            GCodeRobotTarget.SimpleArticulatedArm,
            ['X', 'Y', 'Z', 'A', 'B', 'C'],
            "Requires inverse kinematics and Cartesian tool-path planning before G1 can remain linear."),
        Planned(
            GCodeRobotTarget.DeltaRobot,
            ['X', 'Y', 'Z'],
            "Requires inverse kinematics and Cartesian tool-path planning for its parallel actuators."),
        NotApplicable(
            GCodeRobotTarget.Drone,
            "CNC tool-space G-code does not represent flight attitude, dynamics, or navigation semantics clearly."),
        Planned(
            GCodeRobotTarget.IndustrialArm6Dof,
            ['X', 'Y', 'Z', 'A', 'B', 'C'],
            "Requires full pose inverse kinematics, configuration selection, and Cartesian tool-path planning.")
    ];

    public static GCodeRobotMappingDescriptor Get(GCodeRobotTarget target) =>
        All.Single(mapping => mapping.Target == target);

    private static GCodeRobotMappingDescriptor Available(
        GCodeRobotTarget target,
        IReadOnlyList<char> words,
        string rationale) =>
        new(target, GCodeRobotMappingStatus.Available, words, rationale);

    private static GCodeRobotMappingDescriptor Planned(
        GCodeRobotTarget target,
        IReadOnlyList<char> words,
        string rationale) =>
        new(target, GCodeRobotMappingStatus.Planned, words, rationale);

    private static GCodeRobotMappingDescriptor NotApplicable(
        GCodeRobotTarget target,
        string rationale) =>
        new(target, GCodeRobotMappingStatus.NotApplicable, Array.Empty<char>(), rationale);
}
