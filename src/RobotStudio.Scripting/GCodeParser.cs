using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public sealed partial class GCodeParser : IRobotScriptDialect
{
    private readonly IGCodeCommandMapper commandMapper;

    public GCodeParser()
        : this(new CartesianGCodeCommandMapper())
    {
    }

    public GCodeParser(IGCodeCommandMapper commandMapper)
    {
        ArgumentNullException.ThrowIfNull(commandMapper);
        this.commandMapper = commandMapper;
    }

    public RobotScriptDialectDescriptor Descriptor => RobotScriptDialects.GCode;

    public RobotCommandSequence Parse(
        string script,
        RobotScriptParseContext? context = null) =>
        Compile(script, context).Commands;

    public RobotScriptCompilation Compile(
        string script,
        RobotScriptParseContext? context = null) =>
        commandMapper.Map(CompileProgram(script), context);

    public GCodeProgram CompileProgram(string script)
    {
        ArgumentNullException.ThrowIfNull(script);

        var instructions = new List<GCodeInstruction>();
        var lines = script.Split(["\r\n", "\n"], StringSplitOptions.None);

        for (var index = 0; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var sourceText = lines[index].Trim();
            var commandText = RemoveComments(lineNumber, sourceText);

            if (commandText.Length == 0)
            {
                continue;
            }

            var instruction = ParseLine(
                lineNumber,
                sourceText,
                commandText);
            instructions.Add(instruction);
        }

        return new GCodeProgram(instructions);
    }

    private static GCodeInstruction ParseLine(
        int lineNumber,
        string sourceText,
        string commandText)
    {
        var words = Tokenize(lineNumber, sourceText, commandText);
        var commandIndex = words[0].Letter == 'N' ? 1 : 0;

        if (commandIndex == 1)
        {
            ValidateLineNumber(lineNumber, sourceText, words[0].Value);
        }

        if (commandIndex >= words.Count || words[commandIndex].Letter != 'G')
        {
            throw new ScriptParseException(lineNumber, sourceText, "Expected a G-code command such as G28, G1, G4, G21, G90, or G91.");
        }

        var code = ParseInteger(lineNumber, sourceText, words[commandIndex].Value, "G code");
        var arguments = words.Skip(commandIndex + 1).ToArray();

        if (code is 90 or 91)
        {
            var mode = code == 90
                ? RobotScriptPositioningMode.Absolute
                : RobotScriptPositioningMode.Relative;
            ValidateNoArguments(
                lineNumber,
                sourceText,
                arguments,
                $"G{code} does not accept arguments.");
            return new GCodePositioningModeInstruction(
                CreateSource(lineNumber, sourceText),
                mode);
        }

        if (code == 20)
        {
            throw new ScriptParseException(
                lineNumber,
                sourceText,
                "G20 inch units are not supported. RobotStudio uses millimeters; use G21 instead.");
        }

        return code switch
        {
            1 => ParseLinearMove(lineNumber, sourceText, arguments),
            4 => ParseDwell(lineNumber, sourceText, arguments),
            21 => ParseMillimeterUnits(lineNumber, sourceText, arguments),
            28 => ParseHome(lineNumber, sourceText, arguments),
            _ => throw new ScriptParseException(lineNumber, sourceText, $"Unsupported G-code command 'G{code}'. Supported commands are G28, G1, G4, G21, G90, and G91.")
        };
    }

    private static GCodeHomeInstruction ParseHome(
        int lineNumber,
        string sourceText,
        IReadOnlyCollection<GCodeWord> arguments)
    {
        ValidateNoArguments(
            lineNumber,
            sourceText,
            arguments,
            "G28 does not accept axis arguments in the introductory RobotStudio dialect.");

        return new GCodeHomeInstruction(CreateSource(lineNumber, sourceText));
    }

    private static GCodeLinearMoveInstruction ParseLinearMove(
        int lineNumber,
        string sourceText,
        IReadOnlyCollection<GCodeWord> arguments)
    {
        var values = BuildArgumentMap(lineNumber, sourceText, arguments, ['X', 'Y', 'Z', 'A', 'B', 'C', 'F']);
        if (!values.ContainsKey('X') &&
            !values.ContainsKey('Y') &&
            !values.ContainsKey('Z') &&
            !values.ContainsKey('A') &&
            !values.ContainsKey('B') &&
            !values.ContainsKey('C'))
        {
            throw new ScriptParseException(
                lineNumber,
                sourceText,
                "G1 requires at least one X, Y, Z, A, B, or C coordinate.");
        }

        double? feedRate = null;

        if (values.TryGetValue('F', out var feedRateText))
        {
            feedRate = ParseDouble(lineNumber, sourceText, feedRateText, "F feed rate");
            if (feedRate <= 0)
            {
                throw new ScriptParseException(lineNumber, sourceText, "G1 F feed rate must be greater than zero millimeters per minute.");
            }
        }

        return new GCodeLinearMoveInstruction(
            CreateSource(lineNumber, sourceText),
            GetOptionalDouble(lineNumber, sourceText, values, 'X'),
            GetOptionalDouble(lineNumber, sourceText, values, 'Y'),
            GetOptionalDouble(lineNumber, sourceText, values, 'Z'),
            feedRate,
            GetOptionalDouble(lineNumber, sourceText, values, 'A'),
            GetOptionalDouble(lineNumber, sourceText, values, 'B'),
            GetOptionalDouble(lineNumber, sourceText, values, 'C'));
    }

    private static double? GetOptionalDouble(
        int lineNumber,
        string sourceText,
        IReadOnlyDictionary<char, string> values,
        char letter) =>
        values.TryGetValue(letter, out var value)
            ? ParseDouble(lineNumber, sourceText, value, letter.ToString())
            : null;

    private static double GetRequiredDouble(
        int lineNumber,
        string sourceText,
        IReadOnlyDictionary<char, string> values,
        char letter,
        string missingMessage)
    {
        if (!values.TryGetValue(letter, out var value))
        {
            throw new ScriptParseException(lineNumber, sourceText, missingMessage);
        }

        return ParseDouble(lineNumber, sourceText, value, letter.ToString());
    }

    private static GCodeUnitInstruction ParseMillimeterUnits(
        int lineNumber,
        string sourceText,
        IReadOnlyCollection<GCodeWord> arguments)
    {
        ValidateNoArguments(lineNumber, sourceText, arguments, "G21 does not accept arguments.");

        return new GCodeUnitInstruction(
            CreateSource(lineNumber, sourceText),
            RobotScriptUnit.Millimeters);
    }

    private static GCodeDwellInstruction ParseDwell(
        int lineNumber,
        string sourceText,
        IReadOnlyCollection<GCodeWord> arguments)
    {
        var values = BuildArgumentMap(lineNumber, sourceText, arguments, ['P']);
        var durationMilliseconds = GetRequiredDouble(
            lineNumber,
            sourceText,
            values,
            'P',
            "G4 requires P followed by a dwell duration in milliseconds.");

        if (durationMilliseconds < 0)
        {
            throw new ScriptParseException(lineNumber, sourceText, "G4 P dwell duration cannot be negative.");
        }

        return new GCodeDwellInstruction(
            CreateSource(lineNumber, sourceText),
            TimeSpan.FromMilliseconds(durationMilliseconds));
    }

    private static void ValidateNoArguments(
        int lineNumber,
        string sourceText,
        IReadOnlyCollection<GCodeWord> arguments,
        string message)
    {
        if (arguments.Count > 0)
        {
            throw new ScriptParseException(lineNumber, sourceText, message);
        }
    }

    private static IReadOnlyDictionary<char, string> BuildArgumentMap(
        int lineNumber,
        string sourceText,
        IEnumerable<GCodeWord> arguments,
        IReadOnlyCollection<char> allowedLetters)
    {
        var values = new Dictionary<char, string>();

        foreach (var argument in arguments)
        {
            if (!allowedLetters.Contains(argument.Letter))
            {
                throw new ScriptParseException(lineNumber, sourceText, $"Unexpected G-code word '{argument.Letter}'.");
            }

            if (!values.TryAdd(argument.Letter, argument.Value))
            {
                throw new ScriptParseException(lineNumber, sourceText, $"Duplicate G-code word '{argument.Letter}'.");
            }
        }

        return values;
    }

    private static IReadOnlyList<GCodeWord> Tokenize(
        int lineNumber,
        string sourceText,
        string commandText)
    {
        var words = new List<GCodeWord>();
        var cursor = 0;

        foreach (Match match in GCodeWordPattern().Matches(commandText))
        {
            if (!string.IsNullOrWhiteSpace(commandText[cursor..match.Index]))
            {
                throw new ScriptParseException(lineNumber, sourceText, $"Invalid G-code syntax near '{commandText[cursor..]}'.");
            }

            words.Add(new GCodeWord(
                char.ToUpperInvariant(match.Groups["letter"].Value[0]),
                match.Groups["value"].Value));
            cursor = match.Index + match.Length;
        }

        if (words.Count == 0 || !string.IsNullOrWhiteSpace(commandText[cursor..]))
        {
            throw new ScriptParseException(lineNumber, sourceText, "Invalid G-code syntax. Expected words such as G1, X10, or P500.");
        }

        return words;
    }

    private static string RemoveComments(int lineNumber, string sourceText)
    {
        var result = new StringBuilder(sourceText.Length);
        var insideParentheses = false;

        foreach (var character in sourceText)
        {
            if (character == ';' && !insideParentheses)
            {
                break;
            }

            if (character == '(')
            {
                if (insideParentheses)
                {
                    throw new ScriptParseException(lineNumber, sourceText, "Nested G-code comments are not supported.");
                }

                insideParentheses = true;
                continue;
            }

            if (character == ')')
            {
                if (!insideParentheses)
                {
                    throw new ScriptParseException(lineNumber, sourceText, "G-code comment has a closing parenthesis without an opening parenthesis.");
                }

                insideParentheses = false;
                continue;
            }

            if (!insideParentheses)
            {
                result.Append(character);
            }
        }

        if (insideParentheses)
        {
            throw new ScriptParseException(lineNumber, sourceText, "G-code comment is missing a closing parenthesis.");
        }

        return result.ToString().Trim();
    }

    private static void ValidateLineNumber(int lineNumber, string sourceText, string value)
    {
        if (ParseInteger(lineNumber, sourceText, value, "N line number") < 0)
        {
            throw new ScriptParseException(lineNumber, sourceText, "N line number cannot be negative.");
        }
    }

    private static int ParseInteger(int lineNumber, string sourceText, string text, string name)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new ScriptParseException(lineNumber, sourceText, $"{name} must be a whole number.");
        }

        return value;
    }

    private static double ParseDouble(int lineNumber, string sourceText, string text, string name)
    {
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ||
            !double.IsFinite(value))
        {
            throw new ScriptParseException(lineNumber, sourceText, $"{name} must be a finite number using '.' as the decimal separator.");
        }

        return value;
    }

    private static RobotCommandSource CreateSource(int lineNumber, string sourceText) =>
        new(lineNumber, sourceText);

    [GeneratedRegex(@"(?<letter>[A-Za-z])(?<value>[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[Ee][+-]?\d+)?)", RegexOptions.CultureInvariant)]
    private static partial Regex GCodeWordPattern();

    private sealed record GCodeWord(char Letter, string Value);

}
