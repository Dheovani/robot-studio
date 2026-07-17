namespace RobotStudio.Domain.Commands;

public sealed record WaitCommand : RobotCommand
{
    public WaitCommand(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Wait duration cannot be negative.");
        }

        Duration = duration;
    }

    public TimeSpan Duration { get; }
}
