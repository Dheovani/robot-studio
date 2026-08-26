using System.Numerics;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class IndustrialArmMechanicalShowcaseDefinitionTests
{
    [Fact]
    public void PackagedAsset_ShouldImportEverySelectableSemanticPart()
    {
        var presentation = IndustrialArmMechanicalShowcaseDefinition.CreatePresentation();
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
    public void Create_ShouldRepresentSixSerialRevoluteJointsAndParallelGripper()
    {
        var showcase = IndustrialArmMechanicalShowcaseDefinition.Create();

        Assert.Equal("Six-Axis Industrial Serial Manipulator", showcase.Model.Name);
        AssertParent(showcase, "j1-turntable", "base");
        AssertParent(showcase, "j2-shoulder", "j1-turntable");
        AssertParent(showcase, "upper-arm", "j2-shoulder");
        AssertParent(showcase, "j3-elbow", "upper-arm");
        AssertParent(showcase, "forearm", "j3-elbow");
        AssertParent(showcase, "j4-wrist-roll", "forearm");
        AssertParent(showcase, "j5-wrist-bend", "wrist-roll-housing");
        AssertParent(showcase, "j6-tool-roll", "wrist-bend-housing");
        AssertParent(showcase, "tool", "j6-tool-roll");
        Assert.Equal(RobotPartKind.Tool, showcase.Model.GetPart(new RobotPartId("tool")).Kind);
    }

    [Fact]
    public void CoordinatedPick_DuringInterpolatedMotion_ShouldKeepAllJointPivotsConnected()
    {
        var presentation = IndustrialArmMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "coordinated-pick-tour");
        var sampled = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(6.25));
        var poses = MechanicalRevoluteJointPoseComposer.Compose(sampled, presentation.RevoluteJointPivots);
        var transforms = RobotComponentPoseResolver.ResolveWorldTransforms(presentation.Showcase.Model, poses);

        var parentByJoint = new Dictionary<RobotPartId, RobotPartId>
        {
            [new("j1-turntable")] = new("base"),
            [new("j2-shoulder")] = new("j1-turntable"),
            [new("j3-elbow")] = new("upper-arm"),
            [new("j4-wrist-roll")] = new("forearm"),
            [new("j5-wrist-bend")] = new("wrist-roll-housing"),
            [new("j6-tool-roll")] = new("wrist-bend-housing")
        };

        foreach (var pivot in presentation.RevoluteJointPivots)
        {
            AssertVectorApproximately(
                Vector3.Transform(pivot.PivotMillimeters, transforms[parentByJoint[pivot.PartId]]),
                Vector3.Transform(pivot.PivotMillimeters, transforms[pivot.PartId]));
        }
    }

    [Fact]
    public void WristOrientationTour_ShouldMoveOnlyTheThreeWristAxes()
    {
        var showcase = IndustrialArmMechanicalShowcaseDefinition.Create();
        var demonstration = showcase.Demonstrations.Single(item => item.Id == "wrist-orientation-tour");
        var sampled = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(7));
        var wristIds = new HashSet<RobotPartId>
        {
            new("j4-wrist-roll"),
            new("j5-wrist-bend"),
            new("j6-tool-roll")
        };

        Assert.All(
            sampled.Where(pose => !wristIds.Contains(pose.PartId)),
            pose => Assert.True(pose.PartId == new RobotPartId("j2-shoulder") ||
                                pose.PartId == new RobotPartId("j3-elbow") ||
                                pose.Rotation == Quaternion.Identity));
        Assert.NotEqual(Quaternion.Identity, sampled.Single(pose => pose.PartId == new RobotPartId("j5-wrist-bend")).Rotation);
    }

    [Fact]
    public void Presentation_ShouldExposeSixJointAxesAndPivots()
    {
        var presentation = IndustrialArmMechanicalShowcaseDefinition.CreatePresentation();

        Assert.Equal(6, presentation.MotionAxes.Count);
        Assert.Equal(6, presentation.RevoluteJointPivots.Count);
        Assert.Equal(
            [MechanicalMotionAxis.Z, MechanicalMotionAxis.Y, MechanicalMotionAxis.Y, MechanicalMotionAxis.X, MechanicalMotionAxis.Y, MechanicalMotionAxis.X],
            presentation.MotionAxes.Select(guide => guide.Axis));
    }

    [Fact]
    public void AssemblySequence_WhenCompleted_ShouldReturnEveryExplodedPartToAuthoredPose()
    {
        var presentation = IndustrialArmMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "assembly-sequence");
        var finalPoses = MechanicalDemonstrationSampler.Sample(demonstration, demonstration.Duration);

        var composed = MechanicalTeachingPoseComposer.Compose(
            presentation.Showcase.Model,
            finalPoses,
            MechanicalTeachingViewMode.ExplodedAssembly,
            presentation.ExplodedOffsets);

        Assert.All(composed, pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters));
    }

    private static void AssertParent(MechanicalShowcaseDefinition showcase, string partId, string parentId) =>
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
