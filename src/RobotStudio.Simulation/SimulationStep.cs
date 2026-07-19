using RobotStudio.Domain;

namespace RobotStudio.Simulation;

public sealed record SimulationStep(
    TimeSpan Time,
    RobotState State,
    CartesianPosition Position,
    string Description);
