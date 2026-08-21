using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed record RobotVisualState(
    TimeSpan Time,
    RobotState State,
    VisualVector3 Position,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource,
    double VelocityMillimetersPerSecond = 0,
    double AccelerationMillimetersPerSecondSquared = 0,
    MotionProfilePhase? MotionProfilePhase = null,
    double? RequestedVelocityMillimetersPerSecond = null,
    TimeSpan? RequestedWaitDuration = null) : IRobotPlaybackFrame;
