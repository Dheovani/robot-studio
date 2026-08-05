using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record DroneSimulationStep(
    TimeSpan Time,
    RobotState State,
    DronePose Pose,
    string Description,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource);
