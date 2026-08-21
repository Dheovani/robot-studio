using RobotStudio.Desktop.Robots;

namespace RobotStudio.Desktop.Examples;

public sealed record RobotExample(
    RobotViewerKind ViewerKind,
    string Name,
    string Description,
    string Script,
    string? GCodeScript = null)
{
    public override string ToString() => Name;
}
