using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record SimulationSample(
    TimeSpan Time,
    RobotState State,
    CartesianPosition Position,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource);
