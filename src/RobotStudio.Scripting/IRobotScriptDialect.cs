using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public interface IRobotScriptDialect
{
    RobotScriptDialectDescriptor Descriptor { get; }

    RobotCommandSequence Parse(
        string script,
        RobotScriptParseContext? context = null);
}
