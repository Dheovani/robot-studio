using RobotStudio.Domain;

namespace RobotStudio.Scripting;

public sealed record RobotScriptParseContext
{
    public RobotScriptParseContext(IRobotPosition initialPosition)
    {
        ArgumentNullException.ThrowIfNull(initialPosition);
        InitialPosition = initialPosition;
    }

    public IRobotPosition InitialPosition { get; }
}
