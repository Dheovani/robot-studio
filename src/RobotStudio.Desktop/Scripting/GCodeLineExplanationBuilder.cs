using System.Text.RegularExpressions;
using RobotStudio.Scripting;

namespace RobotStudio.Desktop.Scripting;

public static partial class GCodeLineExplanationBuilder
{
    public static IReadOnlyList<GCodeLineExplanation> Build(
        string script,
        GCodeRobotMappingDescriptor mapping)
    {
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(mapping);

        var explanations = new List<GCodeLineExplanation>();
        var positioningMode = "absolute";
        var lines = script.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');

        for (var index = 0; index < lines.Length; index++)
        {
            var command = GetCommand(lines[index]);
            var explanation = command switch
            {
                "G21" => "Select millimeters as the program unit. RobotStudio rejects inch mode to keep lessons consistent.",
                "G90" => SetModeAndExplain(ref positioningMode, "absolute", "Use absolute positioning. Subsequent G1 coordinates identify destinations in the robot coordinate system."),
                "G91" => SetModeAndExplain(ref positioningMode, "relative", "Use relative positioning. Subsequent G1 coordinates describe displacement from the current TCP pose."),
                "G28" => "Return the robot to its family-specific HOME state before continuing the program.",
                "G1" => ExplainLinearMove(mapping, positioningMode),
                "G4" => "Pause simulated execution. P defines the dwell duration in milliseconds while the robot holds its pose.",
                _ => null
            };

            if (explanation is not null)
            {
                explanations.Add(new GCodeLineExplanation(index + 1, command, explanation));
            }
        }

        return explanations.AsReadOnly();
    }

    private static string ExplainLinearMove(
        GCodeRobotMappingDescriptor mapping,
        string positioningMode)
    {
        var coordinates = mapping.Target switch
        {
            GCodeRobotTarget.ScaraRobot => "X/Y define the planar TCP position",
            GCodeRobotTarget.SimpleArticulatedArm => "X/Y define TCP position and A defines tool orientation",
            GCodeRobotTarget.IndustrialArm6Dof => "X/Y/Z define TCP position and A/B/C define roll, pitch, and yaw",
            _ => $"{string.Join('/', mapping.ToolSpaceWords)} define the TCP position"
        };
        return $"Move the TCP along a linear {positioningMode} tool-space path. " +
               $"{coordinates}; F requests speed in millimeters per minute.";
    }

    private static string SetModeAndExplain(
        ref string positioningMode,
        string nextMode,
        string explanation)
    {
        positioningMode = nextMode;
        return explanation;
    }

    private static string GetCommand(string line)
    {
        var withoutComments = ParenthesizedCommentRegex().Replace(
            line.Split(';', 2)[0],
            string.Empty);
        var tokens = TokenRegex().Matches(withoutComments.ToUpperInvariant());
        foreach (Match token in tokens)
        {
            if (token.Value.StartsWith('N'))
            {
                continue;
            }

            if (token.Value is "G1" or "G01" or "G4" or "G04" or "G21" or "G28" or "G90" or "G91")
            {
                return token.Value switch
                {
                    "G01" => "G1",
                    "G04" => "G4",
                    _ => token.Value
                };
            }
        }

        return string.Empty;
    }

    [GeneratedRegex(@"\([^)]*\)")]
    private static partial Regex ParenthesizedCommentRegex();

    [GeneratedRegex(@"[A-Z][-+]?(?:\d+(?:\.\d*)?|\.\d+)")]
    private static partial Regex TokenRegex();
}
