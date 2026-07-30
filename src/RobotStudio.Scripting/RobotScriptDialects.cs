namespace RobotStudio.Scripting;

public static class RobotScriptDialects
{
    public static RobotScriptDialectDescriptor SimpleDsl { get; } = new(
        RobotScriptDialectId.SimpleDsl,
        "Simple DSL",
        RobotScriptDialectStatus.Available,
        "Beginner-friendly RobotStudio command language for HOME, MOVE, DRIVE, SCARA, and WAIT.");

    public static RobotScriptDialectDescriptor GCode { get; } = new(
        RobotScriptDialectId.GCode,
        "G-code",
        RobotScriptDialectStatus.Planned,
        "Future industrial-style command dialect that will produce the same domain commands.");

    public static IReadOnlyList<RobotScriptDialectDescriptor> All { get; } =
    [
        SimpleDsl,
        GCode
    ];
}
