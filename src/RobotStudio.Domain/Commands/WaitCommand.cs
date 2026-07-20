using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public sealed record WaitCommand : RobotCommand
{
    public WaitCommand(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new InvalidRobotCommandException(
                $"WAIT duration cannot be negative. Invalid value: {duration.TotalMilliseconds:0.###} ms. " +
                "Expected value: zero or greater.");
        }

        Duration = duration;
    }

    public TimeSpan Duration { get; }
}
