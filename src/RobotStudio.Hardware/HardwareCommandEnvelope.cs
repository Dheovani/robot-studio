using RobotStudio.Domain.Commands;

namespace RobotStudio.Hardware;

public sealed record HardwareCommandEnvelope
{
    public HardwareCommandEnvelope(
        Guid commandId,
        RobotCommand command,
        TimeSpan timeout)
    {
        if (commandId == Guid.Empty)
        {
            throw new ArgumentException("Hardware command id cannot be empty.", nameof(commandId));
        }

        ArgumentNullException.ThrowIfNull(command);

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                timeout,
                "Hardware command timeout must be greater than zero.");
        }

        CommandId = commandId;
        Command = command;
        Timeout = timeout;
    }

    public Guid CommandId { get; }

    public RobotCommand Command { get; }

    public TimeSpan Timeout { get; }

    public static HardwareCommandEnvelope Create(
        RobotCommand command,
        TimeSpan timeout) =>
        new(Guid.NewGuid(), command, timeout);
}
