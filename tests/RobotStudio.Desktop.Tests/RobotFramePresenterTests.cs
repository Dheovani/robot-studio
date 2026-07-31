using RobotStudio.Desktop.Viewers;
using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Mobile;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop.Tests;

public sealed class RobotFramePresenterTests
{
    [Fact]
    public void Create_WhenDifferentialDriveFrame_ShouldFormatMobileState()
    {
        var frame = new DifferentialDrivePlaybackFrame(
            TimeSpan.FromMilliseconds(750),
            RobotState.Moving,
            new DifferentialDrivePose(X: 120, Y: 80, HeadingDegrees: 45),
            CommandIndex: 0,
            CommandName: "DifferentialDriveMoveCommand",
            CommandSource: null);

        var status = RobotFramePresenter.Create(frame, frameIndex: 1, frameCount: 4, TimeSpan.FromSeconds(2));

        Assert.Equal("Moving", status.State);
        Assert.Equal("X=120, Y=80, H=45 deg", status.PrimaryPose);
        Assert.Equal("DifferentialDriveMoveCommand", status.Command);
        Assert.Equal("0.75 / 2 s", status.Time);
        Assert.Equal("2 / 4", status.Frames);
        Assert.Equal("Frame 2/4 | t=0.75s | Moving", status.Footer);
    }

    [Fact]
    public void Create_WhenScaraFrame_ShouldExplainForwardKinematics()
    {
        var frame = new ScaraPlaybackFrame(
            TimeSpan.FromSeconds(1),
            RobotState.Moving,
            new ScaraJointPosition(ShoulderDegrees: 45, ElbowDegrees: 30),
            new ScaraToolPose(X: 210, Y: 90),
            CommandIndex: 1,
            CommandName: "ScaraMoveJointsCommand",
            CommandSource: null);

        var status = RobotFramePresenter.Create(frame, frameIndex: 0, frameCount: 3, TimeSpan.FromSeconds(2));

        Assert.Equal("S=45, E=30 deg", status.PrimaryPose);
        Assert.Equal("X=210, Y=90 mm", RobotFramePresenter.FormatScaraToolPose(frame));
        Assert.Contains("forward kinematics", status.MovementExplanation);
        Assert.Contains("X=210, Y=90 mm", status.MovementExplanation);
    }

    [Fact]
    public void Create_WhenSimpleArmFrame_ShouldExplainJointComposition()
    {
        var frame = new SimpleArmPlaybackFrame(
            TimeSpan.FromSeconds(1.25),
            RobotState.Moving,
            new SimpleArmJointPosition(BaseDegrees: 20, ShoulderDegrees: 35, ElbowDegrees: -15),
            new SimpleArmToolPose(X: 160, Y: 110, OrientationDegrees: 40),
            CommandIndex: 1,
            CommandName: "SimpleArmMoveJointsCommand",
            CommandSource: null);

        var status = RobotFramePresenter.Create(frame, frameIndex: 2, frameCount: 5, TimeSpan.FromSeconds(3));

        Assert.Equal("B=20, S=35, E=-15 deg", status.PrimaryPose);
        Assert.Equal("X=160, Y=110, O=40 deg", RobotFramePresenter.FormatSimpleArmToolPose(frame));
        Assert.Contains("base angle rotates", status.MovementExplanation);
        Assert.Contains("O=40 deg", status.MovementExplanation);
    }
}
