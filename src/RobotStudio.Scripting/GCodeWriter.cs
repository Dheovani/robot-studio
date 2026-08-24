using System.Globalization;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public static class GCodeWriter
{
    public static string Write(RobotCommandSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);

        return string.Join(
            Environment.NewLine,
            new[] { "G21", "G90" }.Concat(sequence.Commands.Select(WriteCommand)));
    }

    private static string WriteCommand(RobotCommand command) => command switch
    {
        HomeCommand => "G28",
        MoveToCommand move => WriteMove(move),
        WaitCommand wait => $"G4 P{FormatNumber(wait.Duration.TotalMilliseconds)}",
        _ => throw new NotSupportedException($"G-code output does not support {command.GetType().Name}.")
    };

    private static string WriteMove(MoveToCommand command)
    {
        var result =
            $"G1 X{FormatNumber(command.TargetPosition.X)} " +
            $"Y{FormatNumber(command.TargetPosition.Y)} " +
            $"Z{FormatNumber(command.TargetPosition.Z)}";

        return command.RequestedVelocityMillimetersPerSecond is { } velocity
            ? $"{result} F{FormatNumber(velocity * 60d)}"
            : result;
    }

    private static string FormatNumber(double value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);
}
