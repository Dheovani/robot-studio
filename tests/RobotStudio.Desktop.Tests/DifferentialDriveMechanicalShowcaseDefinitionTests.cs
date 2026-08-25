using System.Numerics;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class DifferentialDriveMechanicalShowcaseDefinitionTests
{
    [Fact]
    public void PackagedAsset_ShouldImportEverySelectableSemanticPart()
    {
        var presentation = DifferentialDriveMechanicalShowcaseDefinition.CreatePresentation();
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
    public void Create_ShouldRepresentIndependentDriveUnitsAndSensors()
    {
        var showcase = DifferentialDriveMechanicalShowcaseDefinition.Create();

        Assert.Equal("Round Differential-Drive Service Robot", showcase.Model.Name);
        AssertPartParent(showcase, "left-motor", "base");
        AssertPartParent(showcase, "left-wheel", "left-motor");
        AssertPartParent(showcase, "left-encoder", "left-motor");
        AssertPartParent(showcase, "right-motor", "base");
        AssertPartParent(showcase, "right-wheel", "right-motor");
        AssertPartParent(showcase, "right-encoder", "right-motor");
        Assert.Equal(RobotPartKind.Wheel, showcase.Model.GetPart(new RobotPartId("caster")).Kind);
    }

    [Fact]
    public void DriveAndTurnTour_ShouldCloseItsSquareRouteAtTheInitialPose()
    {
        var showcase = DifferentialDriveMechanicalShowcaseDefinition.Create();
        var demonstration = showcase.Demonstrations.Single(item => item.Id == "drive-and-turn-tour");
        var initial = demonstration.Keyframes[0].ComponentPoses.Single();
        var final = demonstration.Keyframes[^1].ComponentPoses.Single();

        Assert.Equal(TimeSpan.FromSeconds(16), demonstration.Duration);
        Assert.Equal(Vector3.Zero, initial.TranslationMillimeters);
        Assert.Equal(Vector3.Zero, final.TranslationMillimeters);
        AssertQuaternionEquivalent(Quaternion.Identity, final.Rotation);
        Assert.Contains(
            demonstration.Keyframes,
            frame => frame.ComponentPoses.Single().TranslationMillimeters == new Vector3(300, 300, 0));
    }

    [Fact]
    public void TurningComparison_ShouldRotateInBothDirectionsWithoutTranslation()
    {
        var showcase = DifferentialDriveMechanicalShowcaseDefinition.Create();
        var demonstration = showcase.Demonstrations.Single(item => item.Id == "turning-comparison");
        var poses = demonstration.Keyframes.Select(frame => frame.ComponentPoses.Single()).ToArray();

        Assert.All(poses, pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters));
        AssertQuaternionEquivalent(Yaw(90), poses[1].Rotation);
        AssertQuaternionEquivalent(Yaw(-90), poses[3].Rotation);
    }

    [Fact]
    public void Presentation_ShouldAttachBodyFrameGuidesToTheMovingChassis()
    {
        var presentation = DifferentialDriveMechanicalShowcaseDefinition.CreatePresentation();

        Assert.Equal(
            [MechanicalMotionAxis.X, MechanicalMotionAxis.Y],
            presentation.MotionAxes.Select(guide => guide.Axis));
        Assert.All(
            presentation.MotionAxes,
            guide => Assert.Equal(new RobotPartId("base"), guide.AttachedPartId));
    }

    [Fact]
    public void AssemblySequence_WhenCompleted_ShouldReturnEveryPartToItsAuthoredPose()
    {
        var presentation = DifferentialDriveMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "assembly-sequence");
        var finalPoses = MechanicalDemonstrationSampler.Sample(demonstration, demonstration.Duration);

        var composed = MechanicalTeachingPoseComposer.Compose(
            presentation.Showcase.Model,
            finalPoses,
            MechanicalTeachingViewMode.ExplodedAssembly,
            presentation.ExplodedOffsets);

        Assert.All(composed, pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters));
    }

    private static Quaternion Yaw(float degrees) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitZ, degrees * MathF.PI / 180);

    private static void AssertQuaternionEquivalent(Quaternion expected, Quaternion actual)
    {
        var dot = MathF.Abs(Quaternion.Dot(expected, actual));
        Assert.InRange(dot, 0.9999f, 1.0001f);
    }

    private static void AssertPartParent(
        MechanicalShowcaseDefinition showcase,
        string partId,
        string parentId) =>
        Assert.Equal(
            new RobotPartId(parentId),
            showcase.Model.GetPart(new RobotPartId(partId)).ParentId);
}
