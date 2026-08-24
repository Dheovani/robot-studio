using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public sealed record SimpleArmLinearMoveCommand : RobotCommand
{
    public SimpleArmLinearMoveCommand(
        SimpleArmToolPose targetToolPose,
        double? requestedToolVelocityMillimetersPerSecond = null,
        RobotCommandSource? source = null)
        : base(source)
    {
        if (!double.IsFinite(targetToolPose.X) ||
            !double.IsFinite(targetToolPose.Y) ||
            !double.IsFinite(targetToolPose.OrientationDegrees))
        {
            throw new InvalidRobotCommandException(
                "Simple arm target pose coordinates and orientation must be finite values.");
        }

        if (requestedToolVelocityMillimetersPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "Simple arm requested tool velocity must be greater than zero. " +
                $"Invalid value: {requestedToolVelocityMillimetersPerSecond:0.###} mm/s.");
        }

        TargetToolPose = targetToolPose;
        RequestedToolVelocityMillimetersPerSecond = requestedToolVelocityMillimetersPerSecond;
    }

    public SimpleArmToolPose TargetToolPose { get; }

    public double? RequestedToolVelocityMillimetersPerSecond { get; }
}
