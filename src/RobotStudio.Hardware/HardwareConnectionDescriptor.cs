namespace RobotStudio.Hardware;

public sealed record HardwareConnectionDescriptor
{
    public HardwareConnectionDescriptor(RobotHardwareTarget target, string displayName, string transportName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(transportName);

        Target = target;
        DisplayName = displayName;
        TransportName = transportName;
    }

    public RobotHardwareTarget Target { get; }

    public string DisplayName { get; }

    public string TransportName { get; }
}
