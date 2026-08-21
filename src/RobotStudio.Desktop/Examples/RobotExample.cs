using RobotStudio.Desktop.Robots;

namespace RobotStudio.Desktop.Examples;

public sealed record RobotExample(
    RobotViewerKind ViewerKind,
    string Name,
    string Description,
    string Script,
    string? GCodeScript = null,
    RobotExampleExpectedResult ExpectedResult = RobotExampleExpectedResult.Succeeds)
{
    public override string ToString() => Name;
}
