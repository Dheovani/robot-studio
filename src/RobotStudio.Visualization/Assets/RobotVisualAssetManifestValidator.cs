namespace RobotStudio.Visualization.Assets;

public static class RobotVisualAssetManifestValidator
{
    public static void Validate(
        RobotVisualAssetManifest manifest,
        RobotVisualModelDefinition model)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(model);

        ValidateSchemaVersion(manifest.SchemaVersion);
        ValidateModelId(manifest.ModelId, model.Id);
        ValidateAssetPath(manifest.AssetFile);
        ValidateNodeBindings(manifest.NodeBindings, model);
    }

    private static void ValidateSchemaVersion(int schemaVersion)
    {
        if (schemaVersion != RobotVisualAssetManifest.CurrentSchemaVersion)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.UnsupportedSchemaVersion,
                $"Visual asset schema version {schemaVersion} is not supported. Expected version {RobotVisualAssetManifest.CurrentSchemaVersion}.");
        }
    }

    private static void ValidateModelId(string manifestModelId, string expectedModelId)
    {
        if (!string.Equals(manifestModelId, expectedModelId, StringComparison.Ordinal))
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.ModelMismatch,
                $"Visual asset model '{manifestModelId}' does not match expected model '{expectedModelId}'.");
        }
    }

    private static void ValidateAssetPath(string assetFile)
    {
        var segments = assetFile.Split('/');
        var isUnsafe = assetFile.Contains("\\", StringComparison.Ordinal) ||
                       assetFile.Contains(":", StringComparison.Ordinal) ||
                       assetFile.StartsWith("/", StringComparison.Ordinal) ||
                       Path.IsPathRooted(assetFile) ||
                       segments.Any(segment => segment is "" or "." or "..");
        if (isUnsafe)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.UnsafeAssetPath,
                $"Visual asset path '{assetFile}' must be a safe forward-slash relative path.");
        }

        if (!string.Equals(Path.GetExtension(assetFile), ".glb", StringComparison.OrdinalIgnoreCase))
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.InvalidManifest,
                $"Visual asset '{assetFile}' must use the .glb extension.");
        }
    }

    private static void ValidateNodeBindings(
        IReadOnlyList<RobotVisualNodeBinding> bindings,
        RobotVisualModelDefinition model)
    {
        var duplicateNode = bindings
            .GroupBy(binding => binding.NodeName, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateNode is not null)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.InvalidManifest,
                $"Visual asset node '{duplicateNode.Key}' is mapped more than once.");
        }

        var knownPartIds = model.Parts.Select(part => part.Id).ToHashSet();
        var unknownBinding = bindings.FirstOrDefault(binding => !knownPartIds.Contains(binding.PartId));
        if (unknownBinding is not null)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.UnknownPart,
                $"Visual asset node '{unknownBinding.NodeName}' maps to unknown part '{unknownBinding.PartId}'.");
        }

        var mappedPartIds = bindings.Select(binding => binding.PartId).ToHashSet();
        var missingPart = model.Parts.FirstOrDefault(
            part => part.IsSelectable && !mappedPartIds.Contains(part.Id));
        if (missingPart is not null)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.MissingSemanticBinding,
                $"Selectable visual part '{missingPart.Id}' has no asset node binding.");
        }
    }
}
