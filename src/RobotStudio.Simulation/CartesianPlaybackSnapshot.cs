namespace RobotStudio.Simulation;

public sealed record CartesianPlaybackSnapshot(
    CartesianWorkspaceBounds WorkspaceBounds,
    IReadOnlyList<RobotVisualState> Frames,
    IReadOnlyList<CartesianRobotPose> Poses,
    IReadOnlyList<CartesianSceneFrame> SceneFrames,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage)
{
    public int FrameCount => Frames.Count;

    public int PoseCount => Poses.Count;

    public int SceneFrameCount => SceneFrames.Count;
}
