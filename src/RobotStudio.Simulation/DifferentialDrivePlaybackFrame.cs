using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation;

public sealed record DifferentialDrivePlaybackFrame(
    TimeSpan Time,
    RobotState State,
    DifferentialDrivePose Pose,
    DifferentialDriveOdometry Odometry,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource) : IRobotPlaybackFrame;
