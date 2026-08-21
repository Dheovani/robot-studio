namespace RobotStudio.Desktop.Scripting;

public static class ScriptEditorLineMetadataBuilder
{
    public static IReadOnlyList<ScriptEditorLineMetadata> Build(string script)
    {
        var normalizedScript = script.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalizedScript.Split('\n');
        var metadata = new List<ScriptEditorLineMetadata>(lines.Length);

        for (var index = 0; index < lines.Length; index++)
        {
            var commandText = GetCommandText(lines[index]);
            metadata.Add(new ScriptEditorLineMetadata(
                LineNumber: index + 1,
                CommandText: commandText,
                Kind: GetLineKind(commandText)));
        }

        return metadata;
    }

    private static string GetCommandText(string line)
    {
        var trimmedLine = line.TrimStart();
        if (trimmedLine.Length == 0)
        {
            return string.Empty;
        }

        var separatorIndex = trimmedLine.IndexOf(' ');
        return separatorIndex < 0
            ? trimmedLine.ToUpperInvariant()
            : trimmedLine[..separatorIndex].ToUpperInvariant();
    }

    private static ScriptEditorLineKind GetLineKind(string commandText) => commandText switch
    {
        "HOME" or "G28" => ScriptEditorLineKind.Home,
        "MOVE" or "G1" => ScriptEditorLineKind.Move,
        "WAIT" or "G4" => ScriptEditorLineKind.Wait,
        "" => ScriptEditorLineKind.Empty,
        _ => ScriptEditorLineKind.Other
    };
}
