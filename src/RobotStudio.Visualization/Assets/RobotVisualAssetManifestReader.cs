using System.Text.Json;
using System.Text.Json.Serialization;

namespace RobotStudio.Visualization.Assets;

public static class RobotVisualAssetManifestReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static RobotVisualAssetManifest Read(Stream jsonStream)
    {
        ArgumentNullException.ThrowIfNull(jsonStream);

        try
        {
            var document = JsonSerializer.Deserialize<ManifestDocument>(jsonStream, SerializerOptions)
                ?? throw InvalidManifest("The visual asset manifest is empty.");
            if (document.Nodes is null)
            {
                throw InvalidManifest("The visual asset manifest must define a 'nodes' array.");
            }

            var bindings = document.Nodes.Select(CreateBinding).ToArray();
            return new RobotVisualAssetManifest(
                document.SchemaVersion,
                document.ModelId ?? string.Empty,
                document.AssetFile ?? string.Empty,
                bindings);
        }
        catch (RobotVisualAssetException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.InvalidJson,
                "The visual asset manifest is not valid JSON.",
                exception);
        }
        catch (ArgumentException exception)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.InvalidManifest,
                exception.Message,
                exception);
        }
    }

    private static RobotVisualNodeBinding CreateBinding(NodeBindingDocument document)
    {
        try
        {
            return new RobotVisualNodeBinding(
                document.NodeName ?? string.Empty,
                new RobotPartId(document.PartId ?? string.Empty));
        }
        catch (ArgumentException exception)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.InvalidManifest,
                exception.Message,
                exception);
        }
    }

    private static RobotVisualAssetException InvalidManifest(string message) =>
        new(RobotVisualAssetErrorCode.InvalidManifest, message);

    private sealed record ManifestDocument(
        [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
        [property: JsonPropertyName("modelId")] string? ModelId,
        [property: JsonPropertyName("assetFile")] string? AssetFile,
        [property: JsonPropertyName("nodes")] NodeBindingDocument[]? Nodes);

    private sealed record NodeBindingDocument(
        [property: JsonPropertyName("nodeName")] string? NodeName,
        [property: JsonPropertyName("partId")] string? PartId);
}
