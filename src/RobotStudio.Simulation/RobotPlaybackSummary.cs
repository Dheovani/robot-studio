using RobotStudio.Domain;

namespace RobotStudio.Simulation;

public sealed record RobotPlaybackSummary(
    int FrameCount,
    TimeSpan TotalDuration,
    bool Succeeded,
    string? FailureMessage,
    TimeSpan FirstFrameTime,
    TimeSpan LastFrameTime,
    RobotState FirstState,
    RobotState LastState,
    string? LastCommandName)
{
    public static RobotPlaybackSummary Create(IRobotPlaybackSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new RobotPlaybackSummary(
            snapshot.FrameCount,
            snapshot.TotalDuration,
            snapshot.Succeeded,
            snapshot.FailureMessage,
            snapshot.FirstFrame.Time,
            snapshot.LastFrame.Time,
            snapshot.FirstFrame.State,
            snapshot.LastFrame.State,
            snapshot.LastFrame.CommandName);
    }
}
