using System.Windows.Media;
using System.Windows.Media.Media3D;
using RobotStudio.Simulation;
using RobotStudio.Visualization;

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

        var models = snapshot.SceneFrames[frameIndex].Primitives
            .Where(primitive => IsPrimitiveVisible(primitive, options))
            .Select(CreatePrimitiveModel)
            .ToList();
        var visuals = new List<Visual3D>();
        var overlayScene = CartesianOverlaySceneComposer.Compose(snapshot);
        foreach (var primitive in overlayScene.Primitives.Where(primitive => IsOverlayVisible(primitive, options)))
        {
            switch (primitive)
            {
                case RobotOverlayLine line:
                    models.Add(WpfRobotOverlayAdapter.CreateLine(line));
                    break;
                case RobotOverlayPolyline polyline:
                    models.Add(WpfRobotOverlayAdapter.CreatePolyline(polyline));
                    break;
                case RobotOverlayPoint point:
                    models.Add(WpfRobotOverlayAdapter.CreatePoint(point));
                    break;
                case RobotOverlayBox box:
                    models.Add(WpfRobotOverlayAdapter.CreateBox(box));
                    break;
                case RobotOverlayLabel label when camera is PerspectiveCamera perspectiveCamera:
                    visuals.Add(WpfRobotOverlayAdapter.CreateLabel(label, perspectiveCamera));
                    break;
            }
        }

        return new SchematicViewportScene(
            camera,
            models,
            visuals,
            ambientColor: Color.FromRgb(92, 105, 130));
    }

    private static bool IsPrimitiveVisible(
        CartesianScenePrimitive primitive,
        CartesianSchematicSceneOptions options) =>
        primitive.Kind switch
        {
            CartesianScenePrimitiveKind.Workspace => false,
            CartesianScenePrimitiveKind.Rail => options.ShowRails,
            CartesianScenePrimitiveKind.Carriage => options.ShowCarriages,
            CartesianScenePrimitiveKind.Tool => options.ShowTool,
            _ => true
        };

    private static bool IsOverlayVisible(
        RobotOverlayPrimitive primitive,
        CartesianSchematicSceneOptions options) =>
        primitive.Kind switch
        {
            RobotOverlayKind.CoordinateGrid => options.ShowGrid,
            RobotOverlayKind.CoordinateAxis => options.ShowGlobalAxes,
            RobotOverlayKind.AxisLabel => options.ShowAxisLabels,
            RobotOverlayKind.WorkspaceBoundary => options.ShowWorkspace,
            RobotOverlayKind.Trajectory => options.ShowPlannedPath,
            RobotOverlayKind.StartPosition or RobotOverlayKind.EndPosition => options.ShowStartEndMarkers,
            RobotOverlayKind.PhysicalLimit => false,
            RobotOverlayKind.CollisionBounds => false,
            _ => false
        };

    private static Model3D CreatePrimitiveModel(CartesianScenePrimitive primitive) =>
        MeshModelFactory.CreateBox(primitive.Center, primitive.Size, GetColor(primitive.Kind));

    private static Color GetColor(CartesianScenePrimitiveKind kind) => kind switch
    {
        CartesianScenePrimitiveKind.Rail => Color.FromRgb(96, 165, 250),
        CartesianScenePrimitiveKind.Carriage => Color.FromRgb(34, 197, 94),
        CartesianScenePrimitiveKind.Tool => Color.FromRgb(248, 113, 113),
        _ => Colors.White
    };
}
