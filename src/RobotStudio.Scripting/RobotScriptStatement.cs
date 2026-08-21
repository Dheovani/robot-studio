using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public abstract record RobotScriptStatement
{
    protected RobotScriptStatement(RobotCommandSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Source = source;
    }

    public RobotCommandSource Source { get; }
}

public sealed record RobotScriptCommandStatement : RobotScriptStatement
{
    public RobotScriptCommandStatement(RobotCommand command)
        : base(command?.Source ?? throw new ArgumentException(
            "A compiled script command must retain its source line.",
            nameof(command)))
    {
        Command = command;
    }

    public RobotCommand Command { get; }
}

public sealed record RobotScriptPositioningModeStatement : RobotScriptStatement
{
    public RobotScriptPositioningModeStatement(
        RobotCommandSource source,
        RobotScriptPositioningMode mode)
        : base(source)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        Mode = mode;
    }

    public RobotScriptPositioningMode Mode { get; }
}
