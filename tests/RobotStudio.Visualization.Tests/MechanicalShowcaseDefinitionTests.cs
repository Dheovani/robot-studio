using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Visualization.Tests;

public sealed class MechanicalShowcaseDefinitionTests
{
    [Fact]
    public void Constructor_WhenDemonstrationIsValid_ShouldPreserveCuratedTimeline()
    {
        var model = CreateModel();
        var demonstration = CreateDemonstration(new RobotPartId("tool"));

        var showcase = new MechanicalShowcaseDefinition(model, [demonstration]);

        Assert.Same(model, showcase.Model);
        Assert.Equal(TimeSpan.FromSeconds(2), showcase.Demonstrations[0].Duration);
    }

    [Fact]
    public void Constructor_WhenDemonstrationReferencesUnknownPart_ShouldThrow()
    {
        var model = CreateModel();
        var demonstration = CreateDemonstration(new RobotPartId("unknown"));

        var exception = Assert.Throws<ArgumentException>(() =>
            new MechanicalShowcaseDefinition(model, [demonstration]));

        Assert.Contains("unknown visual part", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Demonstration_WhenFirstKeyframeDoesNotStartAtZero_ShouldThrow()
    {
        var pose = RobotComponentPose.Identity(new RobotPartId("tool"));

        var exception = Assert.Throws<ArgumentException>(() => new MechanicalDemonstrationDefinition(
            "axis-motion",
            "Axis motion",
            "Shows coordinated linear movement.",
            TimeSpan.FromSeconds(2),
            [
                new MechanicalKeyframe(TimeSpan.FromMilliseconds(100), [pose]),
                new MechanicalKeyframe(TimeSpan.FromSeconds(2), [pose])
            ]));

        Assert.Contains("start at zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Demonstration_WhenKeyframeTimesAreNotIncreasing_ShouldThrow()
    {
        var pose = RobotComponentPose.Identity(new RobotPartId("tool"));

        var exception = Assert.Throws<ArgumentException>(() => new MechanicalDemonstrationDefinition(
            "axis-motion",
            "Axis motion",
            "Shows coordinated linear movement.",
            TimeSpan.FromSeconds(2),
            [
                new MechanicalKeyframe(TimeSpan.Zero, [pose]),
                new MechanicalKeyframe(TimeSpan.FromSeconds(1), [pose]),
                new MechanicalKeyframe(TimeSpan.FromSeconds(1), [pose])
            ]));

        Assert.Contains("increasing", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Keyframe_WhenPartHasTwoPoses_ShouldThrow()
    {
        var partId = new RobotPartId("tool");
        var first = RobotComponentPose.Identity(partId);
        var second = first with { TranslationMillimeters = new Vector3(10, 0, 0) };

        var exception = Assert.Throws<ArgumentException>(() =>
            new MechanicalKeyframe(TimeSpan.Zero, [first, second]));

        Assert.Contains("more than one pose", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Demonstration_WhenKeyframesDescribeDifferentParts_ShouldThrow()
    {
        var first = RobotComponentPose.Identity(new RobotPartId("tool"));
        var second = RobotComponentPose.Identity(new RobotPartId("carriage"));

        var exception = Assert.Throws<ArgumentException>(() => new MechanicalDemonstrationDefinition(
            "axis-motion",
            "Axis motion",
            "Shows coordinated linear movement.",
            TimeSpan.FromSeconds(2),
            [
                new MechanicalKeyframe(TimeSpan.Zero, [first]),
                new MechanicalKeyframe(TimeSpan.FromSeconds(2), [second])
            ]));

        Assert.Contains("same component poses", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sample_WhenTimeIsBetweenKeyframes_ShouldInterpolateComponentPose()
    {
        var partId = new RobotPartId("tool");
        var demonstration = CreateDemonstration(partId);

        var poses = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(1));

        Assert.Equal(new Vector3(50, 25, 10), poses.Single().TranslationMillimeters);
    }

    [Fact]
    public void Sample_WhenTimeExceedsDuration_ShouldReturnFinalPose()
    {
        var partId = new RobotPartId("tool");
        var demonstration = CreateDemonstration(partId);

        var poses = MechanicalDemonstrationSampler.Sample(demonstration, TimeSpan.FromSeconds(20));

        Assert.Equal(new Vector3(100, 50, 20), poses.Single().TranslationMillimeters);
    }

    [Fact]
    public void ResolveWorldTransforms_WhenPartsAreNested_ShouldComposeLocalTranslations()
    {
        var model = CreateModel();
        var poses = new[]
        {
            new RobotComponentPose(
                new RobotPartId("base"),
                new Vector3(10, 0, 0),
                Quaternion.Identity,
                Vector3.One),
            new RobotComponentPose(
                new RobotPartId("tool"),
                new Vector3(0, 20, 0),
                Quaternion.Identity,
                Vector3.One)
        };

        var transforms = RobotComponentPoseResolver.ResolveWorldTransforms(model, poses);

        Assert.Equal(new Vector3(10, 20, 0), transforms[new RobotPartId("tool")].Translation);
    }

    private static RobotVisualModelDefinition CreateModel()
    {
        var baseId = new RobotPartId("base");
        var toolId = new RobotPartId("tool");
        return new RobotVisualModelDefinition(
            "cartesian",
            "Cartesian Robot",
            baseId,
            [
                new RobotPartDefinition(
                    baseId,
                    "Base",
                    RobotPartKind.Base,
                    parentId: null,
                    "Supports the robot.",
                    "Remains fixed."),
                new RobotPartDefinition(
                    toolId,
                    "Tool",
                    RobotPartKind.Tool,
                    baseId,
                    "Interacts with the workpiece.",
                    "Follows the linear axes.")
            ]);
    }

    private static MechanicalDemonstrationDefinition CreateDemonstration(RobotPartId partId)
    {
        var start = RobotComponentPose.Identity(partId);
        var end = start with { TranslationMillimeters = new Vector3(100, 50, 20) };
        return new MechanicalDemonstrationDefinition(
            "axis-motion",
            "Axis motion",
            "Shows coordinated linear movement.",
            TimeSpan.FromSeconds(2),
            [
                new MechanicalKeyframe(TimeSpan.Zero, [start]),
                new MechanicalKeyframe(TimeSpan.FromSeconds(2), [end])
            ]);
    }
}
