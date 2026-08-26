using System.Windows.Media.Media3D;
using RobotStudio.Desktop.Rendering;
using RobotStudio.Desktop.Rendering.SceneComposers;
using RobotStudio.Desktop.Robots;
using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Parallel;
using RobotStudio.Simulation;

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

    private static void AssertSceneStructure(SchematicViewportScene scene)
    {
        Assert.IsType<PerspectiveCamera>(scene.Camera);
        Assert.Equal(3, scene.Models.Count);
        Assert.All(scene.Models, model => Assert.IsType<Model3DGroup>(model));
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
