using RobotStudio.Domain.Aerial;

namespace RobotStudio.Simulation;

public sealed record DronePlaybackSnapshot(
    DroneProfile Profile,
    IReadOnlyList<DronePlaybackFrame> Frames,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage) : IRobotPlaybackSnapshot<DronePlaybackFrame>
{
    public int FrameCount => Frames.Count;

    public IRobotPlaybackFrame FirstFrame => Frames[0];

    public IRobotPlaybackFrame LastFrame => Frames[^1];
}
