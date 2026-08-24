namespace RobotStudio.Desktop.Scripting;

public sealed record GCodeLineExplanation(
    int LineNumber,
    string Command,
    string Explanation);
