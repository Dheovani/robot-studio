using System.Numerics;

namespace RobotStudio.Visualization.Tests;

public sealed class RobotOverlaySceneTests
{
    [Fact]
    public void Constructor_ShouldSnapshotRendererIndependentPrimitives()
    {
        var source = new List<RobotOverlayPrimitive>
        {
            new RobotOverlayLine(
                RobotOverlayKind.CoordinateAxis,
                Vector3.Zero,
                Vector3.UnitX,
                0.1f,
                RobotOverlayAxis.X)
        };

        var scene = new RobotOverlayScene(source);
        source.Clear();

        var line = Assert.IsType<RobotOverlayLine>(Assert.Single(scene.Primitives));
        Assert.Equal(RobotOverlayAxis.X, line.Axis);
        Assert.Equal(Vector3.UnitX, line.End);
    }

    [Fact]
    public void Constructor_WhenLineHasNoLength_ShouldRejectPrimitive()
    {
        var primitive = new RobotOverlayLine(
            RobotOverlayKind.CoordinateAxis,
            Vector3.One,
            Vector3.One,
            0.1f,
            RobotOverlayAxis.X);

        var exception = Assert.Throws<ArgumentException>(() => new RobotOverlayScene([primitive]));

        Assert.Equal("primitive", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenPolylineHasTooFewPoints_ShouldRejectPrimitive()
    {
        var primitive = new RobotOverlayPolyline(
            RobotOverlayKind.Trajectory,
            [Vector3.Zero],
            1);

        Assert.Throws<ArgumentException>(() => new RobotOverlayScene([primitive]));
    }

    [Fact]
    public void Constructor_ShouldSnapshotPolylinePoints()
    {
        var points = new List<Vector3> { Vector3.Zero, Vector3.One };
        var scene = new RobotOverlayScene(
        [
            new RobotOverlayPolyline(RobotOverlayKind.Trajectory, points, 1)
        ]);

        points[1] = Vector3.UnitX;

        var polyline = Assert.IsType<RobotOverlayPolyline>(Assert.Single(scene.Primitives));
        Assert.Equal(Vector3.One, polyline.Points[1]);
    }

    [Fact]
    public void Kinds_ShouldReserveCollisionBoundsWithoutRendererDependency()
    {
        Assert.True(Enum.IsDefined(RobotOverlayKind.CollisionBounds));
        Assert.DoesNotContain(typeof(RobotOverlayScene).Assembly.GetReferencedAssemblies(), reference =>
            reference.Name?.Contains("Helix", StringComparison.OrdinalIgnoreCase) == true ||
            reference.Name?.Contains("Presentation", StringComparison.OrdinalIgnoreCase) == true);
    }
}
