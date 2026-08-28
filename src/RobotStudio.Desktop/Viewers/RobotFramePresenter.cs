using System.Globalization;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop.Viewers;

public static class RobotFramePresenter
{
    public static RobotFrameStatus Create(
        DifferentialDrivePlaybackFrame frame,
        int frameIndex,
        int frameCount,
        TimeSpan totalDuration)
    {
        var frameNumber = NormalizeFrameNumber(frameIndex, frameCount);
        return new RobotFrameStatus(
            State: frame.State.ToString(),
            PrimaryPose: $"X={FormatNumber(frame.Pose.X)}, Y={FormatNumber(frame.Pose.Y)}, H={FormatNumber(frame.Pose.HeadingDegrees)} deg",
            Command: frame.CommandName ?? "simulation",
            Time: FormatTime(frame.Time, totalDuration),
            Frames: $"{frameNumber} / {frameCount}",
            Footer: FormatFooter(frameNumber, frameCount, frame.Time),
            MovementExplanation:
                $"Differential-drive odometry estimates motion from wheel travel. " +
                $"Left wheel: {FormatNumber(frame.Odometry.LeftWheelTravelMillimeters)} mm " +
                $"({FormatNumber(frame.Odometry.LeftWheelRotationDegrees)} deg). " +
                $"Right wheel: {FormatNumber(frame.Odometry.RightWheelTravelMillimeters)} mm " +
                $"({FormatNumber(frame.Odometry.RightWheelRotationDegrees)} deg).");
    }

    public static RobotFrameStatus Create(
        ScaraPlaybackFrame frame,
        int frameIndex,
        int frameCount,
        TimeSpan totalDuration)
    {
        var frameNumber = NormalizeFrameNumber(frameIndex, frameCount);
        return new RobotFrameStatus(
            State: frame.State.ToString(),
            PrimaryPose: $"S={FormatNumber(frame.Joints.ShoulderDegrees)}, E={FormatNumber(frame.Joints.ElbowDegrees)} deg",
            Command: frame.CommandName ?? "simulation",
            Time: FormatTime(frame.Time, totalDuration),
            Frames: $"{frameNumber} / {frameCount}",
            Footer: FormatFooter(frameNumber, frameCount, frame.Time),
            MovementExplanation:
                $"{frame.CommandName ?? "simulation"} is represented as joint-space motion. " +
                "The shoulder and elbow angles define the current planar arm shape, " +
                "and forward kinematics maps those angles to the tool position " +
                $"X={FormatNumber(frame.ToolPose.X)}, Y={FormatNumber(frame.ToolPose.Y)} mm.");
    }

    public static RobotFrameStatus Create(
        SimpleArmPlaybackFrame frame,
        int frameIndex,
        int frameCount,
        TimeSpan totalDuration)
    {
        var frameNumber = NormalizeFrameNumber(frameIndex, frameCount);
        return new RobotFrameStatus(
            State: frame.State.ToString(),
            PrimaryPose: $"B={FormatNumber(frame.Joints.BaseDegrees)}, S={FormatNumber(frame.Joints.ShoulderDegrees)}, E={FormatNumber(frame.Joints.ElbowDegrees)} deg",
            Command: frame.CommandName ?? "simulation",
            Time: FormatTime(frame.Time, totalDuration),
            Frames: $"{frameNumber} / {frameCount}",
            Footer: FormatFooter(frameNumber, frameCount, frame.Time),
            MovementExplanation:
                $"{frame.CommandName ?? "simulation"} is represented as joint-space motion. " +
                "The base angle rotates the arm on the floor plane, while shoulder and elbow angles compose the links. " +
                "Forward kinematics maps those joint angles to the tool pose " +
                $"X={FormatNumber(frame.ToolPose.X)}, Y={FormatNumber(frame.ToolPose.Y)}, O={FormatNumber(frame.ToolPose.OrientationDegrees)} deg.");
    }

    public static RobotFrameStatus Create(
        DeltaPlaybackFrame frame,
        int frameIndex,
        int frameCount,
        TimeSpan totalDuration)
    {
        var frameNumber = NormalizeFrameNumber(frameIndex, frameCount);
        return new RobotFrameStatus(
            State: frame.State.ToString(),
            PrimaryPose: $"A={FormatNumber(frame.Actuators.AMillimeters)}, B={FormatNumber(frame.Actuators.BMillimeters)}, C={FormatNumber(frame.Actuators.CMillimeters)} mm",
            Command: frame.CommandName ?? "simulation",
            Time: FormatTime(frame.Time, totalDuration),
            Frames: $"{frameNumber} / {frameCount}",
            Footer: FormatFooter(frameNumber, frameCount, frame.Time),
            MovementExplanation:
                $"{frame.CommandName ?? "simulation"} is represented as coupled actuator-space motion. " +
                "The A, B, and C actuator heights move together through a parallel mechanism. " +
                "In this didactic model, actuator differences shift the tool in X/Y while the actuator average changes Z.");
    }

