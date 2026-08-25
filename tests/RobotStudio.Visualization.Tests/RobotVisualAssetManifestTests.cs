using System.Text;
using RobotStudio.Visualization.Assets;

namespace RobotStudio.Visualization.Tests;

public sealed class RobotVisualAssetManifestTests
{
    [Fact]
    public void Read_WhenJsonIsValid_ShouldCreateManifest()
    {
        using var stream = JsonStream(ValidManifestJson);

        var manifest = RobotVisualAssetManifestReader.Read(stream);

        Assert.Equal(1, manifest.SchemaVersion);
        Assert.Equal("cartesian", manifest.ModelId);
        Assert.Equal("robot.glb", manifest.AssetFile);
        Assert.Equal(2, manifest.NodeBindings.Count);
        Assert.Equal(new RobotPartId("tool"), manifest.NodeBindings[1].PartId);
    }

    [Fact]
    public void Read_WhenJsonIsMalformed_ShouldReportInvalidJson()
    {
        using var stream = JsonStream("{ not-json }");

        var exception = Assert.Throws<RobotVisualAssetException>(
            () => RobotVisualAssetManifestReader.Read(stream));

        Assert.Equal(RobotVisualAssetErrorCode.InvalidJson, exception.Code);
    }

    [Fact]
    public void Validate_WhenManifestCoversSelectableParts_ShouldSucceed()
    {
        var manifest = CreateManifest(
            new RobotVisualNodeBinding("Machine_Base", new RobotPartId("base")),
            new RobotVisualNodeBinding("Tool_Body", new RobotPartId("tool")),
            new RobotVisualNodeBinding("Tool_Nozzle", new RobotPartId("tool")));

        RobotVisualAssetManifestValidator.Validate(manifest, CreateModel());
    }

    [Fact]
    public void Validate_WhenSchemaVersionIsUnsupported_ShouldReportVersion()
    {
        var manifest = new RobotVisualAssetManifest(
            schemaVersion: 2,
            modelId: "cartesian",
            assetFile: "robot.glb",
            [new RobotVisualNodeBinding("Machine_Base", new RobotPartId("base"))]);

        var exception = Assert.Throws<RobotVisualAssetException>(
            () => RobotVisualAssetManifestValidator.Validate(manifest, CreateModel()));

        Assert.Equal(RobotVisualAssetErrorCode.UnsupportedSchemaVersion, exception.Code);
    }

    [Fact]
    public void Validate_WhenModelDoesNotMatch_ShouldReportMismatch()
    {
        var manifest = new RobotVisualAssetManifest(
            schemaVersion: 1,
            modelId: "drone",
            assetFile: "robot.glb",
            [new RobotVisualNodeBinding("Machine_Base", new RobotPartId("base"))]);

        var exception = Assert.Throws<RobotVisualAssetException>(
            () => RobotVisualAssetManifestValidator.Validate(manifest, CreateModel()));

        Assert.Equal(RobotVisualAssetErrorCode.ModelMismatch, exception.Code);
    }

    [Theory]
    [InlineData("../robot.glb")]
    [InlineData("/robot.glb")]
    [InlineData("C:/robot.glb")]
    [InlineData("models\\robot.glb")]
    [InlineData("models//robot.glb")]
    public void Validate_WhenAssetPathEscapesPackage_ShouldRejectPath(string assetFile)
    {
        var manifest = new RobotVisualAssetManifest(
            schemaVersion: 1,
            modelId: "cartesian",
            assetFile,
            [new RobotVisualNodeBinding("Machine_Base", new RobotPartId("base"))]);

        var exception = Assert.Throws<RobotVisualAssetException>(
            () => RobotVisualAssetManifestValidator.Validate(manifest, CreateModel()));

        Assert.Equal(RobotVisualAssetErrorCode.UnsafeAssetPath, exception.Code);
    }

    [Fact]
    public void Validate_WhenNodeNameIsDuplicated_ShouldRejectManifest()
    {
        var manifest = CreateManifest(
            new RobotVisualNodeBinding("Machine_Base", new RobotPartId("base")),
            new RobotVisualNodeBinding("Machine_Base", new RobotPartId("tool")));

        var exception = Assert.Throws<RobotVisualAssetException>(
            () => RobotVisualAssetManifestValidator.Validate(manifest, CreateModel()));

        Assert.Equal(RobotVisualAssetErrorCode.InvalidManifest, exception.Code);
    }

    [Fact]
    public void Validate_WhenNodeMapsToUnknownPart_ShouldReportUnknownPart()
    {
        var manifest = CreateManifest(
            new RobotVisualNodeBinding("Machine_Base", new RobotPartId("base")),
            new RobotVisualNodeBinding("Unknown", new RobotPartId("missing")));

        var exception = Assert.Throws<RobotVisualAssetException>(
            () => RobotVisualAssetManifestValidator.Validate(manifest, CreateModel()));

        Assert.Equal(RobotVisualAssetErrorCode.UnknownPart, exception.Code);
    }

    [Fact]
    public void Validate_WhenSelectablePartHasNoNode_ShouldReportMissingBinding()
    {
        var manifest = CreateManifest(
            new RobotVisualNodeBinding("Machine_Base", new RobotPartId("base")));

        var exception = Assert.Throws<RobotVisualAssetException>(
            () => RobotVisualAssetManifestValidator.Validate(manifest, CreateModel()));

        Assert.Equal(RobotVisualAssetErrorCode.MissingSemanticBinding, exception.Code);
    }

    private static RobotVisualAssetManifest CreateManifest(params RobotVisualNodeBinding[] bindings) =>
        new(1, "cartesian", "robot.glb", bindings);

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
                    "Remains fixed."),
                new RobotPartDefinition(
                    new RobotPartId("tool"),
                    "Tool",
                    RobotPartKind.Tool,
                    baseId,
                    "Deposits material.",
                    "Moves with the carriage."),
                new RobotPartDefinition(
                    new RobotPartId("cover"),
                    "Decorative cover",
                    RobotPartKind.Structure,
                    baseId,
                    "Protects internal parts.",
                    "Remains fixed.",
                    isSelectable: false)
            ]);
    }

    private static MemoryStream JsonStream(string json) =>
        new(Encoding.UTF8.GetBytes(json));

    private const string ValidManifestJson = """
        {
          "schemaVersion": 1,
          "modelId": "cartesian",
          "assetFile": "robot.glb",
          "nodes": [
            { "nodeName": "Machine_Base", "partId": "base" },
            { "nodeName": "Tool", "partId": "tool" }
          ]
        }
        """;
}
