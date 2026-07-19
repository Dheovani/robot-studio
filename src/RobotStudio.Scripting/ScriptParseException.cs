namespace RobotStudio.Scripting;

public sealed class ScriptParseException : FormatException
{
    public ScriptParseException(
        int lineNumber,
        string lineText,
        string message)
        : base($"Line {lineNumber}: {message}")
    {
        LineNumber = lineNumber;
        LineText = lineText;
    }

    public int LineNumber { get; }

    public string LineText { get; }
}
