using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using RobotStudio.Simulation;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Rendering;

internal static class WpfRobotOverlayAdapter
{
    private const double AxisLabelWidth = 22;
    private const double AxisLabelHeight = 16;

    public static Model3DGroup CreateModelGroup(
        RobotOverlayScene scene,
        Color? workspaceBoundaryColor,
        params RobotOverlayKind[] visibleKinds)
    {
        ArgumentNullException.ThrowIfNull(scene);
        var visible = visibleKinds.ToHashSet();
        var group = new Model3DGroup();
        foreach (var primitive in scene.Primitives.Where(primitive => visible.Contains(primitive.Kind)))
        {
            switch (primitive)
            {
                case RobotOverlayLine line:
                    group.Children.Add(CreateLine(line));
                    break;
                case RobotOverlayPolyline polyline:
                    group.Children.Add(CreatePolyline(
                        polyline,
                        polyline.Kind == RobotOverlayKind.WorkspaceBoundary
                            ? workspaceBoundaryColor
                            : null));
                    break;
                case RobotOverlayPoint point:
                    group.Children.Add(CreatePoint(point));
                    break;
                case RobotOverlayBox box:
                    group.Children.Add(CreateBox(
                        box,
                        box.Kind == RobotOverlayKind.WorkspaceBoundary
                            ? workspaceBoundaryColor
                            : null));
                    break;
            }
        }

        return group;
    }

    public static Model3D CreateLine(RobotOverlayLine line)
        => MeshModelFactory.CreateOrientedBox(
            ToPoint(line.Start),
            ToPoint(line.End),
            line.Thickness,
            GetColor(line));

    public static Model3DGroup CreatePolyline(
        RobotOverlayPolyline polyline,
        Color? colorOverride = null)
    {
        var group = new Model3DGroup();
        var color = colorOverride ?? GetColor(polyline);
        for (var index = 1; index < polyline.Points.Count; index++)
        {
            var start = polyline.Points[index - 1];
            var end = polyline.Points[index];
            if (start != end)
            {
                group.Children.Add(MeshModelFactory.CreateOrientedBox(
                    ToPoint(start),
                    ToPoint(end),
                    polyline.Thickness,
                    color));
            }
        }

        return group;
    }

    public static Model3D CreatePoint(RobotOverlayPoint point) =>
        MeshModelFactory.CreateBox(
            ToVisual(point.Position),
            new VisualVector3(point.Size, point.Size, point.Size),
            GetColor(point));

    public static Model3DGroup CreateBox(
        RobotOverlayBox box,
        Color? colorOverride = null)
    {
        var group = new Model3DGroup();
        var half = box.Size / 2;
        var color = colorOverride ?? GetColor(box);

        foreach (var y in new[] { -half.Y, half.Y })
        {
            foreach (var z in new[] { -half.Z, half.Z })
            {
                group.Children.Add(CreateEdge(
                    box.Center + new Vector3(0, y, z),
                    new Vector3(box.Size.X, box.EdgeThickness, box.EdgeThickness),
                    color));
            }
        }

        foreach (var x in new[] { -half.X, half.X })
        {
            foreach (var z in new[] { -half.Z, half.Z })
            {
                group.Children.Add(CreateEdge(
                    box.Center + new Vector3(x, 0, z),
                    new Vector3(box.EdgeThickness, box.Size.Y, box.EdgeThickness),
                    color));
            }
        }

        foreach (var x in new[] { -half.X, half.X })
        {
            foreach (var y in new[] { -half.Y, half.Y })
            {
                group.Children.Add(CreateEdge(
                    box.Center + new Vector3(x, y, 0),
                    new Vector3(box.EdgeThickness, box.EdgeThickness, box.Size.Z),
                    color));
            }
        }

        return group;
    }

    public static Viewport2DVisual3D CreateLabel(RobotOverlayLabel label, PerspectiveCamera camera)
    {
        var accent = GetColor(label);
        var material = new DiffuseMaterial(Brushes.White);
        Viewport2DVisual3D.SetIsVisualHostMaterial(material, true);
        return new Viewport2DVisual3D
        {
            Geometry = CreateBillboardMesh(label.Position, camera),
            Material = material,
            Visual = new Border
            {
                Width = 32,
                Height = 24,
                Background = new SolidColorBrush(Color.FromArgb(210, 15, 23, 42)),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Child = new TextBlock
                {
                    Text = label.Text,
                    Foreground = new SolidColorBrush(accent),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                }
            }
        };
    }

    private static Model3D CreateEdge(Vector3 center, Vector3 size, Color color) =>
        MeshModelFactory.CreateBox(ToVisual(center), ToVisual(size), color);

    private static MeshGeometry3D CreateBillboardMesh(Vector3 center, PerspectiveCamera camera)
    {
        var look = camera.LookDirection;
        var up = camera.UpDirection;
        look.Normalize();
        up.Normalize();
        var right = Vector3D.CrossProduct(look, up);
        if (right.LengthSquared <= 0.000001)
        {
            right = new Vector3D(1, 0, 0);
        }

        right.Normalize();
        up = Vector3D.CrossProduct(right, look);
        up.Normalize();
        var centerPoint = new Point3D(center.X, center.Y, center.Z);
        var halfRight = right * (AxisLabelWidth / 2);
        var halfUp = up * (AxisLabelHeight / 2);

        return new MeshGeometry3D
        {
            Positions =
            [
                centerPoint - halfRight - halfUp,
                centerPoint + halfRight - halfUp,
                centerPoint + halfRight + halfUp,
                centerPoint - halfRight + halfUp
            ],
            TextureCoordinates = [new(0, 1), new(1, 1), new(1, 0), new(0, 0)],
            TriangleIndices = [0, 1, 2, 0, 2, 3]
        };
    }

    private static Color GetColor(RobotOverlayPrimitive primitive)
    {
        var axis = primitive switch
        {
            RobotOverlayLine line => line.Axis,
            RobotOverlayPoint point => point.Axis,
            RobotOverlayLabel label => label.Axis,
            _ => null
        };
        if (axis is not null)
        {
            return axis switch
            {
                RobotOverlayAxis.X => Color.FromRgb(248, 113, 113),
                RobotOverlayAxis.Y => Color.FromRgb(34, 197, 94),
                RobotOverlayAxis.Z => Color.FromRgb(96, 165, 250),
                _ => Colors.White
            };
        }

        return primitive.Kind switch
        {
            RobotOverlayKind.CoordinateGrid => Color.FromArgb(90, 71, 85, 105),
            RobotOverlayKind.CoordinateOrigin => Color.FromRgb(148, 163, 184),
            RobotOverlayKind.WorkspaceBoundary => Color.FromArgb(120, 148, 163, 184),
            RobotOverlayKind.Trajectory => Color.FromArgb(190, 250, 204, 21),
            RobotOverlayKind.StartPosition => Color.FromRgb(74, 222, 128),
            RobotOverlayKind.EndPosition => Color.FromRgb(251, 146, 60),
            RobotOverlayKind.CollisionBounds => Color.FromArgb(150, 239, 68, 68),
            _ => Colors.White
        };
    }

    private static VisualVector3 ToVisual(Vector3 value) =>
        new(value.X, value.Y, value.Z);

    private static Point3D ToPoint(Vector3 value) =>
        new(value.X, value.Y, value.Z);
}
