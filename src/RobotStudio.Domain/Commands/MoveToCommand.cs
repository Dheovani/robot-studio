using RobotStudio.Domain;

namespace RobotStudio.Domain.Commands;

public sealed record MoveToCommand(CartesianPosition TargetPosition) : RobotCommand;
