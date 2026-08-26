using System.Numerics;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class DeltaMechanicalShowcaseDefinitionTests
{
    [Fact]
    public void PackagedAsset_ShouldImportEverySelectableSemanticPart()
    {
        var presentation = DeltaMechanicalShowcaseDefinition.CreatePresentation();
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
    public void Create_ShouldRepresentThreeActuatorsClosingOnOnePlatform()
    {
        var showcase = DeltaMechanicalShowcaseDefinition.Create();

        Assert.Equal("Three-Actuator Linear Delta Robot", showcase.Model.Name);
        foreach (var station in new[] { "a", "b", "c" })
        {
            AssertPartParent(showcase, $"actuator-{station}", "base");
            AssertPartParent(showcase, $"motor-{station}", $"actuator-{station}");
            AssertPartParent(showcase, $"carriage-{station}", $"actuator-{station}");
            AssertPartParent(showcase, $"link-{station}-left", "base");
            AssertPartParent(showcase, $"link-{station}-right", "base");
        }

        AssertPartParent(showcase, "platform", "base");
        AssertPartParent(showcase, "tool", "platform");
    }

    [Fact]
    public void PickAndPlace_DuringInterpolatedMotion_ShouldKeepEveryParallelLinkConnected()
    {
        var presentation = DeltaMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "pick-and-place");
        var sampled = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(6.25));
        var poses = MechanicalParallelLinkPoseComposer.Compose(
            presentation.Showcase.Model,
            sampled,
            presentation.ParallelLinkConstraints);
        var transforms = RobotComponentPoseResolver.ResolveWorldTransforms(presentation.Showcase.Model, poses);

        foreach (var link in presentation.ParallelLinkConstraints)
        {
            AssertVectorApproximately(
                Vector3.Transform(link.AuthoredStartMillimeters, transforms[link.StartPartId]),
                Vector3.Transform(link.AuthoredStartMillimeters, transforms[link.LinkPartId]));
            AssertVectorApproximately(
                Vector3.Transform(link.AuthoredEndMillimeters, transforms[link.EndPartId]),
                Vector3.Transform(link.AuthoredEndMillimeters, transforms[link.LinkPartId]));
        }
    }

    [Theory]
    [InlineData(2, "carriage-a", "carriage-b", "carriage-c")]
    [InlineData(6.5, "carriage-b", "carriage-a", "carriage-c")]
    [InlineData(11, "carriage-c", "carriage-a", "carriage-b")]
    public void IndividualActuatorInspection_ShouldMoveOneCarriageAndTheCoupledPlatform(
        double seconds,
        string movedCarriage,
        string stationaryCarriageOne,
        string stationaryCarriageTwo)
    {
        var showcase = DeltaMechanicalShowcaseDefinition.Create();
        var demonstration = showcase.Demonstrations.Single(item => item.Id == "individual-actuator-inspection");
        var poses = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(seconds));

        Assert.NotEqual(Vector3.Zero, TranslationOf(poses, movedCarriage));
        Assert.Equal(Vector3.Zero, TranslationOf(poses, stationaryCarriageOne));
        Assert.Equal(Vector3.Zero, TranslationOf(poses, stationaryCarriageTwo));
        Assert.NotEqual(Vector3.Zero, TranslationOf(poses, "platform"));
    }

    [Fact]
    public void Presentation_ShouldExposeThreeActuatorGuidesAndSixParallelConstraints()
    {
        var presentation = DeltaMechanicalShowcaseDefinition.CreatePresentation();

        Assert.Equal(6, presentation.ParallelLinkConstraints.Count);
        Assert.Equal(3, presentation.MotionAxes.Count(axis => axis.AttachedPartId?.Value.StartsWith("actuator-", StringComparison.Ordinal) == true));
        Assert.Equal(3, presentation.MotionAxes.Count(axis => axis.AttachedPartId == new RobotPartId("platform")));
    }

    [Fact]
    public void AssemblySequence_WhenCompleted_ShouldReturnEveryPartToItsAuthoredPose()
    {
        var presentation = DeltaMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "assembly-sequence");
        var finalPoses = MechanicalDemonstrationSampler.Sample(demonstration, demonstration.Duration);

        var composed = MechanicalTeachingPoseComposer.Compose(
            presentation.Showcase.Model,
            finalPoses,
            MechanicalTeachingViewMode.ExplodedAssembly,
            presentation.ExplodedOffsets);

        Assert.All(composed, pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters));
    }

    private static Vector3 TranslationOf(IEnumerable<RobotComponentPose> poses, string partId) =>
        poses.Single(pose => pose.PartId == new RobotPartId(partId)).TranslationMillimeters;

    private static void AssertPartParent(
        MechanicalShowcaseDefinition showcase,
        string partId,
        string parentId) =>
        Assert.Equal(
            new RobotPartId(parentId),
            showcase.Model.GetPart(new RobotPartId(partId)).ParentId);

    private static void AssertVectorApproximately(Vector3 expected, Vector3 actual)
    {
        Assert.InRange(actual.X, expected.X - 0.001f, expected.X + 0.001f);
        Assert.InRange(actual.Y, expected.Y - 0.001f, expected.Y + 0.001f);
        Assert.InRange(actual.Z, expected.Z - 0.001f, expected.Z + 0.001f);
    }
}
