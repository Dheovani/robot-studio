using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Media.Media3D;
using RobotStudio.Desktop.Rendering;
using RobotStudio.Desktop.Rendering.SceneComposers;
using RobotStudio.Domain;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop.Tests;

public sealed class SchematicSceneComposerTests
{
    [Fact]
    public void CartesianCompose_WhenOverlaysAreDisabled_ShouldOnlyRenderRequestedLayers()
    {
        var snapshot = CreateCartesianSnapshot();
        var scene = CartesianSchematicSceneComposer.Compose(
            snapshot,
            frameIndex: 0,
            CreateCamera(),
            CreateOptions(enabled: false));

        Assert.Empty(scene.Models);
        Assert.Empty(scene.Overlays);
    }

    [Fact]
    public void CartesianAndXYPlotterCompose_WhenLayersAreEnabled_ShouldProduceEquivalentScenes()
    {
        var snapshot = CreateCartesianSnapshot();
        var options = CreateOptions(enabled: true);

        var result = RunInSta(() =>
        {
            var cartesian = CartesianSchematicSceneComposer.Compose(snapshot, 0, CreateCamera(), options);
            var plotter = XYPlotterSchematicSceneComposer.Compose(snapshot, 0, CreateCamera(), options);
            return (
                CartesianModelCount: cartesian.Models.Count,
                PlotterModelCount: plotter.Models.Count,
                CartesianOverlayCount: cartesian.Overlays.Count,
                PlotterOverlayCount: plotter.Overlays.Count);
        });

        Assert.Equal(result.CartesianModelCount, result.PlotterModelCount);
        Assert.Equal(3, result.CartesianOverlayCount);
        Assert.Equal(3, result.PlotterOverlayCount);
        Assert.True(result.CartesianModelCount > 0);
    }

    [Fact]
    public void DifferentialDriveCompose_WhenFrameAdvances_ShouldAddOnePathSegment()
    {
        var snapshot = CreateDifferentialDriveSnapshot();

        var first = DifferentialDriveSchematicSceneComposer.Compose(snapshot, 0, new Size(400, 300), 1);
        var second = DifferentialDriveSchematicSceneComposer.Compose(snapshot, 1, new Size(400, 300), 1);

        Assert.Equal(first.Primitives.Count + 1, second.Primitives.Count);
        Assert.Contains(second.Primitives, primitive =>
            primitive is CanvasLine2D { Thickness: 3 });
    }

    [Fact]
    public void DifferentialDriveCompose_ShouldCenterMappedRobotInSquareWorkspace()
    {
        var snapshot = CreateDifferentialDriveSnapshot(
            new DifferentialDrivePose(50, 50, 0),
            new DifferentialDrivePose(50, 50, 0));

        var scene = DifferentialDriveSchematicSceneComposer.Compose(
            snapshot,
            frameIndex: 0,
            new Size(200, 200),
            zoomMultiplier: 1);

        var body = Assert.Single(scene.Primitives.OfType<CanvasEllipse2D>(), ellipse =>
            ellipse.Bounds.Width == 36);
        Assert.Equal(100, body.Bounds.X + (body.Bounds.Width / 2), precision: 6);
        Assert.Equal(100, body.Bounds.Y + (body.Bounds.Height / 2), precision: 6);
    }

    private static CartesianPlaybackSnapshot CreateCartesianSnapshot()
    {
        var profile = CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 200, 100, 400),
            new Axis(AxisId.Y, 0, 150, 100, 400),
            new Axis(AxisId.Z, 0, 100, 80, 300));
        var result = new RobotSimulator().Execute(
            SimulationContext.Create(profile, new CartesianPosition(0, 0, 0)),
            new RobotCommandSequence(
            [
                new MoveToCommand(new CartesianPosition(50, 40, 20), 60)
            ]));

        return new CartesianPlaybackSnapshotBuilder().Build(
            profile,
            result,
            TimeSpan.FromMilliseconds(100));
    }

    private static DifferentialDrivePlaybackSnapshot CreateDifferentialDriveSnapshot() =>
        CreateDifferentialDriveSnapshot(
            new DifferentialDrivePose(10, 10, 0),
            new DifferentialDrivePose(80, 60, 45));

    private static DifferentialDrivePlaybackSnapshot CreateDifferentialDriveSnapshot(
        DifferentialDrivePose start,
        DifferentialDrivePose end)
    {
        var profile = new DifferentialDriveProfile(
            0, 100,
            0, 100,
            wheelBaseMillimeters: 40,
            wheelRadiusMillimeters: 10,
            collisionRadiusMillimeters: 12,
            maximumLinearVelocityMillimetersPerSecond: 100,
            maximumAngularVelocityDegreesPerSecond: 180,
            maximumLinearAccelerationMillimetersPerSecondSquared: 200,
            maximumAngularAccelerationDegreesPerSecondSquared: 360);

        return new DifferentialDrivePlaybackSnapshot(
            profile,
            [
                new(TimeSpan.Zero, RobotState.Idle, start, DifferentialDriveOdometry.Zero, null, null, null),
                new(TimeSpan.FromSeconds(1), RobotState.Moving, end, DifferentialDriveOdometry.Zero, 0, "DRIVE", null)
            ],
            TimeSpan.FromSeconds(1),
            Succeeded: true,
            FailureMessage: null);
    }

    private static PerspectiveCamera CreateCamera() =>
        new(
            new Point3D(400, 400, 300),
            new Vector3D(-400, -400, -300),
            new Vector3D(0, 0, 1),
            fieldOfView: 45);

    private static CartesianSchematicSceneOptions CreateOptions(bool enabled) =>
        new(enabled, enabled, enabled, enabled, enabled, enabled, enabled, enabled, enabled);

    private static T RunInSta<T>(Func<T> action)
    {
        T? result = default;
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        return result!;
    }
}
