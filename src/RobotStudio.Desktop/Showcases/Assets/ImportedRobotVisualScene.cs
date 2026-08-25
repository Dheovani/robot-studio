using HelixToolkit.SharpDX.Model.Scene;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases.Assets;

public sealed class ImportedRobotVisualScene
{
    public ImportedRobotVisualScene(
        SceneNode root,
        IReadOnlyDictionary<RobotPartId, IReadOnlyList<SceneNode>> nodesByPart)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(nodesByPart);

        Root = root;
        NodesByPart = nodesByPart;
    }

    public SceneNode Root { get; }

    public IReadOnlyDictionary<RobotPartId, IReadOnlyList<SceneNode>> NodesByPart { get; }
}
