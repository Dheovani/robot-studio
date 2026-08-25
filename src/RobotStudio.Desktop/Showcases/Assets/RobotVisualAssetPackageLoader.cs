using System.IO;
using RobotStudio.Visualization;
using RobotStudio.Visualization.Assets;

namespace RobotStudio.Desktop.Showcases.Assets;

public sealed class RobotVisualAssetPackageLoader
{
    private readonly Dictionary<string, RobotVisualAssetPackage> cache =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly object cacheLock = new();

    public RobotVisualAssetPackage Load(
        string manifestPath,
        RobotVisualModelDefinition model)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);
        ArgumentNullException.ThrowIfNull(model);

        var absoluteManifestPath = Path.GetFullPath(manifestPath);
        var cacheKey = $"{absoluteManifestPath}|{model.Id}";
        lock (cacheLock)
        {
            if (cache.TryGetValue(cacheKey, out var cachedPackage))
            {
                return cachedPackage;
            }
        }

        var package = LoadPackage(absoluteManifestPath, model);
        lock (cacheLock)
        {
            cache[cacheKey] = package;
        }

        return package;
    }

    public void ClearCache()
    {
        lock (cacheLock)
        {
            cache.Clear();
        }
    }

    private static RobotVisualAssetPackage LoadPackage(
        string absoluteManifestPath,
        RobotVisualModelDefinition model)
    {
        if (!File.Exists(absoluteManifestPath))
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.ManifestNotFound,
                $"Visual asset manifest was not found at '{absoluteManifestPath}'.");
        }

        RobotVisualAssetManifest manifest;
        try
        {
            using var stream = File.OpenRead(absoluteManifestPath);
            manifest = RobotVisualAssetManifestReader.Read(stream);
        }
        catch (RobotVisualAssetException)
        {
            throw;
        }
        catch (IOException exception)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.ManifestReadFailed,
                $"Visual asset manifest '{absoluteManifestPath}' could not be read.",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.ManifestReadFailed,
                $"Visual asset manifest '{absoluteManifestPath}' could not be read.",
                exception);
        }

        RobotVisualAssetManifestValidator.Validate(manifest, model);

        var packageDirectory = Path.GetDirectoryName(absoluteManifestPath)
            ?? throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.InvalidManifest,
                $"Visual asset manifest '{absoluteManifestPath}' has no package directory.");
        var relativeAssetPath = manifest.AssetFile.Replace('/', Path.DirectorySeparatorChar);
        var absoluteAssetPath = Path.GetFullPath(Path.Combine(packageDirectory, relativeAssetPath));
        EnsureContainedPath(packageDirectory, absoluteAssetPath);
        if (!File.Exists(absoluteAssetPath))
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.AssetNotFound,
                $"GLB asset was not found at '{absoluteAssetPath}'.");
        }

        return new RobotVisualAssetPackage(
            absoluteManifestPath,
            absoluteAssetPath,
            manifest);
    }

    private static void EnsureContainedPath(string packageDirectory, string assetPath)
    {
        var root = Path.GetFullPath(packageDirectory)
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!assetPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.UnsafeAssetPath,
                $"Visual asset path '{assetPath}' escapes its package directory.");
        }
    }
}
