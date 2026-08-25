using System.Text;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;
using RobotStudio.Visualization.Assets;

namespace RobotStudio.Desktop.Tests;

public sealed class HelixRobotVisualAssetImporterTests
{
    [Fact]
    public void Import_WhenGlbContainsManifestNodes_ShouldBindImportedHierarchy()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "RobotStudio.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            var assetPath = Path.Combine(directory, "robot.glb");
            WriteMinimalGlb(assetPath);
            var manifest = new RobotVisualAssetManifest(
                RobotVisualAssetManifest.CurrentSchemaVersion,
                "cartesian",
                "robot.glb",
                [
                    new RobotVisualNodeBinding("Machine_Base", new RobotPartId("base")),
                    new RobotVisualNodeBinding("Tool", new RobotPartId("tool"))
                ]);
            var package = new RobotVisualAssetPackage(
                Path.Combine(directory, "robot.json"),
                assetPath,
                manifest);

            using var scene = new HelixRobotVisualAssetImporter().Import(package);

            Assert.NotNull(scene.Root);
            Assert.NotEmpty(scene.NodesByPart[new RobotPartId("base")]);
            Assert.NotEmpty(scene.NodesByPart[new RobotPartId("tool")]);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_WhenFileIsNotGlb_ShouldReportImportFailure()
    {
        var assetPath = Path.Combine(
            Path.GetTempPath(),
            $"invalid-{Guid.NewGuid():N}.glb");
        File.WriteAllText(assetPath, "not a GLB file");

        try
        {
            var package = new RobotVisualAssetPackage(
                "robot.json",
                assetPath,
                new RobotVisualAssetManifest(
                    RobotVisualAssetManifest.CurrentSchemaVersion,
                    "cartesian",
                    "robot.glb",
                    [new RobotVisualNodeBinding("Machine_Base", new RobotPartId("base"))]));

            var exception = Assert.Throws<RobotVisualAssetException>(() =>
                new HelixRobotVisualAssetImporter().Import(package));

            Assert.Equal(RobotVisualAssetErrorCode.AssetImportFailed, exception.Code);
        }
        finally
        {
            File.Delete(assetPath);
        }
    }

    private static void WriteMinimalGlb(string path)
    {
        const string json = """
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [{ "nodes": [0, 1] }],
              "nodes": [
                { "name": "Machine_Base", "mesh": 0 },
                { "name": "Tool", "mesh": 0, "translation": [2, 0, 0] }
              ],
              "meshes": [{
                "primitives": [{ "attributes": { "POSITION": 0 }, "indices": 1 }]
              }],
              "buffers": [{ "byteLength": 44 }],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0, 0, 0],
                  "max": [1, 1, 0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """;
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        var paddedLength = (jsonBytes.Length + 3) & ~3;
        const int binaryLength = 44;
        var totalLength = 12 + 8 + paddedLength + 8 + binaryLength;

        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);
        writer.Write(0x46546C67u);
        writer.Write(2u);
        writer.Write((uint)totalLength);
        writer.Write((uint)paddedLength);
        writer.Write(0x4E4F534Au);
        writer.Write(jsonBytes);
        for (var index = jsonBytes.Length; index < paddedLength; index++)
        {
            writer.Write((byte)' ');
        }

        writer.Write((uint)binaryLength);
        writer.Write(0x004E4942u);
        writer.Write(0f);
        writer.Write(0f);
        writer.Write(0f);
        writer.Write(1f);
        writer.Write(0f);
        writer.Write(0f);
        writer.Write(0f);
        writer.Write(1f);
        writer.Write(0f);
        writer.Write((ushort)0);
        writer.Write((ushort)1);
        writer.Write((ushort)2);
        writer.Write((ushort)0);
    }
}
