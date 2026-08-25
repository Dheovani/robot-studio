using System.Numerics;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class MechanicalTeachingPoseComposerTests
{
    [Fact]
    public void Compose_WhenLayerIsAssembled_ShouldPreserveDemonstrationPoses()
    {
        var presentation = CartesianMechanicalShowcaseDefinition.CreatePresentation();
        var poses = new[]
        {
            Pose("x-tool-carriage", new Vector3(80, 0, 0))
        };

        var composed = MechanicalTeachingPoseComposer.Compose(
            presentation.Showcase.Model,
            poses,
            MechanicalTeachingViewMode.Assembled,
            presentation.ExplodedOffsets);

        Assert.Equal(poses, composed);
    }

    [Fact]
    public void Compose_WhenLayerIsExploded_ShouldAddOffsetsAndPreserveHierarchyInputs()
    {
        var presentation = CartesianMechanicalShowcaseDefinition.CreatePresentation();
        var poses = new[]
        {
            Pose("x-tool-carriage", new Vector3(80, 0, 0)),
            Pose("z-gantry", new Vector3(0, 0, -40))
        };

        var composed = MechanicalTeachingPoseComposer.Compose(
                presentation.Showcase.Model,
                poses,
                MechanicalTeachingViewMode.ExplodedAssembly,
                presentation.ExplodedOffsets)
            .ToDictionary(pose => pose.PartId);

        Assert.Equal(
            new Vector3(200, 0, 0),
            composed[new RobotPartId("x-tool-carriage")].TranslationMillimeters);
        Assert.Equal(
            new Vector3(0, 0, 60),
            composed[new RobotPartId("z-gantry")].TranslationMillimeters);
        Assert.Equal(
            new Vector3(0, -60, -40),
            composed[new RobotPartId("tool")].TranslationMillimeters);
        Assert.Equal(
            Vector3.Zero,
            composed[new RobotPartId("base")].TranslationMillimeters);
    }

    [Fact]
    public void Compose_WhenAssemblySequenceFinishes_ShouldReturnEveryPartToItsAuthoredPose()
    {
        var presentation = CartesianMechanicalShowcaseDefinition.CreatePresentation();
        var demonstration = presentation.Showcase.Demonstrations.Single(item => item.Id == "assembly-sequence");
        var finalPoses = MechanicalDemonstrationSampler.Sample(demonstration, demonstration.Duration);

        var composed = MechanicalTeachingPoseComposer.Compose(
            presentation.Showcase.Model,
            finalPoses,
            MechanicalTeachingViewMode.ExplodedAssembly,
            presentation.ExplodedOffsets);

        Assert.All(
            composed,
            pose => Assert.Equal(Vector3.Zero, pose.TranslationMillimeters));
    }

    private static RobotComponentPose Pose(string partId, Vector3 translation) =>
        new(new RobotPartId(partId), translation, Quaternion.Identity, Vector3.One);
}
