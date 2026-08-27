using System.Numerics;
using System.Windows.Media.Media3D;
using RobotStudio.Desktop.Rendering;
using RobotStudio.Desktop.Rendering.SceneComposers;
using RobotStudio.Desktop.Robots;
using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Parallel;
using RobotStudio.Simulation;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class FamilySchematicSceneComposerTests
{
    [Fact]
    public void ScaraCompose_WhenFrameAdvances_ShouldGrowPathWithoutChangingLayerStructure()
    {
        var profile = CreateScaraProfile();
        var snapshot = new ScaraPlaybackSnapshot(
            profile,
            [
                new(TimeSpan.Zero, RobotState.Idle, new(0, 0), new(300, 0), null, null, null),
                new(TimeSpan.FromSeconds(1), RobotState.Moving, new(20, 30), new(240, 120), 0, "SCARA", null)
            ],
            TimeSpan.FromSeconds(1),
            Succeeded: true,
            FailureMessage: null);

        var firstScene = ScaraSchematicSceneComposer.Compose(snapshot, 0, 35, 28, 1);
        var secondScene = ScaraSchematicSceneComposer.Compose(snapshot, 1, 35, 28, 1);

        AssertSceneStructure(firstScene);
        AssertSceneStructure(secondScene);
        Assert.Empty(Assert.IsType<Model3DGroup>(firstScene.Models[1]).Children);
        Assert.Single(Assert.IsType<Model3DGroup>(secondScene.Models[1]).Children);
    }

    [Fact]
    public void SimpleArmCompose_ShouldCreateWorkspacePathAndRobotLayers()
    {
        var profile = CreateSimpleArmProfile();
        var snapshot = new SimpleArmPlaybackSnapshot(
            profile,
            [new(TimeSpan.Zero, RobotState.Idle, new(0, 0, 0), new(270, 0, 0))],
            TimeSpan.Zero,
            Succeeded: true,
            FailureMessage: null);

        AssertSceneStructure(SimpleArmSchematicSceneComposer.Compose(snapshot, 0, 35, 28, 1));
    }

    [Fact]
    public void DeltaCompose_ShouldCreateWorkspacePathAndRobotLayers()
    {
        var snapshot = new DeltaPlaybackSnapshot(
            DeltaTeachingProfile.Create(),
            [new(TimeSpan.Zero, RobotState.Idle, new(0, 0, 0), new(0, 0, -60), null, null, null)],
            TimeSpan.Zero,
            Succeeded: true,
            FailureMessage: null);

        AssertSceneStructure(DeltaSchematicSceneComposer.Compose(snapshot, 0, 35, 28, 1));
    }

    [Fact]
    public void DroneCompose_ShouldCreateWorkspacePathAndRobotLayers()
    {
        var profile = new DroneProfile(
            0, 500,
            0, 350,
            0, 240,
            collisionRadiusMillimeters: 24,
            maximumLinearVelocityMillimetersPerSecond: 180,
            maximumYawVelocityDegreesPerSecond: 120,
            maximumLinearAccelerationMillimetersPerSecondSquared: 360,
            maximumYawAccelerationDegreesPerSecondSquared: 240,
            maximumTiltDegrees: 45,
            maximumAttitudeVelocityDegreesPerSecond: 180,
            maximumAttitudeAccelerationDegreesPerSecondSquared: 360);
        var snapshot = new DronePlaybackSnapshot(
            profile,
            [new(TimeSpan.Zero, RobotState.Idle, new DronePose(0, 0, 0, 0), null, null, null)],
            TimeSpan.Zero,
            Succeeded: true,
            FailureMessage: null);

        AssertSceneStructure(DroneSchematicSceneComposer.Compose(snapshot, 0, 35, 28, 1));
    }

    [Fact]
    public void IndustrialArmCompose_ShouldCreateWorkspacePathAndRobotLayersWithCustomLighting()
    {
        var profile = IndustrialArmTeachingProfile.Create();
        var snapshot = new IndustrialArmPlaybackSnapshot(
            profile,
            [new(
                TimeSpan.Zero,
                RobotState.Idle,
                IndustrialArmJointPosition.Home,
                new IndustrialArmToolPose(400, 0, 110, 0, 0, 0))],
            TimeSpan.Zero,
            Succeeded: true,
            FailureMessage: null);

        var scene = IndustrialArmSchematicSceneComposer.Compose(snapshot, 0, 35, 28, 1);

        AssertSceneStructure(scene);
        Assert.NotNull(scene.AmbientColor);
    }

    [Fact]
    public void Compose_WhenFrameIndexIsInvalid_ShouldRejectScene()
    {
        var profile = CreateScaraProfile();
        var snapshot = new ScaraPlaybackSnapshot(
            profile,
            [new(TimeSpan.Zero, RobotState.Idle, new(0, 0), new(300, 0), null, null, null)],
            TimeSpan.Zero,
            Succeeded: true,
            FailureMessage: null);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ScaraSchematicSceneComposer.Compose(snapshot, 1, 35, 28, 1));
    }

    [Fact]
    public void PlanarOverlayCompose_ShouldExposeWorkspaceCoordinatesLabelsLimitsAndTrajectory()
    {
        var scene = SchematicEducationalOverlayComposer.ComposePlanar(
            reach: 200,
            gridSpacing: 50,
            floorZ: -8,
            gridThickness: 1.8f,
            boundaryThickness: 3,
            [Vector3.Zero, new Vector3(50, 40, 20)],
            trajectoryThickness: 5);

        AssertEducationalLayers(scene, expectedAxisCount: 2, expectedLimitCount: 4);
        Assert.IsType<RobotOverlayPolyline>(Assert.Single(
            scene.Primitives,
            primitive => primitive.Kind == RobotOverlayKind.WorkspaceBoundary));
    }

    [Fact]
    public void BoxOverlayCompose_ShouldExposeVolumetricWorkspaceAndThreeAxisCoordinates()
    {
        var scene = SchematicEducationalOverlayComposer.ComposeBox(
            Vector3.Zero,
            new Vector3(500, 350, 240),
            gridSpacing: 50,
            gridThickness: 1.8f,
            boundaryThickness: 4,
            [Vector3.Zero, new Vector3(100, 80, 60)],
            trajectoryThickness: 4);

        AssertEducationalLayers(scene, expectedAxisCount: 3, expectedLimitCount: 6);
        Assert.IsType<RobotOverlayBox>(Assert.Single(
            scene.Primitives,
            primitive => primitive.Kind == RobotOverlayKind.WorkspaceBoundary));
    }

    [Fact]
    public void RectangularPlanarOverlayCompose_ShouldExposeMobileWorkspaceSemantics()
    {
        var scene = SchematicEducationalOverlayComposer.ComposeRectangularPlanar(
            Vector2.Zero,
            new Vector2(500, 350),
            floorZ: 0,
            gridSpacing: 50,
            gridThickness: 1,
            boundaryThickness: 2,
            [Vector3.Zero, new Vector3(100, 80, 0)],
            trajectoryThickness: 3);

        AssertEducationalLayers(scene, expectedAxisCount: 2, expectedLimitCount: 4);
        var boundary = Assert.IsType<RobotOverlayPolyline>(Assert.Single(
            scene.Primitives,
            primitive => primitive.Kind == RobotOverlayKind.WorkspaceBoundary));
        Assert.Equal(boundary.Points[0], boundary.Points[^1]);
    }

    private static void AssertSceneStructure(SchematicViewportScene scene)
    {
        Assert.IsType<PerspectiveCamera>(scene.Camera);
        Assert.Equal(3, scene.Models.Count);
        Assert.All(scene.Models, model => Assert.IsType<Model3DGroup>(model));
    }

    private static void AssertEducationalLayers(
        RobotOverlayScene scene,
        int expectedAxisCount,
        int expectedLimitCount)
    {
        Assert.Contains(scene.Primitives, primitive => primitive.Kind == RobotOverlayKind.CoordinateGrid);
        Assert.Single(scene.Primitives, primitive => primitive.Kind == RobotOverlayKind.CoordinateOrigin);
        Assert.Equal(expectedAxisCount, scene.Primitives.Count(primitive => primitive.Kind == RobotOverlayKind.CoordinateAxis));
        Assert.Equal(expectedAxisCount, scene.Primitives.Count(primitive => primitive.Kind == RobotOverlayKind.AxisLabel));
        Assert.Equal(expectedLimitCount, scene.Primitives.Count(primitive => primitive.Kind == RobotOverlayKind.PhysicalLimit));
        Assert.Single(scene.Primitives, primitive => primitive.Kind == RobotOverlayKind.Trajectory);
    }

    private static ScaraRobotProfile CreateScaraProfile() =>
        new(
            180,
            120,
            12,
            new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120, 240),
            new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100, 200));

    private static SimpleArmRobotProfile CreateSimpleArmProfile() =>
        new(
            120,
            90,
            60,
            10,
            new SimpleArmJoint(SimpleArmJointId.Base, -180, 180, 100, 200),
            new SimpleArmJoint(SimpleArmJointId.Shoulder, -120, 120, 90, 180),
            new SimpleArmJoint(SimpleArmJointId.Elbow, -150, 150, 80, 160));
}
