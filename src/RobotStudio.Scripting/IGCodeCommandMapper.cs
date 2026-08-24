namespace RobotStudio.Scripting;

public interface IGCodeCommandMapper
{
    RobotScriptCompilation Map(
        GCodeProgram program,
        RobotScriptParseContext? context = null);
}
