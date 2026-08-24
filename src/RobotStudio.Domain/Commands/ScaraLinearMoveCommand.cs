using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public sealed record ScaraLinearMoveCommand : RobotCommand
{
    public ScaraLinearMoveCommand(
        ScaraToolPose targetToolPose,
        double? requestedToolVelocityMillimetersPerSecond = null,
        RobotCommandSource? source = null)
        : base(source)
    {
        if (requestedToolVelocityMillimetersPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "SCARA requested tool velocity must be greater than zero. " +
                $"Invalid value: {requestedToolVelocityMillimetersPerSecond:0.###} mm/s. " +
                "Expected value: greater than zero.");
        }

        if (!double.IsFinite(targetToolPose.X) || !double.IsFinite(targetToolPose.Y))
        {
            throw new InvalidRobotCommandException(
                "SCARA target tool coordinates must be finite numbers.");
        }

        TargetToolPose = targetToolPose;
        RequestedToolVelocityMillimetersPerSecond = requestedToolVelocityMillimetersPerSecond;
    }

    public ScaraToolPose TargetToolPose { get; }

    public double? RequestedToolVelocityMillimetersPerSecond { get; }
}
