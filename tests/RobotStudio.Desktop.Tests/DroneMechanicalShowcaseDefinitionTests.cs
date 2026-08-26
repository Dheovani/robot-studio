using System.Numerics;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class DroneMechanicalShowcaseDefinitionTests
{
    private static readonly string[] RotorPositions =
        ["front-left", "front-right", "rear-left", "rear-right"];

    [Fact]
    public void PackagedAsset_ShouldImportEverySelectableSemanticPart()
    {
        var presentation = DroneMechanicalShowcaseDefinition.CreatePresentation();
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
    public void Create_ShouldRepresentFourCompletePropulsionUnits()
    {
        var showcase = DroneMechanicalShowcaseDefinition.Create();

        Assert.Equal("X-Configuration Technical Quadcopter", showcase.Model.Name);
        foreach (var position in RotorPositions)
        {
            AssertPartParent(showcase, $"arm-{position}", "airframe");
            AssertPartParent(showcase, $"motor-{position}", $"arm-{position}");
            AssertPartParent(showcase, $"propeller-{position}", $"motor-{position}");
            Assert.Equal(
                RobotPartKind.Propeller,
                showcase.Model.GetPart(new RobotPartId($"propeller-{position}")).Kind);
        }
    }

    [Fact]
    public void Presentation_ShouldKeepAllFourTwoBladePropellersVisibleAndSelectable()
    {
        var presentation = DroneMechanicalShowcaseDefinition.CreatePresentation();

        Assert.Equal(4, presentation.RevoluteJointPivots.Count);
        foreach (var position in RotorPositions)
        {
            var partId = new RobotPartId($"propeller-{position}");
            Assert.Contains(presentation.RevoluteJointPivots, pivot => pivot.PartId == partId);
            Assert.True(presentation.Showcase.Model.GetPart(partId).IsSelectable);
            Assert.Equal(2, presentation.FallbackPrimitives.Count(primitive => primitive.PartId == partId));
        }
    }

    [Fact]
    public void FlightTour_DuringAttitudeChange_ShouldKeepPropellersOnTheirMotorAxes()
    {
        var presentation = DroneMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "flight-and-attitude-tour");
        var sampled = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(5.25));
        var poses = MechanicalRevoluteJointPoseComposer.Compose(sampled, presentation.RevoluteJointPivots);
        var transforms = RobotComponentPoseResolver.ResolveWorldTransforms(presentation.Showcase.Model, poses);

        foreach (var pivot in presentation.RevoluteJointPivots)
        {
            var motorId = presentation.Showcase.Model.GetPart(pivot.PartId).ParentId!.Value;
            AssertVectorApproximately(
                Vector3.Transform(pivot.PivotMillimeters, transforms[motorId]),
                Vector3.Transform(pivot.PivotMillimeters, transforms[pivot.PartId]));
        }
    }

    [Fact]
    public void MotorPairInspection_ShouldRunEachDiagonalPairSeparately()
    {
        var showcase = DroneMechanicalShowcaseDefinition.Create();
        var demonstration = showcase.Demonstrations.Single(item => item.Id == "motor-pair-inspection");
        var firstPair = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(2));
        var secondPair = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(5));

        AssertRotating(firstPair, "front-left", "rear-right");
        AssertStationary(firstPair, "front-right", "rear-left");
        AssertRotating(secondPair, "front-right", "rear-left");
    }

    [Fact]
    public void Presentation_ShouldAttachBodyAxesToMovingAirframe()
    {
        var presentation = DroneMechanicalShowcaseDefinition.CreatePresentation();

        Assert.Equal(
            [MechanicalMotionAxis.X, MechanicalMotionAxis.Y, MechanicalMotionAxis.Z],
            presentation.MotionAxes.Select(axis => axis.Axis));
        Assert.All(
            presentation.MotionAxes,
            axis => Assert.Equal(new RobotPartId("airframe"), axis.AttachedPartId));
    }

    [Fact]
    public void AssemblySequence_WhenCompleted_ShouldReturnEveryPartToItsAuthoredPose()
    {
        var presentation = DroneMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "assembly-sequence");
        var finalPoses = MechanicalDemonstrationSampler.Sample(demonstration, demonstration.Duration);

        var composed = MechanicalTeachingPoseComposer.Compose(
            presentation.Showcase.Model,
            finalPoses,
            MechanicalTeachingViewMode.ExplodedAssembly,
            presentation.ExplodedOffsets);

        Assert.All(composed, pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters));
    }

    private static void AssertRotating(IEnumerable<RobotComponentPose> poses, params string[] positions) =>
        Assert.All(
            positions,
            position => Assert.NotEqual(
                Quaternion.Identity,
                poses.Single(pose => pose.PartId == new RobotPartId($"propeller-{position}")).Rotation));

    private static void AssertStationary(IEnumerable<RobotComponentPose> poses, params string[] positions) =>
        Assert.All(
            positions,
            position => Assert.Equal(
                Quaternion.Identity,
                poses.Single(pose => pose.PartId == new RobotPartId($"propeller-{position}")).Rotation));

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
