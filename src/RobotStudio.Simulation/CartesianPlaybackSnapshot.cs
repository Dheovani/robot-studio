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
    string? FailureMessage,
    IReadOnlyList<CartesianCommandMotionSummary>? CommandMotions = null) : IRobotPlaybackSnapshot<RobotVisualState>
{
    public int FrameCount => Frames.Count;

    public IRobotPlaybackFrame FirstFrame => Frames[0];

    public IRobotPlaybackFrame LastFrame => Frames[^1];

    public int PoseCount => Poses.Count;

    public int SceneFrameCount => SceneFrames.Count;
}
