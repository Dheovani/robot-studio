using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop.Rendering.SceneComposers;

internal sealed record CartesianSchematicSceneOptions(
    bool ShowGrid,
    bool ShowGlobalAxes,
    bool ShowAxisLabels,
    bool ShowWorkspace,
    bool ShowRails,
    bool ShowPlannedPath,
    bool ShowStartEndMarkers,
    bool ShowCarriages,
    bool ShowTool);

internal static class CartesianSchematicSceneComposer
{
    public static SchematicViewportScene Compose(
        CartesianPlaybackSnapshot snapshot,
        int frameIndex,
        Camera camera,
        CartesianSchematicSceneOptions options) =>
        CartesianSceneComposerCore.Compose(snapshot, frameIndex, camera, options);
}

internal static class XYPlotterSchematicSceneComposer
{
    public static SchematicViewportScene Compose(
        CartesianPlaybackSnapshot snapshot,
        int frameIndex,
        Camera camera,
        CartesianSchematicSceneOptions options) =>
        CartesianSceneComposerCore.Compose(snapshot, frameIndex, camera, options);
}

internal static class CartesianSceneComposerCore
{
    private const double GridSpacingMillimeters = 25;
    private const double GridLineThicknessMillimeters = 1.2;
    private const double AxisLineThicknessMillimeters = 4;
    private const double PathPointSizeMillimeters = 5;
    private const double StartEndMarkerSizeMillimeters = 14;
    private const double AxisLabelOffsetMillimeters = 24;
    private const double AxisLabelWidthMillimeters = 22;
    private const double AxisLabelHeightMillimeters = 16;
    private const int MaximumPathPointCount = 140;

