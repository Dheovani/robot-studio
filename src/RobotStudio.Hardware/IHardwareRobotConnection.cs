namespace RobotStudio.Hardware;

public interface IHardwareRobotConnection
{
    HardwareConnectionDescriptor Descriptor { get; }

    HardwareConnectionStatus Status { get; }

    Task<HardwareCommandResult> SendAsync(
        HardwareCommandEnvelope command,
        CancellationToken cancellationToken = default);
}
