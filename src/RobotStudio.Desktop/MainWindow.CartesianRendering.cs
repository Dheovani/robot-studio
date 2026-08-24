using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Microsoft.Win32;
using RobotStudio.Desktop.Examples;
using RobotStudio.Desktop.Profiles;
using RobotStudio.Desktop.Rendering;
using RobotStudio.Desktop.Robots;
using RobotStudio.Desktop.Scripting;
using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Domain.Parallel;
using RobotStudio.Scripting;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop;

public partial class MainWindow
{
    private static PerspectiveCamera CreateCamera(
        CartesianViewportSnapshot viewport,
        double azimuthDegrees,
        double elevationDegrees,
        double distanceMillimeters)
    {
        var azimuthRadians = DegreesToRadians(azimuthDegrees);
        var elevationRadians = DegreesToRadians(elevationDegrees);
        var horizontalDistance = distanceMillimeters * Math.Cos(elevationRadians);
        var cameraPosition = new VisualVector3(
            viewport.Target.XMillimeters + (horizontalDistance * Math.Cos(azimuthRadians)),
            viewport.Target.YMillimeters + (horizontalDistance * Math.Sin(azimuthRadians)),
            viewport.Target.ZMillimeters + (distanceMillimeters * Math.Sin(elevationRadians)));

        return new PerspectiveCamera
        {
            Position = ToPoint3D(cameraPosition),
            LookDirection = ToVector3D(Subtract(viewport.Target, cameraPosition)),
            UpDirection = ToVector3D(viewport.Up),
            NearPlaneDistance = viewport.NearClipMillimeters,
            FarPlaneDistance = viewport.FarClipMillimeters,
            FieldOfView = 45
        };
    }

    private static Model3D CreateModel(CartesianScenePrimitive primitive) =>
        primitive.Kind == CartesianScenePrimitiveKind.Workspace
            ? CreateWorkspaceBoundsModel(primitive)
            : CreateBoxModel(primitive);

    private bool IsPrimitiveVisible(CartesianScenePrimitive primitive) =>
        primitive.Kind switch
        {
            CartesianScenePrimitiveKind.Workspace => ShowWorkspaceCheckBox.IsChecked == true,
            CartesianScenePrimitiveKind.Rail => ShowRailsCheckBox.IsChecked == true,
            CartesianScenePrimitiveKind.Carriage => ShowCarriagesCheckBox.IsChecked == true,
            CartesianScenePrimitiveKind.Tool => ShowToolCheckBox.IsChecked == true,
            _ => true
        };

    private static Model3DGroup CreateGridModel(CartesianWorkspaceBounds bounds)
    {
        var group = new Model3DGroup();
        var size = bounds.Size;
        var gridZ = bounds.Minimum.ZMillimeters - GridLineThicknessMillimeters;
        var gridColor = Color.FromArgb(90, 71, 85, 105);

        for (var x = bounds.Minimum.XMillimeters; x <= bounds.Maximum.XMillimeters; x += GridSpacingMillimeters)
        {
            group.Children.Add(CreateColoredBoxModel(
                new VisualVector3(x, bounds.Center.YMillimeters, gridZ),
                new VisualVector3(GridLineThicknessMillimeters, size.YMillimeters, GridLineThicknessMillimeters),
                gridColor));
        }

        for (var y = bounds.Minimum.YMillimeters; y <= bounds.Maximum.YMillimeters; y += GridSpacingMillimeters)
        {
            group.Children.Add(CreateColoredBoxModel(
                new VisualVector3(bounds.Center.XMillimeters, y, gridZ),
                new VisualVector3(size.XMillimeters, GridLineThicknessMillimeters, GridLineThicknessMillimeters),
                gridColor));
        }

        return group;
    }

