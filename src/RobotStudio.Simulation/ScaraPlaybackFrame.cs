using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record ScaraPlaybackFrame(
    TimeSpan Time,
    RobotState State,
    ScaraJointPosition Joints,
    ScaraToolPose ToolPose,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource) : IRobotPlaybackFrame;
