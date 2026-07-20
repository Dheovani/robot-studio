using System.Globalization;
using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public sealed class RobotScriptParser
{
    public RobotCommandSequence Parse(string script)
    {
        ArgumentNullException.ThrowIfNull(script);

        var commands = new List<RobotCommand>();
        var lines = script.Split(
            ["\r\n", "\n"],
            StringSplitOptions.None);

        for (var index = 0; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var line = lines[index].Trim();

            if (line.Length == 0)
            {
                continue;
            }

            commands.Add(ParseLine(lineNumber, line));
        }

        return new RobotCommandSequence(commands);
    }

    private static RobotCommand ParseLine(
        int lineNumber,
        string line)
    {
        var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var commandName = tokens[0].ToUpperInvariant();
        var arguments = tokens.Skip(1).ToArray();

        return commandName switch
        {
            "HOME" => ParseHome(lineNumber, line, arguments),
            "WAIT" => ParseWait(lineNumber, line, arguments),
            "MOVE" => ParseMove(lineNumber, line, arguments),
            _ => throw new ScriptParseException(lineNumber, line, $"Unknown command '{tokens[0]}'.")
        };
    }

    private static HomeCommand ParseHome(
        int lineNumber,
        string line,
        IReadOnlyCollection<string> arguments)
    {
        if (arguments.Count > 0)
        {
            throw new ScriptParseException(lineNumber, line, "HOME does not accept arguments.");
        }

        return new HomeCommand(CreateSource(lineNumber, line));
    }

    private static WaitCommand ParseWait(
        int lineNumber,
        string line,
        IReadOnlyList<string> arguments)
    {
        if (arguments.Count != 1)
        {
            throw new ScriptParseException(lineNumber, line, "WAIT requires one duration in milliseconds.");
        }

        var durationMilliseconds = ParseDouble(lineNumber, line, arguments[0], "WAIT duration");
        if (durationMilliseconds < 0)
        {
            throw new ScriptParseException(lineNumber, line, "WAIT duration cannot be negative.");
        }

        return new WaitCommand(
            TimeSpan.FromMilliseconds(durationMilliseconds),
            CreateSource(lineNumber, line));
    }

    private static MoveToCommand ParseMove(
        int lineNumber,
        string line,
        IReadOnlyList<string> arguments)
    {
        var values = ParseKeyValueArguments(lineNumber, line, arguments);

        var x = GetRequiredDouble(lineNumber, line, values, "X");
        var y = GetRequiredDouble(lineNumber, line, values, "Y");
        var z = GetRequiredDouble(lineNumber, line, values, "Z");

        double? requestedVelocity = values.TryGetValue("SPEED", out var speedText)
            ? ParseDouble(lineNumber, line, speedText, "SPEED")
            : null;

        return new MoveToCommand(
            new CartesianPosition(x, y, z),
            requestedVelocity,
            CreateSource(lineNumber, line));
    }

    private static RobotCommandSource CreateSource(
        int lineNumber,
        string line) =>
        new(lineNumber, line);

    private static Dictionary<string, string> ParseKeyValueArguments(
        int lineNumber,
        string line,
        IEnumerable<string> arguments)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var argument in arguments)
        {
            var parts = argument.Split('=', count: 2);
            if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
            {
                throw new ScriptParseException(lineNumber, line, $"Invalid argument '{argument}'. Expected NAME=VALUE.");
            }

            var key = parts[0].ToUpperInvariant();
            if (key is not ("X" or "Y" or "Z" or "SPEED"))
            {
                throw new ScriptParseException(lineNumber, line, $"Unknown MOVE argument '{parts[0]}'.");
            }

            if (!values.TryAdd(key, parts[1]))
            {
                throw new ScriptParseException(lineNumber, line, $"Duplicate MOVE argument '{parts[0]}'.");
            }
        }

        return values;
    }

    private static double GetRequiredDouble(
        int lineNumber,
        string line,
        IReadOnlyDictionary<string, string> values,
        string key)
    {
        if (!values.TryGetValue(key, out var value))
        {
            throw new ScriptParseException(lineNumber, line, $"MOVE requires {key}.");
        }

        return ParseDouble(lineNumber, line, value, key);
    }

    private static double ParseDouble(
        int lineNumber,
        string line,
        string text,
        string name)
    {
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new ScriptParseException(lineNumber, line, $"{name} must be a valid number.");
        }

        return value;
    }
}
