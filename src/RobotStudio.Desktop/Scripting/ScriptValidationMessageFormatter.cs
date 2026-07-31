using RobotStudio.Domain.Exceptions;
using RobotStudio.Scripting;

namespace RobotStudio.Desktop.Scripting;

public static class ScriptValidationMessageFormatter
{
    public static string Format(Exception exception) =>
        exception switch
        {
            ScriptParseException parseException => FormatParseError(parseException),
            PositionOutOfRangeException => FormatValidationError(
                "Physical limit exceeded",
                exception.Message,
                "Adjust the target position so it stays inside the robot workspace."),
            InvalidRobotCommandException => FormatValidationError(
                "Invalid robot command",
                exception.Message,
                "Check the command arguments and use positive speed or duration values when required."),
            InvalidOperationException => FormatValidationError(
                "Simulation or validation error",
                exception.Message,
                "Review whether this command belongs to the selected robot viewer."),
            ArgumentException => FormatValidationError(
                "Invalid argument",
                exception.Message,
                "Check the command syntax and numeric values."),
            FormatException => FormatValidationError(
                "Invalid script format",
                exception.Message,
                "Check the command syntax and numeric values."),
            _ => FormatValidationError(
                "Unexpected validation error",
                exception.Message,
                "Review the script and try validating it again.")
        };

    private static string FormatParseError(ScriptParseException exception) =>
        $"Script syntax error at line {exception.LineNumber}: {exception.Message} " +
        "Fix the highlighted command syntax and validate again.";

    private static string FormatValidationError(
        string title,
        string detail,
        string nextStep) =>
        $"{title}: {detail} {nextStep}";
}
