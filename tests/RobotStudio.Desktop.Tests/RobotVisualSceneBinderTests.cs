using HelixToolkit.SharpDX.Model.Scene;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;
using RobotStudio.Visualization.Assets;

namespace RobotStudio.Desktop.Tests;

public sealed class RobotVisualSceneBinderTests
{
    [Fact]
    public void Bind_WhenNamedComponentContainsMeshChildren_ShouldInheritSemanticPart()
    {
        var root = Node("Scene");
        var component = Node("Machine_Base");
        var mesh = Node("Machine_Base_Mesh");
        component.AddChildNode(mesh);
        root.AddChildNode(component);

        var scene = RobotVisualSceneBinder.Bind(
            root,
            Manifest(("Machine_Base", "base")));

        var partId = new RobotPartId("base");
        Assert.Equal(partId, component.Tag);
        Assert.Equal(partId, mesh.Tag);
        Assert.Equal([component, mesh], scene.NodesByPart[partId]);
        Assert.True(mesh.IsHitTestVisible);
    }

    [Fact]
    public void Bind_WhenChildHasExplicitBinding_ShouldUseChildSemanticPartForItsSubtree()
    {
        var root = Node("Scene");
        var machine = Node("Machine");
        var tool = Node("Tool");
        var nozzle = Node("Nozzle_Mesh");
        tool.AddChildNode(nozzle);
        machine.AddChildNode(tool);
        root.AddChildNode(machine);

        var scene = RobotVisualSceneBinder.Bind(
            root,
            Manifest(("Machine", "base"), ("Tool", "tool")));

        Assert.Equal(new RobotPartId("base"), machine.Tag);
        Assert.Equal(new RobotPartId("tool"), tool.Tag);
        Assert.Equal(new RobotPartId("tool"), nozzle.Tag);
        Assert.DoesNotContain(
            scene.NodesByPart[new RobotPartId("base")],
            node => ReferenceEquals(node, tool));
    }

    [Fact]
    public void Bind_WhenSeveralNodesMapToOnePart_ShouldCollectAllSubtrees()
    {
        var root = Node("Scene");
        var toolBody = Node("Tool_Body");
        var toolNozzle = Node("Tool_Nozzle");
        root.AddChildNode(toolBody);
        root.AddChildNode(toolNozzle);

        var scene = RobotVisualSceneBinder.Bind(
            root,
            Manifest(("Tool_Body", "tool"), ("Tool_Nozzle", "tool")));

        Assert.Equal(2, scene.NodesByPart[new RobotPartId("tool")].Count);
    }

    [Fact]
    public void Bind_WhenManifestNodeIsMissing_ShouldReportMissingNode()
    {
        var root = Node("Scene");

        var exception = Assert.Throws<RobotVisualAssetException>(() =>
            RobotVisualSceneBinder.Bind(root, Manifest(("Machine_Base", "base"))));

        Assert.Equal(RobotVisualAssetErrorCode.AssetNodeMissing, exception.Code);
    }

    [Fact]
    public void Bind_WhenManifestNodeNameOccursTwice_ShouldReportAmbiguousNode()
    {
        var root = Node("Scene");
        root.AddChildNode(Node("Machine_Base"));
        root.AddChildNode(Node("Machine_Base"));

        var exception = Assert.Throws<RobotVisualAssetException>(() =>
            RobotVisualSceneBinder.Bind(root, Manifest(("Machine_Base", "base"))));

        Assert.Equal(RobotVisualAssetErrorCode.AssetNodeAmbiguous, exception.Code);
    }

    private static GroupNode Node(string name) => new() { Name = name };

    private static RobotVisualAssetManifest Manifest(
        params (string NodeName, string PartId)[] bindings) =>
        new(
            RobotVisualAssetManifest.CurrentSchemaVersion,
            "cartesian",
            "robot.glb",
            bindings.Select(binding => new RobotVisualNodeBinding(
                binding.NodeName,
                new RobotPartId(binding.PartId))));
}
