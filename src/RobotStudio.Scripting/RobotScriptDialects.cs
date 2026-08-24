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
        "Introductory Cartesian G-code subset for millimeter units, homing, linear movement, dwell, and absolute/relative positioning.");

    public static IReadOnlyList<RobotScriptDialectDescriptor> All { get; } =
    [
        SimpleDsl,
        GCode
    ];
}
