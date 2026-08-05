using RobotStudio.Domain.Parallel;

namespace RobotStudio.Simulation;

public sealed record DeltaPlaybackSnapshot(
    DeltaRobotProfile Profile,
    IReadOnlyList<DeltaPlaybackFrame> Frames,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage) : IRobotPlaybackSnapshot<DeltaPlaybackFrame>
{
    public int FrameCount => Frames.Count;

    public IRobotPlaybackFrame FirstFrame => Frames[0];

    public IRobotPlaybackFrame LastFrame => Frames[^1];
}
