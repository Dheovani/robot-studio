using RobotStudio.Desktop.Viewers;
using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Domain.Parallel;
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

    [Fact]
    public void Create_WhenDeltaFrame_ShouldExplainCoupledActuatorMotion()
    {
        var frame = new DeltaPlaybackFrame(
            TimeSpan.FromSeconds(1),
            RobotState.Moving,
            new DeltaActuatorPosition(AMillimeters: 30, BMillimeters: 60, CMillimeters: 90),
            new DeltaToolPose(XMillimeters: -17.321, YMillimeters: -45, ZMillimeters: -60),
            CommandIndex: 0,
            CommandName: nameof(DeltaMoveActuatorsCommand),
            CommandSource: null);

        var status = RobotFramePresenter.Create(frame, frameIndex: 1, frameCount: 4, TimeSpan.FromSeconds(3));

        Assert.Equal("A=30, B=60, C=90 mm", status.PrimaryPose);
        Assert.Equal("X=-17.321, Y=-45, Z=-60 mm", RobotFramePresenter.FormatDeltaToolPose(frame));
        Assert.Contains("coupled actuator-space motion", status.MovementExplanation);
        Assert.Contains("actuator average changes Z", status.MovementExplanation);
    }

    [Fact]
    public void Create_WhenDroneFrame_ShouldExplainCoordinatedFlightMotion()
    {
        var frame = new DronePlaybackFrame(
            TimeSpan.FromSeconds(1.5),
            RobotState.Moving,
            new DronePose(
                XMillimeters: 120,
                YMillimeters: 80,
                ZMillimeters: 40,
                YawDegrees: 90),
            CommandIndex: 0,
            CommandName: nameof(DroneMoveCommand),
            CommandSource: null);

        var status = RobotFramePresenter.Create(frame, frameIndex: 2, frameCount: 5, TimeSpan.FromSeconds(4));

        Assert.Equal("X=120, Y=80, Z=40 mm", status.PrimaryPose);
        Assert.Equal("Yaw=90 deg", RobotFramePresenter.FormatDroneYaw(frame));
        Assert.Contains("coordinated 3D flight motion", status.MovementExplanation);
        Assert.Contains("without simulating thrust", status.MovementExplanation);
    }
}
