namespace RobotStudio.Simulation;

public sealed record CartesianPlaybackSnapshot(
    PlaybackSnapshotMetadata Metadata,
    CartesianWorkspaceBounds WorkspaceBounds,
    CartesianViewportSnapshot Viewport,
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
