using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class CartesianRobotPoseMapperTests
{
    [Fact]
    public void Map_ShouldCreateDidacticCartesianMechanismPose()
    {
        var mapper = new CartesianRobotPoseMapper();
        var visualState = new RobotVisualState(
            TimeSpan.FromSeconds(1),
            RobotState.Moving,
            new VisualVector3(120, 80, 40),
            CommandIndex: 2,
            CommandName: nameof(MoveToCommand),
            CommandSource: new RobotCommandSource(3, "MOVE X=120 Y=80 Z=40"));

        var pose = mapper.Map(visualState);

        Assert.Equal(new VisualVector3(0, 0, 0), pose.Base);
        Assert.Equal(new VisualVector3(120, 0, 0), pose.XAxisCarriage);
        Assert.Equal(new VisualVector3(120, 80, 0), pose.YAxisCarriage);
        Assert.Equal(new VisualVector3(120, 80, 40), pose.ZAxisCarriage);
        Assert.Equal(new VisualVector3(120, 80, 40), pose.ToolCenterPoint);
    }

    [Fact]
    public void Map_ShouldPreserveTimelineMetadata()
    {
        var source = new RobotCommandSource(3, "MOVE X=120 Y=80 Z=40");
        var mapper = new CartesianRobotPoseMapper();
        var visualState = new RobotVisualState(
            TimeSpan.FromSeconds(1),
            RobotState.Moving,
            new VisualVector3(120, 80, 40),
            CommandIndex: 2,
            CommandName: nameof(MoveToCommand),
            CommandSource: source);

        var pose = mapper.Map(visualState);

        Assert.Equal(TimeSpan.FromSeconds(1), pose.Time);
        Assert.Equal(RobotState.Moving, pose.State);
        Assert.Equal(2, pose.CommandIndex);
        Assert.Equal(nameof(MoveToCommand), pose.CommandName);
        Assert.Equal(source, pose.CommandSource);
    }

    [Fact]
    public void Map_WhenVisualStateIsNull_ShouldThrow()
    {
        var mapper = new CartesianRobotPoseMapper();

        Assert.Throws<ArgumentNullException>(() => mapper.Map(null!));
    }
}
