using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public sealed record MoveToCommand : RobotCommand
{
    public MoveToCommand(
        CartesianPosition targetPosition,
        double? requestedVelocityMillimetersPerSecond = null,
        RobotCommandSource? source = null)
        : base(source)
    {
        if (requestedVelocityMillimetersPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "MOVE requested velocity must be greater than zero. " +
                $"Invalid value: {requestedVelocityMillimetersPerSecond:0.###} mm/s. " +
                "Expected value: greater than zero.");
        }

        TargetPosition = targetPosition;
        RequestedVelocityMillimetersPerSecond = requestedVelocityMillimetersPerSecond;
    }

    public CartesianPosition TargetPosition { get; }

    public double? RequestedVelocityMillimetersPerSecond { get; }
}
