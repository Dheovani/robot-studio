using System.Numerics;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class ScaraMechanicalShowcaseDefinitionTests
{
    [Fact]
    public void PackagedAsset_ShouldImportEverySelectableSemanticPart()
    {
        var presentation = ScaraMechanicalShowcaseDefinition.CreatePresentation();
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
    public void Create_ShouldRepresentTheCompleteScaraKinematicChain()
    {
        var showcase = ScaraMechanicalShowcaseDefinition.Create();

        Assert.Equal("Selective-Compliance Assembly Robot Arm", showcase.Model.Name);
        AssertPartParent(showcase, "first-link", "base");
        AssertPartParent(showcase, "elbow-joint", "first-link");
        AssertPartParent(showcase, "second-link", "elbow-joint");
        AssertPartParent(showcase, "z-actuator", "second-link");
        AssertPartParent(showcase, "tool", "z-actuator");
        Assert.Equal(RobotPartKind.Joint, showcase.Model.GetPart(new RobotPartId("elbow-joint")).Kind);
        Assert.Equal(RobotPartKind.Actuator, showcase.Model.GetPart(new RobotPartId("z-actuator")).Kind);
    }

    [Fact]
    public void PickAndPlaceCycle_DuringInterpolatedMotion_ShouldKeepBothLinksConnected()
    {
        var presentation = ScaraMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "pick-and-place-cycle");
        var sampled = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(5.5));
        var poses = MechanicalRevoluteJointPoseComposer.Compose(sampled, presentation.RevoluteJointPivots);
        var transforms = RobotComponentPoseResolver.ResolveWorldTransforms(presentation.Showcase.Model, poses);
        var pivot = new Vector3(325, 0, 0);

        var firstLinkEndpoint = Vector3.Transform(pivot, transforms[new RobotPartId("first-link")]);
        var elbowPivot = Vector3.Transform(pivot, transforms[new RobotPartId("elbow-joint")]);

        AssertVectorApproximately(firstLinkEndpoint, elbowPivot);
    }

    [Fact]
    public void PickAndPlaceCycle_ShouldMoveBothRotaryJointsAndVerticalSpindle()
    {
        var showcase = ScaraMechanicalShowcaseDefinition.Create();
        var demonstration = showcase.Demonstrations.Single(item => item.Id == "pick-and-place-cycle");
        var planarPoses = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(2));
        var loweredPoses = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(3));

        Assert.NotEqual(
            Quaternion.Identity,
            planarPoses.Single(pose => pose.PartId == new RobotPartId("first-link")).Rotation);
        Assert.NotEqual(
            Quaternion.Identity,
            planarPoses.Single(pose => pose.PartId == new RobotPartId("elbow-joint")).Rotation);
        Assert.Equal(
            new Vector3(0, 0, -95),
            loweredPoses.Single(pose => pose.PartId == new RobotPartId("z-actuator")).TranslationMillimeters);
    }

    [Fact]
    public void Presentation_ShouldExposePlanarAndVerticalMotionGuides()
    {
        var presentation = ScaraMechanicalShowcaseDefinition.CreatePresentation();

        Assert.Equal(
            [MechanicalMotionAxis.X, MechanicalMotionAxis.Y, MechanicalMotionAxis.Z],
            presentation.MotionAxes.Select(guide => guide.Axis));
        Assert.Equal(
            new RobotPartId("z-actuator"),
            presentation.MotionAxes.Single(guide => guide.Axis == MechanicalMotionAxis.Z).AttachedPartId);
    }

    [Fact]
    public void AssemblySequence_WhenCompleted_ShouldReturnEveryPartToItsAuthoredPose()
    {
        var presentation = ScaraMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "assembly-sequence");
        var finalPoses = MechanicalDemonstrationSampler.Sample(demonstration, demonstration.Duration);

        var composed = MechanicalTeachingPoseComposer.Compose(
            presentation.Showcase.Model,
            finalPoses,
            MechanicalTeachingViewMode.ExplodedAssembly,
            presentation.ExplodedOffsets);

        Assert.All(composed, pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters));
    }

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
