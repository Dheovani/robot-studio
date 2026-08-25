using RobotStudio.Visualization.Assets;

namespace RobotStudio.Desktop.Showcases.Assets;

public sealed record RobotVisualAssetPackage(
    string ManifestPath,
    string AssetPath,
    RobotVisualAssetManifest Manifest);
