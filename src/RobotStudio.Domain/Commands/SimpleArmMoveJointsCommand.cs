using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public sealed record SimpleArmMoveJointsCommand : RobotCommand
{
    public SimpleArmMoveJointsCommand(
        SimpleArmJointPosition targetJoints,
        double? requestedJointVelocityDegreesPerSecond = null,
        RobotCommandSource? source = null)
        : base(source)
    {
        if (requestedJointVelocityDegreesPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "Simple arm requested joint velocity must be greater than zero. " +
                $"Invalid value: {requestedJointVelocityDegreesPerSecond:0.###} deg/s.");
        }

        TargetJoints = targetJoints;
        RequestedJointVelocityDegreesPerSecond = requestedJointVelocityDegreesPerSecond;
    }

    public SimpleArmJointPosition TargetJoints { get; }

    public double? RequestedJointVelocityDegreesPerSecond { get; }
}
