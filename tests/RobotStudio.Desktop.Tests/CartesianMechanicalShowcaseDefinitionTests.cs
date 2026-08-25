using System.Numerics;
using HelixToolkit.SharpDX.Model.Scene;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class CartesianMechanicalShowcaseDefinitionTests
{
    [Fact]
    public void PackagedAsset_ShouldImportEverySelectableSemanticPart()
    {
        var showcase = CartesianMechanicalShowcaseDefinition.Create();
        var manifestPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Robots",
            "CartesianMechanical",
            "robot.json");

        var package = new RobotVisualAssetPackageLoader().Load(manifestPath, showcase.Model);
        using var scene = new HelixRobotVisualAssetImporter().Import(package);

        Assert.All(
            showcase.Model.Parts.Where(part => part.IsSelectable),
            part => Assert.NotEmpty(scene.NodesByPart[part.Id]));

        var geometryNodes = scene.NodesByPart.Values
            .SelectMany(nodes => nodes)
            .OfType<MaterialGeometryNode>()
            .Distinct()
            .ToArray();
        Assert.Contains(
            geometryNodes.GroupBy(node => node.Geometry),
            group => group.Count() > 1);
        Assert.Contains(
            geometryNodes.GroupBy(node => node.Material),
            group => group.Count() > 1);
    }

    [Fact]
    public void Create_ShouldRepresentARecognizableThreeAxisDesktopMachine()
    {
        var showcase = CartesianMechanicalShowcaseDefinition.Create();

        Assert.Equal("Desktop Cartesian Machine", showcase.Model.Name);
        AssertPartParent(showcase, "y-bed-carriage", "base");
        AssertPartParent(showcase, "build-plate", "y-bed-carriage");
        AssertPartParent(showcase, "z-gantry", "base");
        AssertPartParent(showcase, "x-tool-carriage", "z-gantry");
        AssertPartParent(showcase, "tool", "x-tool-carriage");
    }

    [Fact]
    public void Create_ShouldRepresentDualZHardwareAsOneSynchronizedGantry()
    {
        var showcase = CartesianMechanicalShowcaseDefinition.Create();
        var partIds = showcase.Model.Parts.Select(part => part.Id.Value).ToArray();

        Assert.Contains("left-z-motor", partIds);
        Assert.Contains("right-z-motor", partIds);
        Assert.Contains("left-z-screw", partIds);
        Assert.Contains("right-z-screw", partIds);
        Assert.Single(
            showcase.Demonstrations[0].Keyframes[0].ComponentPoses,
            pose => pose.PartId == new RobotPartId("z-gantry"));
    }

    [Fact]
    public void Create_ShouldOfferCoordinatedAndIndividualAxisDemonstrations()
    {
        var showcase = CartesianMechanicalShowcaseDefinition.Create();

        Assert.Equal(
            ["coordinated-axis-tour", "individual-axis-inspection"],
            showcase.Demonstrations.Select(demonstration => demonstration.Id));
        Assert.All(
            showcase.Demonstrations,
            demonstration => Assert.False(string.IsNullOrWhiteSpace(demonstration.Description)));
    }

    [Fact]
    public void IndividualAxisInspection_ShouldMoveOnlyOneLogicalAxisPerPhase()
    {
        var showcase = CartesianMechanicalShowcaseDefinition.Create();
        var demonstration = showcase.Demonstrations.Single(item => item.Id == "individual-axis-inspection");

        AssertAxisPhase(demonstration, 2, "y-bed-carriage", new Vector3(0, 160, 0));
        AssertAxisPhase(demonstration, 6, "x-tool-carriage", new Vector3(260, 0, 0));
        AssertAxisPhase(demonstration, 10, "z-gantry", new Vector3(0, 0, -120));
        Assert.All(
            demonstration.Keyframes.Where(frame => frame.Time.TotalSeconds is 0 or 4 or 8 or 12),
            frame => Assert.All(frame.ComponentPoses, pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters)));
    }

    private static void AssertAxisPhase(
        MechanicalDemonstrationDefinition demonstration,
        double seconds,
        string movingPartId,
        Vector3 expectedTranslation)
    {
        var frame = demonstration.Keyframes.Single(item => item.Time == TimeSpan.FromSeconds(seconds));

        Assert.Equal(
            expectedTranslation,
            frame.ComponentPoses.Single(pose => pose.PartId == new RobotPartId(movingPartId)).TranslationMillimeters);
        Assert.All(
            frame.ComponentPoses.Where(pose => pose.PartId != new RobotPartId(movingPartId)),
            pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters));
    }

    private static void AssertPartParent(
        MechanicalShowcaseDefinition showcase,
        string partId,
        string parentId)
    {
        var part = showcase.Model.GetPart(new RobotPartId(partId));

        Assert.Equal(new RobotPartId(parentId), part.ParentId);
    }
}
