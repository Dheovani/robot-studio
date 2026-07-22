namespace RobotStudio.Scripting;

public sealed record RobotScriptDialectDescriptor(
    RobotScriptDialectId Id,
    string Name,
    RobotScriptDialectStatus Status,
    string Description);
