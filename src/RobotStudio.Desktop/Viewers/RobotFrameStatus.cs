namespace RobotStudio.Desktop.Viewers;

public sealed record RobotFrameStatus(
    string State,
    string PrimaryPose,
    string Command,
    string Time,
    string Frames,
    string Footer,
    string? MovementExplanation = null);
