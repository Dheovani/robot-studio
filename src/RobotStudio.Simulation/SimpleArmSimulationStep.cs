using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed record SimpleArmSimulationStep(
    TimeSpan Time,
    RobotState State,
    SimpleArmJointPosition Joints,
    SimpleArmToolPose ToolPose,
    string Description,
    int? CommandIndex = null,
    string? CommandName = null,
    RobotCommandSource? CommandSource = null)
{
    public TrapezoidalMotionProfile? MotionProfile { get; init; }

    public SimpleArmCartesianMotionPlan? CartesianMotionPlan { get; init; }
}
