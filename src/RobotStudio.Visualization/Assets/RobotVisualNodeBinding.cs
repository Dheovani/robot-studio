namespace RobotStudio.Visualization.Assets;

public sealed record RobotVisualNodeBinding
{
    public RobotVisualNodeBinding(string nodeName, RobotPartId partId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(partId.Value);

        NodeName = nodeName.Trim();
        PartId = partId;
    }

    public string NodeName { get; }

    public RobotPartId PartId { get; }
}
