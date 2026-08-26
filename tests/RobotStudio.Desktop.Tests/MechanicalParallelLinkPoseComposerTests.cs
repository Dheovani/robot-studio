using System.Numerics;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class MechanicalParallelLinkPoseComposerTests
{
    [Fact]
    public void Compose_WhenEndpointsMoveIndependently_ShouldKeepLinkAttachedAtBothEnds()
    {
        var model = CreateModel();
        var linkId = new RobotPartId("link");
        var authoredStart = new Vector3(0, 0, 10);
        var authoredEnd = new Vector3(20, 0, 0);
        var poses = new[]
        {
            new RobotComponentPose(new RobotPartId("carriage"), new Vector3(0, 0, -4), Quaternion.Identity, Vector3.One),
            new RobotComponentPose(new RobotPartId("platform"), new Vector3(3, 7, 2), Quaternion.Identity, Vector3.One)
        };

        var result = MechanicalParallelLinkPoseComposer.Compose(
            model,
            poses,
            [new MechanicalParallelLinkConstraint(
                linkId,
                new RobotPartId("carriage"),
                new RobotPartId("platform"),
                authoredStart,
                authoredEnd)]);
        var transforms = RobotComponentPoseResolver.ResolveWorldTransforms(model, result);

        AssertVectorApproximately(
            Vector3.Transform(authoredStart, transforms[new RobotPartId("carriage")]),
            Vector3.Transform(authoredStart, transforms[linkId]));
        AssertVectorApproximately(
            Vector3.Transform(authoredEnd, transforms[new RobotPartId("platform")]),
            Vector3.Transform(authoredEnd, transforms[linkId]));
    }

    [Fact]
    public void Compose_WhenNoConstraintsExist_ShouldPreservePoses()
    {
        var model = CreateModel();
        var pose = new RobotComponentPose(
            new RobotPartId("platform"),
            new Vector3(1, 2, 3),
            Quaternion.Identity,
            Vector3.One);

        var result = MechanicalParallelLinkPoseComposer.Compose(model, [pose], []);

        Assert.Equal([pose], result);
    }

    private static RobotVisualModelDefinition CreateModel() =>
        new(
            "parallel-test",
            "Parallel test",
            new RobotPartId("base"),
            [
                Part("base", null),
                Part("carriage", "base"),
                Part("platform", "base"),
                Part("link", "base")
            ]);

    private static RobotPartDefinition Part(string id, string? parentId) =>
        new(
            new RobotPartId(id),
            id,
            RobotPartKind.Other,
            parentId is null ? null : new RobotPartId(parentId),
            "Test part.",
            "Test movement.");

    private static void AssertVectorApproximately(Vector3 expected, Vector3 actual)
    {
        Assert.InRange(actual.X, expected.X - 0.001f, expected.X + 0.001f);
        Assert.InRange(actual.Y, expected.Y - 0.001f, expected.Y + 0.001f);
        Assert.InRange(actual.Z, expected.Z - 0.001f, expected.Z + 0.001f);
    }
}
