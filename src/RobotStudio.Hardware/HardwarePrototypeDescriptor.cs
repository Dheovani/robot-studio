namespace RobotStudio.Hardware;

public sealed record HardwarePrototypeDescriptor
{
    public HardwarePrototypeDescriptor(
        string name,
        RobotHardwareTarget target,
        HardwareActuatorKind actuatorKind,
        string description,
        bool isImplemented)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Name = name;
        Target = target;
        ActuatorKind = actuatorKind;
        Description = description;
        IsImplemented = isImplemented;
    }

    public string Name { get; }

    public RobotHardwareTarget Target { get; }

    public HardwareActuatorKind ActuatorKind { get; }

    public string Description { get; }

    public bool IsImplemented { get; }
}
