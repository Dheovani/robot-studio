using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record IndustrialArmSimulationStep(
    TimeSpan Time,
    RobotState State,
    IndustrialArmJointPosition Joints,
    IndustrialArmToolPose ToolPose,
    string Description,
    int? CommandIndex = null,
    string? CommandName = null,
    RobotCommandSource? CommandSource = null);
