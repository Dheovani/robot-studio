using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public sealed record IndustrialArmPlaybackSnapshot(
    IndustrialArmRobotProfile Profile,
    IReadOnlyList<IndustrialArmPlaybackFrame> Frames,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage) : IRobotPlaybackSnapshot<IndustrialArmPlaybackFrame>
{
    public int FrameCount => Frames.Count;

    public IRobotPlaybackFrame FirstFrame => Frames[0];

    public IRobotPlaybackFrame LastFrame => Frames[^1];
}
