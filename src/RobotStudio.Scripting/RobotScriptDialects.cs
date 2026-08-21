namespace RobotStudio.Scripting;

public static class RobotScriptDialects
{
    public static RobotScriptDialectDescriptor SimpleDsl { get; } = new(
        RobotScriptDialectId.SimpleDsl,
        "Simple DSL",
        RobotScriptDialectStatus.Available,
        "Beginner-friendly RobotStudio command language for HOME, RESET, MOVE, DRIVE, SCARA, ARM, ARM6, DELTA, DRONE, and WAIT.");

    public static RobotScriptDialectDescriptor GCode { get; } = new(
        RobotScriptDialectId.GCode,
        "G-code",
        RobotScriptDialectStatus.Available,
        "Introductory G-code subset for G28 homing, G1 linear movement, and G4 dwell commands.");

    public static IReadOnlyList<RobotScriptDialectDescriptor> All { get; } =
    [
        SimpleDsl,
        GCode
    ];
}
