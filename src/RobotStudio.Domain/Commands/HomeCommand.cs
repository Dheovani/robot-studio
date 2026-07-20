namespace RobotStudio.Domain.Commands;

public sealed record HomeCommand : RobotCommand
{
    public HomeCommand(RobotCommandSource? source = null)
        : base(source)
    {
    }
}
