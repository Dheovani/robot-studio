using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed record DifferentialDriveSimulationStep(
    TimeSpan Time,
    RobotState State,
    DifferentialDrivePose Pose,
    string Description,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource)
{
    public TrapezoidalMotionProfile? MotionProfile { get; init; }
}
