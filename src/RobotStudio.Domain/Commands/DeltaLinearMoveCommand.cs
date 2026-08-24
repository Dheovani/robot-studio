using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Domain.Commands;

public sealed record DeltaLinearMoveCommand : RobotCommand
{
    public DeltaLinearMoveCommand(
        DeltaToolPose targetToolPose,
        double? requestedToolVelocityMillimetersPerSecond = null,
        RobotCommandSource? source = null)
        : base(source)
    {
        if (!double.IsFinite(targetToolPose.XMillimeters) ||
            !double.IsFinite(targetToolPose.YMillimeters) ||
            !double.IsFinite(targetToolPose.ZMillimeters))
        {
            throw new InvalidRobotCommandException(
                "Delta target tool coordinates must be finite values.");
        }

        if (requestedToolVelocityMillimetersPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "Delta requested tool velocity must be greater than zero. " +
                $"Invalid value: {requestedToolVelocityMillimetersPerSecond:0.###} mm/s.");
        }

        TargetToolPose = targetToolPose;
        RequestedToolVelocityMillimetersPerSecond = requestedToolVelocityMillimetersPerSecond;
    }

    public DeltaToolPose TargetToolPose { get; }

    public double? RequestedToolVelocityMillimetersPerSecond { get; }
}
