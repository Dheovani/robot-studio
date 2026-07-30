using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public sealed record ScaraPlaybackSnapshot(
    ScaraRobotProfile Profile,
    IReadOnlyList<ScaraPlaybackFrame> Frames,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage)
{
    public int FrameCount => Frames.Count;
}
