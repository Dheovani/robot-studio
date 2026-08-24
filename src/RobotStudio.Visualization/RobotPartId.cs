namespace RobotStudio.Visualization;

public readonly record struct RobotPartId
{
    public RobotPartId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; } = string.Empty;

    public override string ToString() => Value;
}
