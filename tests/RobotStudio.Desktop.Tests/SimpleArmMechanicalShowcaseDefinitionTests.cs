using System.Numerics;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class SimpleArmMechanicalShowcaseDefinitionTests
{
    [Fact]
    public void PackagedAsset_ShouldImportEverySelectableSemanticPart()
    {
        var presentation = SimpleArmMechanicalShowcaseDefinition.CreatePresentation();
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
    public void Create_ShouldRepresentAThreeJointSerialChain()
    {
        var showcase = SimpleArmMechanicalShowcaseDefinition.Create();

        Assert.Equal("Desktop Three-Joint Serial Arm", showcase.Model.Name);
        AssertPartParent(showcase, "turntable", "base");
        AssertPartParent(showcase, "shoulder-joint", "turntable");
        AssertPartParent(showcase, "upper-arm", "shoulder-joint");
        AssertPartParent(showcase, "elbow-joint", "upper-arm");
        AssertPartParent(showcase, "forearm", "elbow-joint");
        AssertPartParent(showcase, "wrist", "forearm");
        AssertPartParent(showcase, "tool", "wrist");
    }

    [Fact]
    public void ReachAndTransfer_DuringInterpolatedMotion_ShouldKeepSerialJointsConnected()
    {
        var presentation = SimpleArmMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "reach-and-transfer");
        var sampled = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(6.5));
        var poses = MechanicalRevoluteJointPoseComposer.Compose(sampled, presentation.RevoluteJointPivots);
        var transforms = RobotComponentPoseResolver.ResolveWorldTransforms(presentation.Showcase.Model, poses);
        var shoulderPivot = new Vector3(0, 0, 220);
        var elbowPivot = new Vector3(270, 0, 460);

        AssertVectorApproximately(
            Vector3.Transform(shoulderPivot, transforms[new RobotPartId("turntable")]),
            Vector3.Transform(shoulderPivot, transforms[new RobotPartId("shoulder-joint")]));
        AssertVectorApproximately(
            Vector3.Transform(elbowPivot, transforms[new RobotPartId("upper-arm")]),
            Vector3.Transform(elbowPivot, transforms[new RobotPartId("elbow-joint")]));
    }

    [Fact]
    public void IndividualJointInspection_ShouldMoveOneJointAtATime()
    {
        var showcase = SimpleArmMechanicalShowcaseDefinition.Create();
        var demonstration = showcase.Demonstrations.Single(item => item.Id == "individual-joint-inspection");
        var baseFrame = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(2));
        var shoulderFrame = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(6));
        var elbowFrame = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(10));

        AssertMovedOnly(baseFrame, "turntable");
        AssertMovedOnly(shoulderFrame, "shoulder-joint");
        AssertMovedOnly(elbowFrame, "elbow-joint");
    }

    [Fact]
    public void Presentation_ShouldExposeBaseShoulderAndElbowAxisGuides()
    {
        var presentation = SimpleArmMechanicalShowcaseDefinition.CreatePresentation();

        Assert.Equal(
            [MechanicalMotionAxis.Z, MechanicalMotionAxis.Y, MechanicalMotionAxis.Y],
            presentation.MotionAxes.Select(guide => guide.Axis));
        Assert.Equal(2, presentation.RevoluteJointPivots.Count);
    }

    [Fact]
    public void AssemblySequence_WhenCompleted_ShouldReturnEveryPartToItsAuthoredPose()
    {
        var presentation = SimpleArmMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "assembly-sequence");
        var finalPoses = MechanicalDemonstrationSampler.Sample(demonstration, demonstration.Duration);

        var composed = MechanicalTeachingPoseComposer.Compose(
            presentation.Showcase.Model,
            finalPoses,
            MechanicalTeachingViewMode.ExplodedAssembly,
            presentation.ExplodedOffsets);

        Assert.All(composed, pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters));
    }

    private static void AssertMovedOnly(IReadOnlyList<RobotComponentPose> poses, string movedPartId)
    {
        var movedId = new RobotPartId(movedPartId);
        Assert.NotEqual(Quaternion.Identity, poses.Single(pose => pose.PartId == movedId).Rotation);
        Assert.All(
            poses.Where(pose => pose.PartId != movedId),
            pose => Assert.Equal(Quaternion.Identity, pose.Rotation));
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
