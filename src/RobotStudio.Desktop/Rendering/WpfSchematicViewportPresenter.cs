using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace RobotStudio.Desktop.Rendering;

internal sealed class WpfSchematicViewportPresenter(Viewport3D viewport) : ISchematicViewportPresenter
{
    private readonly Viewport3D viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));

    public void Present(SchematicViewportScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var sceneRoot = SceneLightingFactory.CreateDefault(
            scene.AmbientColor,
            scene.DirectionalLightDirection);
        foreach (var model in scene.Models)
        {
            sceneRoot.Children.Add(model);
        }

        viewport.Children.Clear();
        viewport.Camera = scene.Camera;
        viewport.Children.Add(new ModelVisual3D { Content = sceneRoot });
        foreach (var overlay in scene.Overlays)
        {
            viewport.Children.Add(overlay);
        }
    }

    public void Clear()
    {
        viewport.Children.Clear();
        viewport.Camera = null;
    }
}
