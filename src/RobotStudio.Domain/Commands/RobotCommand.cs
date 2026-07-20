namespace RobotStudio.Domain.Commands;

public abstract record RobotCommand
{
    protected RobotCommand(RobotCommandSource? source = null)
    {
        Source = source;
    }

    public RobotCommandSource? Source { get; }
}
