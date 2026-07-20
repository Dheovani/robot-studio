using RobotStudio.Domain;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record SimulationStep(
    TimeSpan Time,
    RobotState State,
    CartesianPosition Position,
    string Description,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource);
