using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed record ScaraSimulationStep(
    TimeSpan Time,
    RobotState State,
    ScaraJointPosition Joints,
    ScaraToolPose ToolPose,
    string Description,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource)
{
    public TrapezoidalMotionProfile? MotionProfile { get; init; }

    public ScaraCartesianMotionPlan? CartesianMotionPlan { get; init; }
}
