using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record RobotVisualState(
    TimeSpan Time,
    RobotState State,
    VisualVector3 Position,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource);
