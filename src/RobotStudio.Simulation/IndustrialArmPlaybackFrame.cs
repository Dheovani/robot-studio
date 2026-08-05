using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record IndustrialArmPlaybackFrame(
    TimeSpan Time,
    RobotState State,
    IndustrialArmJointPosition Joints,
    IndustrialArmToolPose ToolPose,
    int? CommandIndex = null,
    string? CommandName = null,
    RobotCommandSource? CommandSource = null) : IRobotPlaybackFrame;
