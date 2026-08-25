using HelixToolkit.SharpDX.Model.Scene;
using RobotStudio.Visualization;
using RobotStudio.Visualization.Assets;

namespace RobotStudio.Desktop.Showcases.Assets;

public static class RobotVisualSceneBinder
{
    public static ImportedRobotVisualScene Bind(
        SceneNode root,
        RobotVisualAssetManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(manifest);

        var sceneNodes = Traverse(root).ToArray();
        var nodesByName = sceneNodes
            .Where(node => !string.IsNullOrWhiteSpace(node.Name))
            .GroupBy(node => node.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var explicitBindings = ResolveExplicitBindings(manifest, nodesByName);
        var nodesByPart = new Dictionary<RobotPartId, HashSet<SceneNode>>();

        foreach (var (node, partId) in explicitBindings)
        {
            BindSubtree(node, partId, explicitBindings, nodesByPart);
        }

        return new ImportedRobotVisualScene(
            root,
            nodesByPart.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<SceneNode>)pair.Value.ToArray()),
            explicitBindings
                .GroupBy(pair => pair.Value)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<SceneNode>)group.Select(pair => pair.Key).ToArray()));
    }

    private static IReadOnlyDictionary<SceneNode, RobotPartId> ResolveExplicitBindings(
        RobotVisualAssetManifest manifest,
        IReadOnlyDictionary<string, SceneNode[]> nodesByName)
    {
        var bindings = new Dictionary<SceneNode, RobotPartId>();
        foreach (var binding in manifest.NodeBindings)
        {
            if (!nodesByName.TryGetValue(binding.NodeName, out var matches))
            {
                throw new RobotVisualAssetException(
                    RobotVisualAssetErrorCode.AssetNodeMissing,
                    $"GLB node '{binding.NodeName}' mapped to part '{binding.PartId}' was not found.");
            }

            if (matches.Length > 1)
            {
                throw new RobotVisualAssetException(
                    RobotVisualAssetErrorCode.AssetNodeAmbiguous,
                    $"GLB node name '{binding.NodeName}' is ambiguous because it occurs {matches.Length} times.");
            }

            bindings.Add(matches[0], binding.PartId);
        }

        return bindings;
    }

    private static void BindSubtree(
        SceneNode node,
        RobotPartId inheritedPartId,
        IReadOnlyDictionary<SceneNode, RobotPartId> explicitBindings,
        IDictionary<RobotPartId, HashSet<SceneNode>> nodesByPart)
    {
        var partId = explicitBindings.TryGetValue(node, out var explicitPartId)
            ? explicitPartId
            : inheritedPartId;
        node.Tag = partId;
        node.IsHitTestVisible = true;

        if (!nodesByPart.TryGetValue(partId, out var partNodes))
        {
            partNodes = [];
            nodesByPart.Add(partId, partNodes);
        }

        partNodes.Add(node);
        foreach (var child in node.Items)
        {
            BindSubtree(child, partId, explicitBindings, nodesByPart);
        }
    }

    private static IEnumerable<SceneNode> Traverse(SceneNode node)
    {
        yield return node;
        foreach (var child in node.Items)
        {
            foreach (var descendant in Traverse(child))
            {
                yield return descendant;
            }
        }
    }
}