    private static Model3DGroup CreateGlobalAxesModel(CartesianWorkspaceBounds bounds)
    {
        var origin = new VisualVector3(
            Math.Clamp(0, bounds.Minimum.XMillimeters, bounds.Maximum.XMillimeters),
            Math.Clamp(0, bounds.Minimum.YMillimeters, bounds.Maximum.YMillimeters),
            Math.Clamp(0, bounds.Minimum.ZMillimeters, bounds.Maximum.ZMillimeters));
        var group = new Model3DGroup();

        group.Children.Add(CreateColoredBoxModel(
            new VisualVector3(bounds.Center.XMillimeters, origin.YMillimeters, origin.ZMillimeters),
            new VisualVector3(bounds.Size.XMillimeters, AxisLineThicknessMillimeters, AxisLineThicknessMillimeters),
            Color.FromRgb(248, 113, 113)));

        group.Children.Add(CreateColoredBoxModel(
            new VisualVector3(origin.XMillimeters, bounds.Center.YMillimeters, origin.ZMillimeters),
            new VisualVector3(AxisLineThicknessMillimeters, bounds.Size.YMillimeters, AxisLineThicknessMillimeters),
            Color.FromRgb(34, 197, 94)));

        group.Children.Add(CreateColoredBoxModel(
            new VisualVector3(origin.XMillimeters, origin.YMillimeters, bounds.Center.ZMillimeters),
            new VisualVector3(AxisLineThicknessMillimeters, AxisLineThicknessMillimeters, bounds.Size.ZMillimeters),
            Color.FromRgb(96, 165, 250)));

        return group;
    }

    private static Model3DGroup CreatePlannedPathModel(CartesianPlaybackSnapshot snapshot)
    {
        var group = new Model3DGroup();
        var pathPointSize = new VisualVector3(
            PathPointSizeMillimeters,
            PathPointSizeMillimeters,
            PathPointSizeMillimeters);
        var pathPointColor = Color.FromArgb(190, 250, 204, 21);
        var step = Math.Max(1, snapshot.Poses.Count / MaximumPathPointCount);
        VisualVector3? previousPoint = null;

        for (var index = 0; index < snapshot.Poses.Count; index += step)
        {
            var point = snapshot.Poses[index].ToolCenterPoint;
            if (previousPoint is not null && AreNear(previousPoint.Value, point))
            {
                continue;
            }

            group.Children.Add(CreateColoredBoxModel(point, pathPointSize, pathPointColor));
            previousPoint = point;
        }

        var finalPoint = snapshot.Poses[^1].ToolCenterPoint;
        if (previousPoint is null || !AreNear(previousPoint.Value, finalPoint))
        {
            group.Children.Add(CreateColoredBoxModel(finalPoint, pathPointSize, pathPointColor));
        }

        return group;
    }

    private static Model3DGroup CreateStartEndMarkersModel(CartesianPlaybackSnapshot snapshot)
    {
        var markerSize = new VisualVector3(
            StartEndMarkerSizeMillimeters,
            StartEndMarkerSizeMillimeters,
            StartEndMarkerSizeMillimeters);
        var group = new Model3DGroup();

        group.Children.Add(CreateColoredBoxModel(
            snapshot.Poses[0].ToolCenterPoint,
            markerSize,
            Color.FromRgb(74, 222, 128)));
        group.Children.Add(CreateColoredBoxModel(
            snapshot.Poses[^1].ToolCenterPoint,
            markerSize,
            Color.FromRgb(251, 146, 60)));

        return group;
    }

    private static IReadOnlyList<Viewport2DVisual3D> CreateAxisLabelVisuals(
        CartesianWorkspaceBounds bounds,
        PerspectiveCamera camera)
    {
        var origin = new VisualVector3(
            Math.Clamp(0, bounds.Minimum.XMillimeters, bounds.Maximum.XMillimeters),
            Math.Clamp(0, bounds.Minimum.YMillimeters, bounds.Maximum.YMillimeters),
            Math.Clamp(0, bounds.Minimum.ZMillimeters, bounds.Maximum.ZMillimeters));

        return
        [
            CreateAxisLabelVisual(
                "X",
                new VisualVector3(
                    bounds.Maximum.XMillimeters + AxisLabelOffsetMillimeters,
                    origin.YMillimeters,
                    origin.ZMillimeters),
                Color.FromRgb(248, 113, 113),
                camera),
            CreateAxisLabelVisual(
                "Y",
                new VisualVector3(
                    origin.XMillimeters,
                    bounds.Maximum.YMillimeters + AxisLabelOffsetMillimeters,
                    origin.ZMillimeters),
                Color.FromRgb(34, 197, 94),
                camera),
            CreateAxisLabelVisual(
                "Z",
                new VisualVector3(
                    origin.XMillimeters,
                    origin.YMillimeters,
                    bounds.Maximum.ZMillimeters + AxisLabelOffsetMillimeters),
                Color.FromRgb(96, 165, 250),
                camera)
        ];
    }

