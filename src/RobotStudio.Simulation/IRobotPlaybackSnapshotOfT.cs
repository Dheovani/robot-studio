namespace RobotStudio.Simulation;

public interface IRobotPlaybackSnapshot<out TFrame> : IRobotPlaybackSnapshot
    where TFrame : IRobotPlaybackFrame
{
    IReadOnlyList<TFrame> Frames { get; }
}
