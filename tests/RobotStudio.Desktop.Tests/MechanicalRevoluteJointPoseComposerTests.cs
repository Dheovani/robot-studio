using System.Numerics;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class MechanicalRevoluteJointPoseComposerTests
{
    [Fact]
    public void Compose_WhenPoseRotatesAroundDeclaredPivot_ShouldPreservePivotPosition()
    {
        var partId = new RobotPartId("joint");
        var pivot = new Vector3(325, 0, 0);
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 47 * MathF.PI / 180);
        var pose = new RobotComponentPose(partId, Vector3.Zero, rotation, Vector3.One);

        var result = MechanicalRevoluteJointPoseComposer.Compose(
            [pose],
            [new MechanicalRevoluteJointPivot(partId, pivot)]).Single();
        var transform = Matrix4x4.CreateFromQuaternion(result.Rotation) *
                        Matrix4x4.CreateTranslation(result.TranslationMillimeters);

        AssertVectorApproximately(pivot, Vector3.Transform(pivot, transform));
    }

    [Fact]
    public void Compose_WhenPartHasNoDeclaredPivot_ShouldPreservePose()
    {
        var pose = new RobotComponentPose(
            new RobotPartId("linear-axis"),
            new Vector3(10, 20, 30),
            Quaternion.Identity,
            Vector3.One);

        var result = MechanicalRevoluteJointPoseComposer.Compose([pose], []).Single();

        Assert.Equal(pose, result);
    }

    private static void AssertVectorApproximately(Vector3 expected, Vector3 actual)
    {
        Assert.InRange(actual.X, expected.X - 0.001f, expected.X + 0.001f);
        Assert.InRange(actual.Y, expected.Y - 0.001f, expected.Y + 0.001f);
        Assert.InRange(actual.Z, expected.Z - 0.001f, expected.Z + 0.001f);
    }
}
