using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation;

public sealed record DifferentialDriveSimulationStep(
    TimeSpan Time,
    RobotState State,
    DifferentialDrivePose Pose,
    string Description,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource);
