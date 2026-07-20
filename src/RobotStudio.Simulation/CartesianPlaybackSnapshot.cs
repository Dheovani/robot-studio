namespace RobotStudio.Simulation;

public sealed record CartesianPlaybackSnapshot(
    CartesianWorkspaceBounds WorkspaceBounds,
    IReadOnlyList<RobotVisualState> Frames,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage)
{
    public int FrameCount => Frames.Count;
}
