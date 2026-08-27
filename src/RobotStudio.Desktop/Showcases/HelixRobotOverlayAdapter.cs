using HelixToolkit.Geometry;
using HelixToolkit.Maths;
using HelixToolkit.SharpDX;
using HelixToolkit.Wpf.SharpDX;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class HelixRobotOverlayAdapter
{
    public static MeshGeometryModel3D CreateAxisArrow(RobotOverlayLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (line.Kind != RobotOverlayKind.CoordinateAxis || line.Axis is null)
        {
            throw new ArgumentException("An axis arrow requires a coordinate-axis overlay.", nameof(line));
        }

        var builder = new MeshBuilder();
        builder.AddArrow(line.Start, line.End, line.Thickness, 3.2f, 24);
        return new MeshGeometryModel3D
        {
            Geometry = builder.ToMeshGeometry3D(),
            Material = AxisMaterial(line.Axis.Value),
            IsHitTestVisible = false,
            Visibility = System.Windows.Visibility.Collapsed
        };
    }

    private static PhongMaterial AxisMaterial(RobotOverlayAxis axis)
    {
        var color = axis switch
        {
            RobotOverlayAxis.X => new Color4(0.95f, 0.18f, 0.22f, 1f),
            RobotOverlayAxis.Y => new Color4(0.1f, 0.8f, 0.36f, 1f),
            RobotOverlayAxis.Z => new Color4(0.16f, 0.48f, 1f, 1f),
            _ => throw new ArgumentOutOfRangeException(nameof(axis))
        };
        return new PhongMaterial
        {
            DiffuseColor = color,
            AmbientColor = new Color4(color.Red * 0.2f, color.Green * 0.2f, color.Blue * 0.2f, 1f),
            SpecularColor = new Color4(0.85f, 0.9f, 1f, 1f),
            SpecularShininess = 80
        };
    }
}
