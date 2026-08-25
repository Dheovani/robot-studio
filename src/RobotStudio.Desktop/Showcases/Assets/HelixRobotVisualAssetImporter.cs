using HelixToolkit.SharpDX.Assimp;
using RobotStudio.Visualization.Assets;

namespace RobotStudio.Desktop.Showcases.Assets;

public sealed class HelixRobotVisualAssetImporter
{
    public ImportedRobotVisualScene Import(RobotVisualAssetPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);

        try
        {
            var importer = new Importer();
            var scene = importer.Load(package.AssetPath);
            if (scene?.Root is null)
            {
                throw new RobotVisualAssetException(
                    RobotVisualAssetErrorCode.AssetImportFailed,
                    $"GLB asset '{package.AssetPath}' could not be imported. Assimp error: {importer.ErrorCode}.");
            }

            return RobotVisualSceneBinder.Bind(scene.Root, package.Manifest);
        }
        catch (RobotVisualAssetException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new RobotVisualAssetException(
                RobotVisualAssetErrorCode.AssetImportFailed,
                $"GLB asset '{package.AssetPath}' could not be imported.",
                exception);
        }
    }
}
