using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public interface IRobotScriptDialect
{
    RobotScriptDialectDescriptor Descriptor { get; }

    RobotScriptCompilation Compile(
        string script,
        RobotScriptParseContext? context = null);

    RobotCommandSequence Parse(
        string script,
        RobotScriptParseContext? context = null);
}
