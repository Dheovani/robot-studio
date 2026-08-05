using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation;

public sealed record DifferentialDrivePlaybackFrame(
    TimeSpan Time,
    RobotState State,
    DifferentialDrivePose Pose,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource) : IRobotPlaybackFrame;
