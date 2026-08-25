using System.Windows.Media.Media3D;
using RobotStudio.Desktop.Rendering;

namespace RobotStudio.Desktop.Tests;

public sealed class OrbitCameraInteractionMathTests
{
    [Fact]
    public void PanTarget_WhenDraggedHorizontally_ShouldMoveTargetInCameraPlane()
    {
        var target = OrbitCameraInteractionMath.PanTarget(
            new Point3D(0, 0, 0),
            new Vector3D(0, 1, 0),
            new Vector3D(0, 0, 1),
            distance: 10,
            fieldOfViewDegrees: 45,
            viewportHeight: 500,
            deltaX: 100,
            deltaY: 0);

        Assert.True(target.X < 0);
        Assert.Equal(0, target.Y, 10);
        Assert.Equal(0, target.Z, 10);
    }

    [Fact]
    public void PanTarget_WhenViewportHasNoHeight_ShouldKeepTarget()
    {
        var original = new Point3D(1, 2, 3);

        var target = OrbitCameraInteractionMath.PanTarget(
            original,
            new Vector3D(0, 1, 0),
            new Vector3D(0, 0, 1),
            distance: 10,
            fieldOfViewDegrees: 45,
            viewportHeight: 0,
            deltaX: 100,
            deltaY: 100);

        Assert.Equal(original, target);
    }

    [Fact]
    public void FitDistance_ShouldIncludeRequestedMargin()
    {
        var withoutMargin = OrbitCameraInteractionMath.FitDistance(5, 45, margin: 1);
        var withMargin = OrbitCameraInteractionMath.FitDistance(5, 45, margin: 1.2);

        Assert.Equal(withoutMargin * 1.2, withMargin, 10);
    }

    [Theory]
    [InlineData(0, 45, 1.15)]
    [InlineData(5, 0, 1.15)]
    [InlineData(5, 180, 1.15)]
    [InlineData(5, 45, 0.9)]
    public void FitDistance_WhenInputIsInvalid_ShouldThrow(
        double radius,
        double fieldOfView,
        double margin)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OrbitCameraInteractionMath.FitDistance(radius, fieldOfView, margin));
    }
}
