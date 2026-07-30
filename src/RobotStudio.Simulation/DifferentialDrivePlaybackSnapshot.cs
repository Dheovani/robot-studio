using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation;

public sealed record DifferentialDrivePlaybackSnapshot(
    DifferentialDriveProfile Profile,
    IReadOnlyList<DifferentialDrivePlaybackFrame> Frames,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage)
{
    public int FrameCount => Frames.Count;
}
