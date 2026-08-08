using RobotStudio.Domain;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed record SimulationStep(
    TimeSpan Time,
    RobotState State,
    CartesianPosition Position,
    string Description,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource)
{
    public TrapezoidalMotionProfile? MotionProfile { get; init; }
}