    private static Viewport2DVisual3D CreateAxisLabelVisual(
        string text,
        VisualVector3 center,
        Color accent,
        PerspectiveCamera camera)
    {
        var material = new DiffuseMaterial(Brushes.White);
        Viewport2DVisual3D.SetIsVisualHostMaterial(material, true);

        return new Viewport2DVisual3D
        {
            Geometry = CreateBillboardMesh(
                center,
                AxisLabelWidthMillimeters,
                AxisLabelHeightMillimeters,
                camera),
            Material = material,
            Visual = CreateAxisLabelElement(text, accent)
        };
    }

    private static Border CreateAxisLabelElement(
        string text,
        Color accent) =>
        new()
        {
            Width = 32,
            Height = 24,
            Background = new SolidColorBrush(Color.FromArgb(210, 15, 23, 42)),
            BorderBrush = new SolidColorBrush(accent),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Child = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(accent),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            }
        };

    private static MeshGeometry3D CreateBillboardMesh(
        VisualVector3 center,
        double widthMillimeters,
        double heightMillimeters,
        PerspectiveCamera camera)
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

        var centerPoint = ToPoint3D(center);
        var halfRight = right * (widthMillimeters / 2);
        var halfUp = up * (heightMillimeters / 2);

        return new MeshGeometry3D
        {
            Positions = new Point3DCollection
            {
                centerPoint - halfRight - halfUp,
                centerPoint + halfRight - halfUp,
                centerPoint + halfRight + halfUp,
                centerPoint - halfRight + halfUp
            },
            TextureCoordinates = new PointCollection
            {
                new(0, 1),
                new(1, 1),
                new(1, 0),
                new(0, 0)
            },
            TriangleIndices = new Int32Collection
            {
                0, 1, 2,
                0, 2, 3
            }
        };
    }

    private static GeometryModel3D CreateBoxModel(CartesianScenePrimitive primitive) =>
        new(
            CreateBoxMesh(primitive.Center, primitive.Size),
            new DiffuseMaterial(new SolidColorBrush(GetColor(primitive.Kind))))
        {
            BackMaterial = new DiffuseMaterial(new SolidColorBrush(GetColor(primitive.Kind)))
        };

    private static GeometryModel3D CreateColoredBoxModel(
        VisualVector3 center,
        VisualVector3 size,
        Color color) =>
        new(
            CreateBoxMesh(center, size),
            new DiffuseMaterial(new SolidColorBrush(color)))
        {
            BackMaterial = new DiffuseMaterial(new SolidColorBrush(color))
        };

    private static Model3DGroup CreateWorkspaceBoundsModel(CartesianScenePrimitive primitive)
    {
        var lineThickness = Math.Max(
            1,
            Math.Min(
                Math.Min(primitive.Size.XMillimeters, primitive.Size.YMillimeters),
                primitive.Size.ZMillimeters) * 0.008);
        var halfX = Math.Max(primitive.Size.XMillimeters, 1) / 2;
        var halfY = Math.Max(primitive.Size.YMillimeters, 1) / 2;
        var halfZ = Math.Max(primitive.Size.ZMillimeters, 1) / 2;
        var center = primitive.Center;
        var group = new Model3DGroup();

        foreach (var yOffset in new[] { -halfY, halfY })
        {
            foreach (var zOffset in new[] { -halfZ, halfZ })
            {
                group.Children.Add(CreateWorkspaceEdge(
                    new VisualVector3(center.XMillimeters, center.YMillimeters + yOffset, center.ZMillimeters + zOffset),
                    new VisualVector3(primitive.Size.XMillimeters, lineThickness, lineThickness)));
            }
        }

        foreach (var xOffset in new[] { -halfX, halfX })
        {
            foreach (var zOffset in new[] { -halfZ, halfZ })
            {
                group.Children.Add(CreateWorkspaceEdge(
                    new VisualVector3(center.XMillimeters + xOffset, center.YMillimeters, center.ZMillimeters + zOffset),
                    new VisualVector3(lineThickness, primitive.Size.YMillimeters, lineThickness)));
            }
        }

        foreach (var xOffset in new[] { -halfX, halfX })
        {
            foreach (var yOffset in new[] { -halfY, halfY })
            {
                group.Children.Add(CreateWorkspaceEdge(
                    new VisualVector3(center.XMillimeters + xOffset, center.YMillimeters + yOffset, center.ZMillimeters),
                    new VisualVector3(lineThickness, lineThickness, primitive.Size.ZMillimeters)));
            }
        }

        return group;
    }

    private static GeometryModel3D CreateWorkspaceEdge(
        VisualVector3 center,
        VisualVector3 size) =>
        new(
            CreateBoxMesh(center, size),
            new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(120, 148, 163, 184))))
        {
            BackMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(120, 148, 163, 184)))
        };

    private static MeshGeometry3D CreateBoxMesh(
        VisualVector3 center,
        VisualVector3 size)
    {
        var halfX = Math.Max(size.XMillimeters, 1) / 2;
        var halfY = Math.Max(size.YMillimeters, 1) / 2;
        var halfZ = Math.Max(size.ZMillimeters, 1) / 2;
        var x = center.XMillimeters;
        var y = center.YMillimeters;
        var z = center.ZMillimeters;
        var mesh = new MeshGeometry3D
        {
            Positions = new Point3DCollection
            {
                new(x - halfX, y - halfY, z - halfZ),
                new(x + halfX, y - halfY, z - halfZ),
                new(x + halfX, y + halfY, z - halfZ),
                new(x - halfX, y + halfY, z - halfZ),
                new(x - halfX, y - halfY, z + halfZ),
                new(x + halfX, y - halfY, z + halfZ),
                new(x + halfX, y + halfY, z + halfZ),
                new(x - halfX, y + halfY, z + halfZ)
            },
            TriangleIndices = new Int32Collection
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                1, 2, 6, 1, 6, 5,
                2, 3, 7, 2, 7, 6,
                3, 0, 4, 3, 4, 7
            }
        };

        return mesh;
    }

    private static Color GetColor(CartesianScenePrimitiveKind kind) => kind switch
    {
        CartesianScenePrimitiveKind.Workspace => Color.FromArgb(28, 148, 163, 184),
        CartesianScenePrimitiveKind.Rail => Color.FromRgb(96, 165, 250),
        CartesianScenePrimitiveKind.Carriage => Color.FromRgb(34, 197, 94),
        CartesianScenePrimitiveKind.Tool => Color.FromRgb(248, 113, 113),
        _ => Colors.White
    };

    private static Point3D ToPoint3D(VisualVector3 vector) =>
        new(vector.XMillimeters, vector.YMillimeters, vector.ZMillimeters);

    private static Vector3D ToVector3D(VisualVector3 vector) =>
        new(vector.XMillimeters, vector.YMillimeters, vector.ZMillimeters);

    private static VisualVector3 Subtract(
        VisualVector3 left,
        VisualVector3 right) =>
        new(
            left.XMillimeters - right.XMillimeters,
            left.YMillimeters - right.YMillimeters,
            left.ZMillimeters - right.ZMillimeters);

    private static double CalculateDistance(
        VisualVector3 left,
        VisualVector3 right)
    {
        var deltaX = left.XMillimeters - right.XMillimeters;
        var deltaY = left.YMillimeters - right.YMillimeters;
        var deltaZ = left.ZMillimeters - right.ZMillimeters;

        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
    }

    private static bool AreNear(
        VisualVector3 left,
        VisualVector3 right)
    {
        const double toleranceMillimeters = 0.001;

        return Math.Abs(left.XMillimeters - right.XMillimeters) <= toleranceMillimeters &&
               Math.Abs(left.YMillimeters - right.YMillimeters) <= toleranceMillimeters &&
               Math.Abs(left.ZMillimeters - right.ZMillimeters) <= toleranceMillimeters;
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;

    private static string FormatNumber(double value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);
}
