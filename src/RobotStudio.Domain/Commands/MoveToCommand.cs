using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public sealed record MoveToCommand : RobotCommand
{
    public MoveToCommand(
        CartesianPosition targetPosition,
        double? requestedVelocityMillimetersPerSecond = null)
    {
        if (requestedVelocityMillimetersPerSecond <= 0)
        {
            throw new InvalidRobotCommandException("Requested movement velocity must be greater than zero.");
        }

        TargetPosition = targetPosition;
        RequestedVelocityMillimetersPerSecond = requestedVelocityMillimetersPerSecond;
    }

    public CartesianPosition TargetPosition { get; }

    public double? RequestedVelocityMillimetersPerSecond { get; }
}
