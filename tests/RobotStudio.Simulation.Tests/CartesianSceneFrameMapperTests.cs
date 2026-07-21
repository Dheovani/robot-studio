using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class CartesianSceneFrameMapperTests
{
    [Fact]
    public void Map_ShouldCreateRenderableScenePrimitives()
    {
        var mapper = new CartesianSceneFrameMapper();
        var bounds = CreateWorkspaceBounds();
        var pose = CreatePose();

        var sceneFrame = mapper.Map(bounds, pose);

        Assert.Equal(8, sceneFrame.PrimitiveCount);
        Assert.Contains(sceneFrame.Primitives, primitive => primitive.Id == "workspace" && primitive.Kind == CartesianScenePrimitiveKind.Workspace);
        Assert.Contains(sceneFrame.Primitives, primitive => primitive.Id == "x-rail" && primitive.Kind == CartesianScenePrimitiveKind.Rail);
        Assert.Contains(sceneFrame.Primitives, primitive => primitive.Id == "y-rail" && primitive.Kind == CartesianScenePrimitiveKind.Rail);
        Assert.Contains(sceneFrame.Primitives, primitive => primitive.Id == "z-rail" && primitive.Kind == CartesianScenePrimitiveKind.Rail);
        Assert.Contains(sceneFrame.Primitives, primitive => primitive.Id == "tool" && primitive.Kind == CartesianScenePrimitiveKind.Tool);
    }

    [Fact]
    public void Map_ShouldPositionMovingPrimitivesFromPose()
    {
        var mapper = new CartesianSceneFrameMapper();
        var bounds = CreateWorkspaceBounds();
        var pose = CreatePose();

        var sceneFrame = mapper.Map(bounds, pose);

        Assert.Equal(new VisualVector3(120, 0, 0), GetPrimitive(sceneFrame, "x-carriage").Center);
        Assert.Equal(new VisualVector3(120, 80, 0), GetPrimitive(sceneFrame, "y-carriage").Center);
        Assert.Equal(new VisualVector3(120, 80, 40), GetPrimitive(sceneFrame, "z-carriage").Center);
        Assert.Equal(new VisualVector3(120, 80, 40), GetPrimitive(sceneFrame, "tool").Center);
    }

    [Fact]
    public void Map_ShouldUseWorkspaceBoundsForFixedPrimitives()
    {
        var mapper = new CartesianSceneFrameMapper();
        var bounds = CreateWorkspaceBounds();
        var pose = CreatePose();

        var sceneFrame = mapper.Map(bounds, pose);

        Assert.Equal(bounds.Center, GetPrimitive(sceneFrame, "workspace").Center);
        Assert.Equal(bounds.Size, GetPrimitive(sceneFrame, "workspace").Size);
        Assert.Equal(new VisualVector3(150, 0, 0), GetPrimitive(sceneFrame, "x-rail").Center);
        Assert.Equal(new VisualVector3(120, 100, 0), GetPrimitive(sceneFrame, "y-rail").Center);
        Assert.Equal(new VisualVector3(120, 80, 75), GetPrimitive(sceneFrame, "z-rail").Center);
    }

    [Fact]
    public void Map_ShouldPreservePoseMetadata()
    {
        var mapper = new CartesianSceneFrameMapper();
        var bounds = CreateWorkspaceBounds();
        var pose = CreatePose();

        var sceneFrame = mapper.Map(bounds, pose);

        Assert.Equal(pose.Time, sceneFrame.Time);
        Assert.Equal(pose.State, sceneFrame.State);
        Assert.Equal(pose.CommandIndex, sceneFrame.CommandIndex);
        Assert.Equal(pose.CommandName, sceneFrame.CommandName);
        Assert.Equal(pose.CommandSource, sceneFrame.CommandSource);
    }

    [Fact]
    public void Map_WhenWorkspaceBoundsIsNull_ShouldThrow()
    {
        var mapper = new CartesianSceneFrameMapper();

        Assert.Throws<ArgumentNullException>(() =>
            mapper.Map(null!, CreatePose()));
    }

    [Fact]
    public void Map_WhenPoseIsNull_ShouldThrow()
    {
        var mapper = new CartesianSceneFrameMapper();

        Assert.Throws<ArgumentNullException>(() =>
            mapper.Map(CreateWorkspaceBounds(), null!));
    }

    private static CartesianScenePrimitive GetPrimitive(
        CartesianSceneFrame sceneFrame,
        string id) =>
        sceneFrame.Primitives.Single(primitive => primitive.Id == id);

    private static CartesianWorkspaceBounds CreateWorkspaceBounds() =>
        new(
            new VisualVector3(0, 0, 0),
            new VisualVector3(300, 200, 150));

    private static CartesianRobotPose CreatePose() =>
        new(
            TimeSpan.FromSeconds(1),
            RobotState.Moving,
            Base: new VisualVector3(0, 0, 0),
            XAxisCarriage: new VisualVector3(120, 0, 0),
            YAxisCarriage: new VisualVector3(120, 80, 0),
            ZAxisCarriage: new VisualVector3(120, 80, 40),
            ToolCenterPoint: new VisualVector3(120, 80, 40),
            CommandIndex: 2,
            CommandName: nameof(MoveToCommand),
            CommandSource: new RobotCommandSource(3, "MOVE X=120 Y=80 Z=40"));
}
