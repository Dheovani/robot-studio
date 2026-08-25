using HelixToolkit.SharpDX.Model.Scene;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases.Assets;

public sealed class ImportedRobotVisualScene : IDisposable
{
    private bool isDisposed;

    public ImportedRobotVisualScene(
        SceneNode root,
        IReadOnlyDictionary<RobotPartId, IReadOnlyList<SceneNode>> nodesByPart,
        IReadOnlyDictionary<RobotPartId, IReadOnlyList<SceneNode>> rootNodesByPart)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(nodesByPart);
        ArgumentNullException.ThrowIfNull(rootNodesByPart);

        Root = root;
        NodesByPart = nodesByPart;
        RootNodesByPart = rootNodesByPart;
    }

    public SceneNode Root { get; }

    public IReadOnlyDictionary<RobotPartId, IReadOnlyList<SceneNode>> NodesByPart { get; }

    public IReadOnlyDictionary<RobotPartId, IReadOnlyList<SceneNode>> RootNodesByPart { get; }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        Root.Dispose();
        isDisposed = true;
    }
}
