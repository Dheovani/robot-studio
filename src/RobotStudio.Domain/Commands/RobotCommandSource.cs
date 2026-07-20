namespace RobotStudio.Domain.Commands;

public sealed record RobotCommandSource
{
    public RobotCommandSource(
        int lineNumber,
        string text)
    {
        if (lineNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lineNumber), "Command source line number must be greater than zero.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        LineNumber = lineNumber;
        Text = text;
    }

    public int LineNumber { get; }

    public string Text { get; }
}
