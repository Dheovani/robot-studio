using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation;

public sealed record DifferentialDrivePlaybackSnapshot(
    DifferentialDriveProfile Profile,
    IReadOnlyList<DifferentialDrivePlaybackFrame> Frames,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage) : IRobotPlaybackSnapshot<DifferentialDrivePlaybackFrame>
{
    public int FrameCount => Frames.Count;

    public IRobotPlaybackFrame FirstFrame => Frames[0];

    public IRobotPlaybackFrame LastFrame => Frames[^1];
}
