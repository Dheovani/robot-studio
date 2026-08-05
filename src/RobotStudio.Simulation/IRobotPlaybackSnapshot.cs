namespace RobotStudio.Simulation;

public interface IRobotPlaybackSnapshot
{
    TimeSpan TotalDuration { get; }

    bool Succeeded { get; }

    string? FailureMessage { get; }

    int FrameCount { get; }

    IRobotPlaybackFrame FirstFrame { get; }

    IRobotPlaybackFrame LastFrame { get; }
}
