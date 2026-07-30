using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public sealed record ScaraMoveJointsCommand : RobotCommand
{
    public ScaraMoveJointsCommand(
        ScaraJointPosition targetJoints,
        double? requestedJointVelocityDegreesPerSecond = null,
        RobotCommandSource? source = null)
        : base(source)
    {
        if (requestedJointVelocityDegreesPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "SCARA requested joint velocity must be greater than zero. " +
                $"Invalid value: {requestedJointVelocityDegreesPerSecond:0.###} deg/s. " +
                "Expected value: greater than zero.");
        }

        TargetJoints = targetJoints;
        RequestedJointVelocityDegreesPerSecond = requestedJointVelocityDegreesPerSecond;
    }

    public ScaraJointPosition TargetJoints { get; }

    public double? RequestedJointVelocityDegreesPerSecond { get; }
}
