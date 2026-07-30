using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record SimpleArmPlaybackFrame(
    TimeSpan Time,
    RobotState State,
    SimpleArmJointPosition Joints,
    SimpleArmToolPose ToolPose,
    int? CommandIndex = null,
    string? CommandName = null,
    RobotCommandSource? CommandSource = null);
