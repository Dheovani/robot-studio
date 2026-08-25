namespace RobotStudio.Visualization.Assets;

public sealed class RobotVisualAssetManifest
{
    public const int CurrentSchemaVersion = 1;

    public RobotVisualAssetManifest(
        int schemaVersion,
        string modelId,
        string assetFile,
        IEnumerable<RobotVisualNodeBinding> nodeBindings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        ArgumentException.ThrowIfNullOrWhiteSpace(assetFile);
        ArgumentNullException.ThrowIfNull(nodeBindings);

        var bindings = nodeBindings.ToArray();
        if (bindings.Length == 0)
        {
            throw new ArgumentException(
                "A visual asset manifest must define at least one node binding.",
                nameof(nodeBindings));
        }

        SchemaVersion = schemaVersion;
        ModelId = modelId.Trim();
        AssetFile = assetFile.Trim();
        NodeBindings = Array.AsReadOnly(bindings);
    }

    public int SchemaVersion { get; }

    public string ModelId { get; }

    public string AssetFile { get; }

    public IReadOnlyList<RobotVisualNodeBinding> NodeBindings { get; }
}
