using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RobotStudio.Desktop.Rendering;

internal static class SceneLightingFactory
{
    public static Model3DGroup CreateDefault(
        Color? ambientColor = null,
        Vector3D? directionalLightDirection = null)
    {
        var group = new Model3DGroup();
        group.Children.Add(new AmbientLight(ambientColor ?? Color.FromRgb(82, 94, 116)));
        group.Children.Add(new DirectionalLight(
            Colors.White,
            directionalLightDirection ?? new Vector3D(-1, -1, -2)));

        return group;
    }
}
