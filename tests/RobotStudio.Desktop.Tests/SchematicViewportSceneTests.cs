using System.Windows.Media;
using System.Windows.Media.Media3D;
using RobotStudio.Desktop.Rendering;

namespace RobotStudio.Desktop.Tests;

public sealed class SchematicViewportSceneTests
{
    [Fact]
    public void Constructor_ShouldSnapshotModelsOverlaysAndLightingSettings()
    {
        var camera = new PerspectiveCamera();
        var model = new Model3DGroup();
        var overlay = new ModelVisual3D();
        var models = new List<Model3D> { model };
        var overlays = new List<Visual3D> { overlay };
        var ambient = Color.FromRgb(80, 90, 110);
        var lightDirection = new Vector3D(-1, -2, -3);

        var scene = new SchematicViewportScene(
            camera,
            models,
            overlays,
            ambient,
            lightDirection);
        models.Clear();
        overlays.Clear();

        Assert.Same(camera, scene.Camera);
        Assert.Equal([model], scene.Models);
        Assert.Equal([overlay], scene.Overlays);
        Assert.Equal(ambient, scene.AmbientColor);
        Assert.Equal(lightDirection, scene.DirectionalLightDirection);
    }

    [Fact]
    public void Constructor_WhenVisualCollectionContainsNull_ShouldRejectScene()
    {
        var camera = new PerspectiveCamera();
        Model3D[] models = [new Model3DGroup(), null!];

        var exception = Assert.Throws<ArgumentException>(() =>
            new SchematicViewportScene(camera, models));

        Assert.Contains("null visual elements", exception.Message, StringComparison.Ordinal);
    }
}
