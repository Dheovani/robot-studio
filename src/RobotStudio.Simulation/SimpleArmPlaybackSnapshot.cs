using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public sealed record SimpleArmPlaybackSnapshot(
    SimpleArmRobotProfile Profile,
    IReadOnlyList<SimpleArmPlaybackFrame> Frames,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage) : IRobotPlaybackSnapshot<SimpleArmPlaybackFrame>
{
    public int FrameCount => Frames.Count;

    public IRobotPlaybackFrame FirstFrame => Frames[0];

    public IRobotPlaybackFrame LastFrame => Frames[^1];
}
