using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;
using RobotStudio.Visualization.Assets;

namespace RobotStudio.Desktop.Tests;

public sealed class RobotVisualAssetPackageLoaderTests
{
    [Fact]
    public void Load_WhenPackageIsValid_ShouldResolveAndCachePackage()
    {
        using var packageDirectory = VisualAssetTestDirectory.Create(includeAsset: true);
        var loader = new RobotVisualAssetPackageLoader();
        var model = CreateModel();

        var first = loader.Load(packageDirectory.ManifestPath, model);
        var second = loader.Load(packageDirectory.ManifestPath, model);

        Assert.Same(first, second);
        Assert.Equal(Path.GetFullPath(packageDirectory.ManifestPath), first.ManifestPath);
        Assert.Equal(Path.GetFullPath(packageDirectory.AssetPath), first.AssetPath);
    }

    [Fact]
    public void Load_AfterCacheIsCleared_ShouldReloadPackage()
    {
        using var packageDirectory = VisualAssetTestDirectory.Create(includeAsset: true);
        var loader = new RobotVisualAssetPackageLoader();

        var first = loader.Load(packageDirectory.ManifestPath, CreateModel());
        loader.ClearCache();
        var second = loader.Load(packageDirectory.ManifestPath, CreateModel());

        Assert.NotSame(first, second);
    }

    [Fact]
    public void Load_WhenManifestDoesNotExist_ShouldReportManifestNotFound()
    {
        var loader = new RobotVisualAssetPackageLoader();
        var manifestPath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        var exception = Assert.Throws<RobotVisualAssetException>(
            () => loader.Load(manifestPath, CreateModel()));

        Assert.Equal(RobotVisualAssetErrorCode.ManifestNotFound, exception.Code);
    }

    [Fact]
    public void Load_WhenGlbDoesNotExist_ShouldReportAssetNotFound()
    {
        using var packageDirectory = VisualAssetTestDirectory.Create(includeAsset: false);
        var loader = new RobotVisualAssetPackageLoader();

        var exception = Assert.Throws<RobotVisualAssetException>(
            () => loader.Load(packageDirectory.ManifestPath, CreateModel()));

        Assert.Equal(RobotVisualAssetErrorCode.AssetNotFound, exception.Code);
    }

    private static RobotVisualModelDefinition CreateModel()
    {
        var baseId = new RobotPartId("base");
        return new RobotVisualModelDefinition(
            "cartesian",
            "Cartesian Robot",
            baseId,
            [
                new RobotPartDefinition(
                    baseId,
                    "Machine base",
                    RobotPartKind.Base,
                    parentId: null,
                    "Supports the machine.",
                    "Remains fixed.")
            ]);
    }

    private sealed class VisualAssetTestDirectory : IDisposable
    {
        private VisualAssetTestDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public string ManifestPath => System.IO.Path.Combine(Path, "robot.json");

        public string AssetPath => System.IO.Path.Combine(Path, "robot.glb");

        public static VisualAssetTestDirectory Create(bool includeAsset)
        {
            var directory = new VisualAssetTestDirectory(System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "RobotStudio.Tests",
                Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(directory.Path);
            File.WriteAllText(directory.ManifestPath, """
                {
                  "schemaVersion": 1,
                  "modelId": "cartesian",
                  "assetFile": "robot.glb",
                  "nodes": [
                    { "nodeName": "Machine_Base", "partId": "base" }
                  ]
                }
                """);
            if (includeAsset)
            {
                File.WriteAllBytes(directory.AssetPath, []);
            }

            return directory;
        }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
