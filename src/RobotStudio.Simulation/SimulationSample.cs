using RobotStudio.Domain;

namespace RobotStudio.Simulation;

public sealed record SimulationSample(
    TimeSpan Time,
    RobotState State,
    CartesianPosition Position,
    int? CommandIndex,
    string? CommandName);
