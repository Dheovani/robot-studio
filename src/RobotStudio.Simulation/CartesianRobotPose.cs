using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record CartesianRobotPose(
    TimeSpan Time,
    RobotState State,
    VisualVector3 Base,
    VisualVector3 XAxisCarriage,
    VisualVector3 YAxisCarriage,
    VisualVector3 ZAxisCarriage,
    VisualVector3 ToolCenterPoint,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource);
