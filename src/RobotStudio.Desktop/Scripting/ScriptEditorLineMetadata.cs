namespace RobotStudio.Desktop.Scripting;

public sealed record ScriptEditorLineMetadata(
    int LineNumber,
    string CommandText,
    ScriptEditorLineKind Kind);
