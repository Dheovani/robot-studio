using System.Numerics;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class XYPlotterMechanicalShowcaseDefinitionTests
{
    [Fact]
    public void PackagedAsset_ShouldImportEverySelectableSemanticPart()
    {
        var presentation = XYPlotterMechanicalShowcaseDefinition.CreatePresentation();
        var manifestPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Robots",
            presentation.AssetDirectoryName,
            "robot.json");

        var package = new RobotVisualAssetPackageLoader().Load(manifestPath, presentation.Showcase.Model);
        using var scene = new HelixRobotVisualAssetImporter().Import(package);

        Assert.All(
            presentation.Showcase.Model.Parts.Where(part => part.IsSelectable),
            part => Assert.NotEmpty(scene.NodesByPart[part.Id]));
    }

    [Fact]
    public void Create_ShouldRepresentAPlanarBridgeAndPenHierarchy()
    {
        var showcase = XYPlotterMechanicalShowcaseDefinition.Create();

        Assert.Equal("Two-Axis Pen Plotter", showcase.Model.Name);
        AssertPartParent(showcase, "y-gantry", "base");
        AssertPartParent(showcase, "x-rail", "y-gantry");
        AssertPartParent(showcase, "x-carriage", "y-gantry");
        AssertPartParent(showcase, "pen-lift", "x-carriage");
        AssertPartParent(showcase, "pen", "pen-lift");
    }

    [Fact]
    public void RectangularPathTour_ShouldCoordinateXAndYAtEveryCorner()
    {
        var showcase = XYPlotterMechanicalShowcaseDefinition.Create();
        var demonstration = showcase.Demonstrations.Single(item => item.Id == "rectangular-path-tour");
        var expectedCorners = new[]
        {
            (Seconds: 0d, X: 0f, Y: 0f),
            (Seconds: 2d, X: 250f, Y: 0f),
            (Seconds: 4d, X: 250f, Y: 150f),
            (Seconds: 6d, X: -150f, Y: 150f),
            (Seconds: 8d, X: -150f, Y: 0f),
            (Seconds: 10d, X: 0f, Y: 0f)
        };

        Assert.Equal(TimeSpan.FromSeconds(10), demonstration.Duration);
        foreach (var corner in expectedCorners)
        {
            var frame = demonstration.Keyframes.Single(item => item.Time == TimeSpan.FromSeconds(corner.Seconds));
            Assert.Equal(
                new Vector3(corner.X, 0, 0),
                frame.ComponentPoses.Single(pose => pose.PartId == new RobotPartId("x-carriage")).TranslationMillimeters);
            Assert.Equal(
                new Vector3(0, corner.Y, 0),
                frame.ComponentPoses.Single(pose => pose.PartId == new RobotPartId("y-gantry")).TranslationMillimeters);
        }
    }

    [Fact]
    public void IndividualAxisInspection_ShouldMoveOnlyOnePlanarAxisPerPhase()
    {
        var showcase = XYPlotterMechanicalShowcaseDefinition.Create();
        var demonstration = showcase.Demonstrations.Single(item => item.Id == "individual-axis-inspection");

        AssertPhase(demonstration, 2, new Vector3(0, 160, 0), Vector3.Zero);
        AssertPhase(demonstration, 6, Vector3.Zero, new Vector3(260, 0, 0));
    }

    [Fact]
    public void Presentation_ShouldExposeOnlyXAndYMotionGuides()
    {
        var presentation = XYPlotterMechanicalShowcaseDefinition.CreatePresentation();

        Assert.Equal(
            [MechanicalMotionAxis.X, MechanicalMotionAxis.Y],
            presentation.MotionAxes.Select(guide => guide.Axis));
        Assert.DoesNotContain(presentation.MotionAxes, guide => guide.Axis == MechanicalMotionAxis.Z);
    }

    [Fact]
    public void AssemblySequence_WhenCompleted_ShouldReturnEveryPartToItsAuthoredPose()
    {
        var presentation = XYPlotterMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "assembly-sequence");
        var finalPoses = MechanicalDemonstrationSampler.Sample(demonstration, demonstration.Duration);

        var composed = MechanicalTeachingPoseComposer.Compose(
            presentation.Showcase.Model,
            finalPoses,
            MechanicalTeachingViewMode.ExplodedAssembly,
            presentation.ExplodedOffsets);

        Assert.All(composed, pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters));
    }

    private static void AssertPhase(
        MechanicalDemonstrationDefinition demonstration,
        double seconds,
        Vector3 expectedY,
        Vector3 expectedX)
    {
        var frame = demonstration.Keyframes.Single(item => item.Time == TimeSpan.FromSeconds(seconds));
        Assert.Equal(
            expectedY,
            frame.ComponentPoses.Single(pose => pose.PartId == new RobotPartId("y-gantry")).TranslationMillimeters);
        Assert.Equal(
            expectedX,
            frame.ComponentPoses.Single(pose => pose.PartId == new RobotPartId("x-carriage")).TranslationMillimeters);
    }

    private static void AssertPartParent(
        MechanicalShowcaseDefinition showcase,
        string partId,
        string parentId) =>
        Assert.Equal(
            new RobotPartId(parentId),
            showcase.Model.GetPart(new RobotPartId(partId)).ParentId);
}
