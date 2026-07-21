using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class CartesianPlaybackSnapshotBuilderTests
{
    [Fact]
    public void Build_WhenSimulationSucceeds_ShouldCreateSnapshotWithBoundsFramesAndDuration()
    {
        var profile = CreateProfile();
        var result = CreateMoveSimulation(profile);
        var builder = new CartesianPlaybackSnapshotBuilder();

        var snapshot = builder.Build(profile, result, TimeSpan.FromSeconds(1));

        Assert.Equal(CartesianWorkspaceBounds.FromProfile(profile), snapshot.WorkspaceBounds);
        Assert.Equal(snapshot.WorkspaceBounds.Center, snapshot.Viewport.Target);
        Assert.Equal(TimeSpan.FromSeconds(2), snapshot.TotalDuration);
        Assert.True(snapshot.Succeeded);
        Assert.Null(snapshot.FailureMessage);
        Assert.Equal(3, snapshot.FrameCount);
        Assert.Equal(3, snapshot.PoseCount);
        Assert.Equal(3, snapshot.SceneFrameCount);
        Assert.Equal(TimeSpan.Zero, snapshot.Frames[0].Time);
        Assert.Equal(TimeSpan.FromSeconds(2), snapshot.Frames[^1].Time);
        Assert.Equal(snapshot.Frames[^1].Position, snapshot.Poses[^1].ToolCenterPoint);
        Assert.Equal(snapshot.Poses[^1].ToolCenterPoint, GetPrimitive(snapshot.SceneFrames[^1], "tool").Center);
    }

    [Fact]
    public void Build_WhenSimulationFails_ShouldPreserveFailureMessage()
    {
        var profile = CreateProfile();
        var result = CreateFailedSimulation(profile);
        var builder = new CartesianPlaybackSnapshotBuilder();

        var snapshot = builder.Build(profile, result, TimeSpan.FromMilliseconds(500));

        Assert.False(snapshot.Succeeded);
        Assert.NotNull(snapshot.FailureMessage);
        Assert.Contains("outside", snapshot.FailureMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_WhenProfileIsNull_ShouldThrow()
    {
        var result = CreateMoveSimulation(CreateProfile());
        var builder = new CartesianPlaybackSnapshotBuilder();

        Assert.Throws<ArgumentNullException>(() =>
            builder.Build(null!, result, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Build_WhenResultIsNull_ShouldThrow()
    {
        var builder = new CartesianPlaybackSnapshotBuilder();

        Assert.Throws<ArgumentNullException>(() =>
            builder.Build(CreateProfile(), null!, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Constructor_WhenPlaybackSamplerIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CartesianPlaybackSnapshotBuilder(null!));
    }

    [Fact]
    public void Constructor_WhenPoseMapperIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CartesianPlaybackSnapshotBuilder(new CartesianPlaybackSampler(), null!));
    }

    [Fact]
    public void Constructor_WhenSceneFrameMapperIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CartesianPlaybackSnapshotBuilder(
                new CartesianPlaybackSampler(),
                new CartesianRobotPoseMapper(),
                null!));
    }

    [Fact]
    public void Constructor_WhenViewportPlannerIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CartesianPlaybackSnapshotBuilder(
                new CartesianPlaybackSampler(),
                new CartesianRobotPoseMapper(),
                new CartesianSceneFrameMapper(),
                null!));
    }

    private static CartesianScenePrimitive GetPrimitive(
        CartesianSceneFrame sceneFrame,
        string id) =>
        sceneFrame.Primitives.Single(primitive => primitive.Id == id);

    private static SimulationResult CreateMoveSimulation(CartesianRobotProfile profile)
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            profile,
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var sequence = new RobotCommandSequence(
        [
            new MoveToCommand(
                new CartesianPosition(X: 100, Y: 0, Z: 0),
                requestedVelocityMillimetersPerSecond: 50)
        ]);

        return simulator.Execute(context, sequence);
    }

    private static SimulationResult CreateFailedSimulation(CartesianRobotProfile profile)
    {
        var simulator = new RobotSimulator();
        var context = SimulationContext.Create(
            profile,
            new CartesianPosition(X: 0, Y: 0, Z: 0));
        var sequence = new RobotCommandSequence(
        [
            new MoveToCommand(new CartesianPosition(X: 301, Y: 0, Z: 0))
        ]);

        return simulator.Execute(context, sequence);
    }

    private static CartesianRobotProfile CreateProfile() =>
        CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));
}
