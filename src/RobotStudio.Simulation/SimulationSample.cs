using RobotStudio.Domain;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed record SimulationSample(
    TimeSpan Time,
    RobotState State,
    CartesianPosition Position,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource,
    double VelocityMillimetersPerSecond = 0,
    double AccelerationMillimetersPerSecondSquared = 0,
    MotionProfilePhase? MotionProfilePhase = null,
    double? RequestedVelocityMillimetersPerSecond = null,
    TimeSpan? RequestedWaitDuration = null);
