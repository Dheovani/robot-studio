namespace RobotStudio.Domain.Commands;

public sealed record ResetFaultCommand : RobotCommand
{
    public ResetFaultCommand(RobotCommandSource? source = null)
        : base(source)
    {
    }
}
