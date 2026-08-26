using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RobotStudio.Desktop.Rendering;

internal sealed class SchematicViewportScene
{
    public SchematicViewportScene(
        Camera camera,
        IEnumerable<Model3D> models,
        IEnumerable<Visual3D>? overlays = null,
        Color? ambientColor = null,
        Vector3D? directionalLightDirection = null)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(models);

        Camera = camera;
        Models = models.ToArray();
        Overlays = overlays?.ToArray() ?? [];
        AmbientColor = ambientColor;
        DirectionalLightDirection = directionalLightDirection;

        if (Models.Any(model => model is null) || Overlays.Any(overlay => overlay is null))
        {
            throw new ArgumentException("A schematic viewport scene cannot contain null visual elements.");
        }
    }

    public Camera Camera { get; }

    public IReadOnlyList<Model3D> Models { get; }

    public IReadOnlyList<Visual3D> Overlays { get; }

    public Color? AmbientColor { get; }

    public Vector3D? DirectionalLightDirection { get; }
}
