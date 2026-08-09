using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed record DroneSimulationStep(
    TimeSpan Time,
    RobotState State,
    DronePose Pose,
    string Description,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource)
{
    public TrapezoidalMotionProfile? TranslationProfile { get; init; }

    public TrapezoidalMotionProfile? AttitudeProfile { get; init; }

    public TrapezoidalMotionProfile? YawProfile { get; init; }
}