    public static SchematicViewportScene Compose(
        CartesianPlaybackSnapshot snapshot,
        int frameIndex,
        Camera camera,
        CartesianSchematicSceneOptions options)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(options);
        if (frameIndex < 0 || frameIndex >= snapshot.SceneFrameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frameIndex));
        }

        var models = new List<Model3D>();
        if (options.ShowGrid)
        {
            models.Add(CreateGrid(snapshot.WorkspaceBounds));
        }

        if (options.ShowGlobalAxes)
        {
            models.Add(CreateGlobalAxes(snapshot.WorkspaceBounds));
        }

        if (options.ShowPlannedPath)
        {
            models.Add(CreatePlannedPath(snapshot));
        }

        if (options.ShowStartEndMarkers)
        {
            models.Add(CreateStartEndMarkers(snapshot));
        }

        foreach (var primitive in snapshot.SceneFrames[frameIndex].Primitives.Where(
                     primitive => IsPrimitiveVisible(primitive, options)))
        {
            models.Add(CreatePrimitiveModel(primitive));
        }

        var overlays = options.ShowAxisLabels && camera is PerspectiveCamera perspectiveCamera
            ? CreateAxisLabels(snapshot.WorkspaceBounds, perspectiveCamera).Cast<Visual3D>()
            : [];

        return new SchematicViewportScene(
            camera,
            models,
            overlays,
            ambientColor: Color.FromRgb(92, 105, 130));
    }

    private static bool IsPrimitiveVisible(
        CartesianScenePrimitive primitive,
        CartesianSchematicSceneOptions options) =>
        primitive.Kind switch
        {
            CartesianScenePrimitiveKind.Workspace => options.ShowWorkspace,
            CartesianScenePrimitiveKind.Rail => options.ShowRails,
            CartesianScenePrimitiveKind.Carriage => options.ShowCarriages,
            CartesianScenePrimitiveKind.Tool => options.ShowTool,
            _ => true
        };

    private static Model3D CreatePrimitiveModel(CartesianScenePrimitive primitive) =>
        primitive.Kind == CartesianScenePrimitiveKind.Workspace
            ? CreateWorkspaceBounds(primitive)
            : MeshModelFactory.CreateBox(primitive.Center, primitive.Size, GetColor(primitive.Kind));

    private static Model3DGroup CreateGrid(CartesianWorkspaceBounds bounds)
    {
        var group = new Model3DGroup();
        var size = bounds.Size;
        var gridZ = bounds.Minimum.ZMillimeters - GridLineThicknessMillimeters;
        var color = Color.FromArgb(90, 71, 85, 105);

        for (var x = bounds.Minimum.XMillimeters; x <= bounds.Maximum.XMillimeters; x += GridSpacingMillimeters)
        {
            group.Children.Add(MeshModelFactory.CreateBox(
                new VisualVector3(x, bounds.Center.YMillimeters, gridZ),
                new VisualVector3(GridLineThicknessMillimeters, size.YMillimeters, GridLineThicknessMillimeters),
                color));
        }

        for (var y = bounds.Minimum.YMillimeters; y <= bounds.Maximum.YMillimeters; y += GridSpacingMillimeters)
        {
            group.Children.Add(MeshModelFactory.CreateBox(
                new VisualVector3(bounds.Center.XMillimeters, y, gridZ),
                new VisualVector3(size.XMillimeters, GridLineThicknessMillimeters, GridLineThicknessMillimeters),
                color));
        }

        return group;
    }

    private static Model3DGroup CreateGlobalAxes(CartesianWorkspaceBounds bounds)
    {
        var origin = GetClampedOrigin(bounds);
        var group = new Model3DGroup();
        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(bounds.Center.XMillimeters, origin.YMillimeters, origin.ZMillimeters),
            new VisualVector3(bounds.Size.XMillimeters, AxisLineThicknessMillimeters, AxisLineThicknessMillimeters),
            Color.FromRgb(248, 113, 113)));
        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(origin.XMillimeters, bounds.Center.YMillimeters, origin.ZMillimeters),
            new VisualVector3(AxisLineThicknessMillimeters, bounds.Size.YMillimeters, AxisLineThicknessMillimeters),
            Color.FromRgb(34, 197, 94)));
        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(origin.XMillimeters, origin.YMillimeters, bounds.Center.ZMillimeters),
            new VisualVector3(AxisLineThicknessMillimeters, AxisLineThicknessMillimeters, bounds.Size.ZMillimeters),
            Color.FromRgb(96, 165, 250)));
        return group;
    }

    private static Model3DGroup CreatePlannedPath(CartesianPlaybackSnapshot snapshot)
    {
        var group = new Model3DGroup();
        var size = new VisualVector3(
            PathPointSizeMillimeters,
            PathPointSizeMillimeters,
            PathPointSizeMillimeters);
        var color = Color.FromArgb(190, 250, 204, 21);
        var step = Math.Max(1, snapshot.Poses.Count / MaximumPathPointCount);
        VisualVector3? previous = null;

        for (var index = 0; index < snapshot.Poses.Count; index += step)
        {
            var point = snapshot.Poses[index].ToolCenterPoint;
            if (previous is not null && AreNear(previous.Value, point))
            {
                continue;
            }

            group.Children.Add(MeshModelFactory.CreateBox(point, size, color));
            previous = point;
        }

        var finalPoint = snapshot.Poses[^1].ToolCenterPoint;
        if (previous is null || !AreNear(previous.Value, finalPoint))
        {
            group.Children.Add(MeshModelFactory.CreateBox(finalPoint, size, color));
        }

        return group;
    }

    private static Model3DGroup CreateStartEndMarkers(CartesianPlaybackSnapshot snapshot)
    {
        var size = new VisualVector3(
            StartEndMarkerSizeMillimeters,
            StartEndMarkerSizeMillimeters,
            StartEndMarkerSizeMillimeters);
        var group = new Model3DGroup();
        group.Children.Add(MeshModelFactory.CreateBox(
            snapshot.Poses[0].ToolCenterPoint,
            size,
            Color.FromRgb(74, 222, 128)));
        group.Children.Add(MeshModelFactory.CreateBox(
            snapshot.Poses[^1].ToolCenterPoint,
            size,
            Color.FromRgb(251, 146, 60)));
        return group;
    }

    private static IReadOnlyList<Viewport2DVisual3D> CreateAxisLabels(
        CartesianWorkspaceBounds bounds,
        PerspectiveCamera camera)
    {
        var origin = GetClampedOrigin(bounds);
        return
        [
            CreateAxisLabel(
                "X",
                new VisualVector3(bounds.Maximum.XMillimeters + AxisLabelOffsetMillimeters, origin.YMillimeters, origin.ZMillimeters),
                Color.FromRgb(248, 113, 113),
                camera),
            CreateAxisLabel(
                "Y",
                new VisualVector3(origin.XMillimeters, bounds.Maximum.YMillimeters + AxisLabelOffsetMillimeters, origin.ZMillimeters),
                Color.FromRgb(34, 197, 94),
                camera),
            CreateAxisLabel(
                "Z",
                new VisualVector3(origin.XMillimeters, origin.YMillimeters, bounds.Maximum.ZMillimeters + AxisLabelOffsetMillimeters),
                Color.FromRgb(96, 165, 250),
                camera)
        ];
    }

    private static Viewport2DVisual3D CreateAxisLabel(
        string text,
        VisualVector3 center,
        Color accent,
        PerspectiveCamera camera)
    {
        var material = new DiffuseMaterial(Brushes.White);
        Viewport2DVisual3D.SetIsVisualHostMaterial(material, true);
        return new Viewport2DVisual3D
        {
            Geometry = CreateBillboardMesh(center, camera),
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
                    Text = text,
                    Foreground = new SolidColorBrush(accent),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                }
            }
        };
    }

    private static MeshGeometry3D CreateBillboardMesh(
        VisualVector3 center,
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
        var centerPoint = new Point3D(center.XMillimeters, center.YMillimeters, center.ZMillimeters);
        var halfRight = right * (AxisLabelWidthMillimeters / 2);
        var halfUp = up * (AxisLabelHeightMillimeters / 2);

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

    private static Model3DGroup CreateWorkspaceBounds(CartesianScenePrimitive primitive)
    {
        var thickness = Math.Max(
            1,
            Math.Min(Math.Min(primitive.Size.XMillimeters, primitive.Size.YMillimeters), primitive.Size.ZMillimeters) * 0.008);
        var halfX = Math.Max(primitive.Size.XMillimeters, 1) / 2;
        var halfY = Math.Max(primitive.Size.YMillimeters, 1) / 2;
        var halfZ = Math.Max(primitive.Size.ZMillimeters, 1) / 2;
        var center = primitive.Center;
        var group = new Model3DGroup();
        var color = Color.FromArgb(120, 148, 163, 184);

        foreach (var y in new[] { -halfY, halfY })
        {
            foreach (var z in new[] { -halfZ, halfZ })
            {
                group.Children.Add(MeshModelFactory.CreateBox(
                    new VisualVector3(center.XMillimeters, center.YMillimeters + y, center.ZMillimeters + z),
                    new VisualVector3(primitive.Size.XMillimeters, thickness, thickness),
                    color));
            }
        }

        foreach (var x in new[] { -halfX, halfX })
        {
            foreach (var z in new[] { -halfZ, halfZ })
            {
                group.Children.Add(MeshModelFactory.CreateBox(
                    new VisualVector3(center.XMillimeters + x, center.YMillimeters, center.ZMillimeters + z),
                    new VisualVector3(thickness, primitive.Size.YMillimeters, thickness),
                    color));
            }
        }

        foreach (var x in new[] { -halfX, halfX })
        {
            foreach (var y in new[] { -halfY, halfY })
            {
                group.Children.Add(MeshModelFactory.CreateBox(
                    new VisualVector3(center.XMillimeters + x, center.YMillimeters + y, center.ZMillimeters),
                    new VisualVector3(thickness, thickness, primitive.Size.ZMillimeters),
                    color));
            }
        }

        return group;
    }

    private static VisualVector3 GetClampedOrigin(CartesianWorkspaceBounds bounds) =>
        new(
            Math.Clamp(0, bounds.Minimum.XMillimeters, bounds.Maximum.XMillimeters),
            Math.Clamp(0, bounds.Minimum.YMillimeters, bounds.Maximum.YMillimeters),
            Math.Clamp(0, bounds.Minimum.ZMillimeters, bounds.Maximum.ZMillimeters));

    private static Color GetColor(CartesianScenePrimitiveKind kind) => kind switch
    {
        CartesianScenePrimitiveKind.Workspace => Color.FromArgb(28, 148, 163, 184),
        CartesianScenePrimitiveKind.Rail => Color.FromRgb(96, 165, 250),
        CartesianScenePrimitiveKind.Carriage => Color.FromRgb(34, 197, 94),
        CartesianScenePrimitiveKind.Tool => Color.FromRgb(248, 113, 113),
        _ => Colors.White
    };

    private static bool AreNear(VisualVector3 left, VisualVector3 right)
    {
        const double toleranceMillimeters = 0.001;
        return Math.Abs(left.XMillimeters - right.XMillimeters) <= toleranceMillimeters &&
               Math.Abs(left.YMillimeters - right.YMillimeters) <= toleranceMillimeters &&
               Math.Abs(left.ZMillimeters - right.ZMillimeters) <= toleranceMillimeters;
    }
}
