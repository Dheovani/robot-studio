using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public sealed record ScaraPlaybackSnapshot(
    ScaraRobotProfile Profile,
    IReadOnlyList<ScaraPlaybackFrame> Frames,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage) : IRobotPlaybackSnapshot<ScaraPlaybackFrame>
{
    public int FrameCount => Frames.Count;

    public IRobotPlaybackFrame FirstFrame => Frames[0];

    public IRobotPlaybackFrame LastFrame => Frames[^1];
}
