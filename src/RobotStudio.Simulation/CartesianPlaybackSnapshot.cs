namespace RobotStudio.Simulation;

public sealed record CartesianPlaybackSnapshot(
    CartesianWorkspaceBounds WorkspaceBounds,
    IReadOnlyList<RobotVisualState> Frames,
    IReadOnlyList<CartesianRobotPose> Poses,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage)
{
    public int FrameCount => Frames.Count;

    public int PoseCount => Poses.Count;
}
