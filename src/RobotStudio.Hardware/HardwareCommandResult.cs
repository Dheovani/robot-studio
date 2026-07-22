namespace RobotStudio.Hardware;

public sealed record HardwareCommandResult
{
    public HardwareCommandResult(Guid commandId, HardwareCommandResultStatus status, string message)
    {
        if (commandId == Guid.Empty)
        {
            throw new ArgumentException("Hardware command result id cannot be empty.", nameof(commandId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        CommandId = commandId;
        Status = status;
        Message = message;
    }

    public Guid CommandId { get; }

    public HardwareCommandResultStatus Status { get; }

    public string Message { get; }
}
