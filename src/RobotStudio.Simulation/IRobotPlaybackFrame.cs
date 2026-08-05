using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public interface IRobotPlaybackFrame
{
    TimeSpan Time { get; }

    RobotState State { get; }

    int? CommandIndex { get; }

    string? CommandName { get; }

    RobotCommandSource? CommandSource { get; }
}
