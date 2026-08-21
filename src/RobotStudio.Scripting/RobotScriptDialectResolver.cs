namespace RobotStudio.Scripting;

public static class RobotScriptDialectResolver
{
    public static IRobotScriptDialect Resolve(
        string? requestedDialect = null,
        string? scriptPath = null)
    {
        if (!string.IsNullOrWhiteSpace(requestedDialect))
        {
            return CreateFromName(requestedDialect);
        }

        return string.Equals(
            Path.GetExtension(scriptPath),
            ".gcode",
            StringComparison.OrdinalIgnoreCase)
                ? new GCodeParser()
                : new RobotScriptParser();
    }

    private static IRobotScriptDialect CreateFromName(string requestedDialect) =>
        requestedDialect.Trim().ToLowerInvariant() switch
        {
            "dsl" or "simple-dsl" => new RobotScriptParser(),
            "gcode" or "g-code" => new GCodeParser(),
            _ => throw new ArgumentException(
                $"Unknown script dialect '{requestedDialect}'. Expected 'dsl' or 'gcode'.",
                nameof(requestedDialect))
        };
}
