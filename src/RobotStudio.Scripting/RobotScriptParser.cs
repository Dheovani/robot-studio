using System.Globalization;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Scripting;

public sealed class RobotScriptParser : IRobotScriptDialect
{
    public RobotScriptDialectDescriptor Descriptor => RobotScriptDialects.SimpleDsl;

    public RobotCommandSequence Parse(
        string script,
        RobotScriptParseContext? context = null)
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
            "RESET" => ParseResetFault(lineNumber, line, arguments),
            "WAIT" => ParseWait(lineNumber, line, arguments),
            "MOVE" => ParseMove(lineNumber, line, arguments),
            "DRIVE" => ParseDrive(lineNumber, line, arguments),
            "SCARA" => ParseScara(lineNumber, line, arguments),
            "ARM" => ParseSimpleArm(lineNumber, line, arguments),
            "ARM6" => ParseIndustrialArm(lineNumber, line, arguments),
            "DELTA" => ParseDelta(lineNumber, line, arguments),
            "DRONE" => ParseDrone(lineNumber, line, arguments),
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

    private static ResetFaultCommand ParseResetFault(
        int lineNumber,
        string line,
        IReadOnlyCollection<string> arguments)
    {
        if (arguments.Count > 0)
        {
            throw new ScriptParseException(lineNumber, line, "RESET does not accept arguments.");
        }

        return new ResetFaultCommand(CreateSource(lineNumber, line));
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

    private static DifferentialDriveMoveCommand ParseDrive(
        int lineNumber,
        string line,
        IReadOnlyList<string> arguments)
    {
        var values = ParseKeyValueArguments(
            lineNumber,
            line,
            arguments,
            ["X", "Y", "HEADING", "LIN", "ANG"]);

        var x = GetRequiredDouble(lineNumber, line, values, "X", "DRIVE");
        var y = GetRequiredDouble(lineNumber, line, values, "Y", "DRIVE");
        var heading = GetRequiredDouble(lineNumber, line, values, "HEADING", "DRIVE");

        double? requestedLinearVelocity = values.TryGetValue("LIN", out var linearText)
            ? ParseDouble(lineNumber, line, linearText, "LIN")
            : null;
        double? requestedAngularVelocity = values.TryGetValue("ANG", out var angularText)
            ? ParseDouble(lineNumber, line, angularText, "ANG")
            : null;

        return new DifferentialDriveMoveCommand(
            new DifferentialDrivePose(x, y, heading),
            requestedLinearVelocity,
            requestedAngularVelocity,
            CreateSource(lineNumber, line));
    }

    private static ScaraMoveJointsCommand ParseScara(
        int lineNumber,
        string line,
        IReadOnlyList<string> arguments)
    {
        var values = ParseKeyValueArguments(
            lineNumber,
            line,
            arguments,
            ["SHOULDER", "ELBOW", "SPEED"]);

        var shoulder = GetRequiredDouble(lineNumber, line, values, "SHOULDER", "SCARA");
        var elbow = GetRequiredDouble(lineNumber, line, values, "ELBOW", "SCARA");

        double? requestedJointVelocity = values.TryGetValue("SPEED", out var speedText)
            ? ParseDouble(lineNumber, line, speedText, "SPEED")
            : null;

        return new ScaraMoveJointsCommand(
            new ScaraJointPosition(shoulder, elbow),
            requestedJointVelocity,
            CreateSource(lineNumber, line));
    }

    private static SimpleArmMoveJointsCommand ParseSimpleArm(
        int lineNumber,
        string line,
        IReadOnlyList<string> arguments)
    {
        var values = ParseKeyValueArguments(
            lineNumber,
            line,
            arguments,
            ["BASE", "SHOULDER", "ELBOW", "SPEED"]);

        var baseDegrees = GetRequiredDouble(lineNumber, line, values, "BASE", "ARM");
        var shoulder = GetRequiredDouble(lineNumber, line, values, "SHOULDER", "ARM");
        var elbow = GetRequiredDouble(lineNumber, line, values, "ELBOW", "ARM");

        double? requestedJointVelocity = values.TryGetValue("SPEED", out var speedText)
            ? ParseDouble(lineNumber, line, speedText, "SPEED")
            : null;

        return new SimpleArmMoveJointsCommand(
            new SimpleArmJointPosition(baseDegrees, shoulder, elbow),
            requestedJointVelocity,
            CreateSource(lineNumber, line));
    }

    private static IndustrialArmMoveJointsCommand ParseIndustrialArm(
        int lineNumber,
        string line,
        IReadOnlyList<string> arguments)
    {
        var values = ParseKeyValueArguments(
            lineNumber,
            line,
            arguments,
            ["J1", "J2", "J3", "J4", "J5", "J6", "SPEED"]);

        var target = new IndustrialArmJointPosition(
            GetRequiredDouble(lineNumber, line, values, "J1", "ARM6"),
            GetRequiredDouble(lineNumber, line, values, "J2", "ARM6"),
            GetRequiredDouble(lineNumber, line, values, "J3", "ARM6"),
            GetRequiredDouble(lineNumber, line, values, "J4", "ARM6"),
            GetRequiredDouble(lineNumber, line, values, "J5", "ARM6"),
            GetRequiredDouble(lineNumber, line, values, "J6", "ARM6"));
        double? requestedJointVelocity = values.TryGetValue("SPEED", out var speedText)
            ? ParseDouble(lineNumber, line, speedText, "SPEED")
            : null;

        return new IndustrialArmMoveJointsCommand(
            target,
            requestedJointVelocity,
            CreateSource(lineNumber, line));
    }

    private static DeltaMoveActuatorsCommand ParseDelta(
        int lineNumber,
        string line,
        IReadOnlyList<string> arguments)
    {
        var values = ParseKeyValueArguments(
            lineNumber,
            line,
            arguments,
            ["A", "B", "C", "SPEED"]);

        var actuatorA = GetRequiredDouble(lineNumber, line, values, "A", "DELTA");
        var actuatorB = GetRequiredDouble(lineNumber, line, values, "B", "DELTA");
        var actuatorC = GetRequiredDouble(lineNumber, line, values, "C", "DELTA");

        double? requestedActuatorVelocity = values.TryGetValue("SPEED", out var speedText)
            ? ParseDouble(lineNumber, line, speedText, "SPEED")
            : null;

        return new DeltaMoveActuatorsCommand(
            new DeltaActuatorPosition(actuatorA, actuatorB, actuatorC),
            requestedActuatorVelocity,
            CreateSource(lineNumber, line));
    }

    private static DroneMoveCommand ParseDrone(
        int lineNumber,
        string line,
        IReadOnlyList<string> arguments)
    {
        var values = ParseKeyValueArguments(
            lineNumber,
            line,
            arguments,
            ["X", "Y", "Z", "ROLL", "PITCH", "YAW", "SPEED", "ATTITUDE_SPEED", "YAW_SPEED"]);

        var x = GetRequiredDouble(lineNumber, line, values, "X", "DRONE");
        var y = GetRequiredDouble(lineNumber, line, values, "Y", "DRONE");
        var z = GetRequiredDouble(lineNumber, line, values, "Z", "DRONE");
        var yaw = GetRequiredDouble(lineNumber, line, values, "YAW", "DRONE");
        var roll = values.TryGetValue("ROLL", out var rollText)
            ? ParseDouble(lineNumber, line, rollText, "ROLL")
            : 0;
        var pitch = values.TryGetValue("PITCH", out var pitchText)
            ? ParseDouble(lineNumber, line, pitchText, "PITCH")
            : 0;

        double? requestedLinearVelocity = values.TryGetValue("SPEED", out var speedText)
            ? ParseDouble(lineNumber, line, speedText, "SPEED")
            : null;
        double? requestedYawVelocity = values.TryGetValue("YAW_SPEED", out var yawSpeedText)
            ? ParseDouble(lineNumber, line, yawSpeedText, "YAW_SPEED")
            : null;
        double? requestedAttitudeVelocity = values.TryGetValue("ATTITUDE_SPEED", out var attitudeSpeedText)
            ? ParseDouble(lineNumber, line, attitudeSpeedText, "ATTITUDE_SPEED")
            : null;

        return new DroneMoveCommand(
            new DronePose(
                XMillimeters: x,
                YMillimeters: y,
                ZMillimeters: z,
                YawDegrees: yaw,
                RollDegrees: roll,
                PitchDegrees: pitch),
            requestedLinearVelocity,
            requestedYawVelocity,
            CreateSource(lineNumber, line),
            requestedAttitudeVelocity);
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
        var values = ParseKeyValueArguments(
            lineNumber,
            line,
            arguments,
            ["X", "Y", "Z", "SPEED"]);

        return values;
    }

    private static Dictionary<string, string> ParseKeyValueArguments(
        int lineNumber,
        string line,
        IEnumerable<string> arguments,
        IReadOnlyCollection<string> allowedKeys)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var allowed = allowedKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var argument in arguments)
        {
            var parts = argument.Split('=', count: 2);
            if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
            {
                throw new ScriptParseException(lineNumber, line, $"Invalid argument '{argument}'. Expected NAME=VALUE.");
            }

            var key = parts[0].ToUpperInvariant();
            if (!allowed.Contains(key))
            {
                throw new ScriptParseException(lineNumber, line, $"Unknown {line.Split(' ', 2)[0].ToUpperInvariant()} argument '{parts[0]}'.");
            }

            if (!values.TryAdd(key, parts[1]))
            {
                throw new ScriptParseException(lineNumber, line, $"Duplicate {line.Split(' ', 2)[0].ToUpperInvariant()} argument '{parts[0]}'.");
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

    private static double GetRequiredDouble(
        int lineNumber,
        string line,
        IReadOnlyDictionary<string, string> values,
        string key,
        string commandName)
    {
        if (!values.TryGetValue(key, out var value))
        {
            throw new ScriptParseException(lineNumber, line, $"{commandName} requires {key}.");
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