    public static RobotFrameStatus Create(
        DronePlaybackFrame frame,
        int frameIndex,
        int frameCount,
        TimeSpan totalDuration)
    {
        var frameNumber = NormalizeFrameNumber(frameIndex, frameCount);
        return new RobotFrameStatus(
            State: frame.State.ToString(),
            PrimaryPose: $"X={FormatNumber(frame.Pose.XMillimeters)}, Y={FormatNumber(frame.Pose.YMillimeters)}, Z={FormatNumber(frame.Pose.ZMillimeters)} mm",
            Command: frame.CommandName ?? "simulation",
            Time: FormatTime(frame.Time, totalDuration),
            Frames: $"{frameNumber} / {frameCount}",
            Footer: FormatFooter(frameNumber, frameCount, frame.Time),
            MovementExplanation:
                $"{frame.CommandName ?? "simulation"} is represented as coordinated 3D flight motion. " +
                "The drone pose combines X/Y/Z position with roll, pitch, and yaw attitude. " +
                "This didactic model coordinates translation and orientation without simulating thrust or real aerodynamics.");
    }

    public static RobotFrameStatus Create(
        IndustrialArmPlaybackFrame frame,
        int frameIndex,
        int frameCount,
        TimeSpan totalDuration)
    {
        var frameNumber = NormalizeFrameNumber(frameIndex, frameCount);
        return new RobotFrameStatus(
            State: frame.State.ToString(),
            PrimaryPose:
                $"J1={FormatNumber(frame.Joints.J1Degrees)}, J2={FormatNumber(frame.Joints.J2Degrees)}, " +
                $"J3={FormatNumber(frame.Joints.J3Degrees)}, J4={FormatNumber(frame.Joints.J4Degrees)}, " +
                $"J5={FormatNumber(frame.Joints.J5Degrees)}, J6={FormatNumber(frame.Joints.J6Degrees)} deg",
            Command: frame.CommandName ?? "simulation",
            Time: FormatTime(frame.Time, totalDuration),
            Frames: $"{frameNumber} / {frameCount}",
            Footer: FormatFooter(frameNumber, frameCount, frame.Time),
            MovementExplanation:
                $"{frame.CommandName ?? "simulation"} is represented as coordinated six-joint motion. " +
                "J1 rotates the base, J2/J3 position the main links, and J4/J5/J6 orient the wrist and tool. " +
                "The slowest involved joint limits the shared movement speed.");
    }

    public static string FormatScaraToolPose(ScaraPlaybackFrame frame) =>
        $"X={FormatNumber(frame.ToolPose.X)}, Y={FormatNumber(frame.ToolPose.Y)} mm";

    public static string FormatSimpleArmToolPose(SimpleArmPlaybackFrame frame) =>
        $"X={FormatNumber(frame.ToolPose.X)}, Y={FormatNumber(frame.ToolPose.Y)}, O={FormatNumber(frame.ToolPose.OrientationDegrees)} deg";

    public static string FormatDeltaToolPose(DeltaPlaybackFrame frame) =>
        $"X={FormatNumber(frame.ToolPose.XMillimeters)}, Y={FormatNumber(frame.ToolPose.YMillimeters)}, Z={FormatNumber(frame.ToolPose.ZMillimeters)} mm";

    public static string FormatDroneAttitude(DronePlaybackFrame frame) =>
        $"R={FormatNumber(frame.Pose.RollDegrees)}, P={FormatNumber(frame.Pose.PitchDegrees)}, Y={FormatNumber(frame.Pose.YawDegrees)} deg";

    public static string FormatIndustrialArmToolPose(IndustrialArmPlaybackFrame frame) =>
        $"X={FormatNumber(frame.ToolPose.XMillimeters)}, Y={FormatNumber(frame.ToolPose.YMillimeters)}, " +
        $"Z={FormatNumber(frame.ToolPose.ZMillimeters)} mm | R={FormatNumber(frame.ToolPose.RollDegrees)}, " +
        $"P={FormatNumber(frame.ToolPose.PitchDegrees)}, Y={FormatNumber(frame.ToolPose.YawDegrees)} deg";

    private static string FormatTime(
        TimeSpan frameTime,
        TimeSpan totalDuration) =>
        $"{FormatNumber(frameTime.TotalSeconds)} / {FormatNumber(totalDuration.TotalSeconds)} s";

    private static string FormatFooter(
        int frameNumber,
        int frameCount,
        TimeSpan frameTime) =>
        $"Frame {frameNumber}/{frameCount} | t={FormatNumber(frameTime.TotalSeconds)}s";

    private static int NormalizeFrameNumber(
        int zeroBasedFrameIndex,
        int frameCount) =>
        Math.Clamp(zeroBasedFrameIndex + 1, 1, Math.Max(1, frameCount));

    private static string FormatNumber(double value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);
}
