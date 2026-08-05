using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record DronePlaybackFrame(
    TimeSpan Time,
    RobotState State,
    DronePose Pose,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource) : IRobotPlaybackFrame;
