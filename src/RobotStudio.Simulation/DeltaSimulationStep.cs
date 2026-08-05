using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Simulation;

public sealed record DeltaSimulationStep(
    TimeSpan Time,
    RobotState State,
    DeltaActuatorPosition Actuators,
    DeltaToolPose ToolPose,
    string Description,
    int? CommandIndex = null,
    string? CommandName = null,
    RobotCommandSource? CommandSource = null);
