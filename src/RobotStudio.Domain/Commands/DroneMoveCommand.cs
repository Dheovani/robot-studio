using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public sealed record DroneMoveCommand : RobotCommand
{
    public DroneMoveCommand(
        DronePose targetPose,
        double? requestedLinearVelocityMillimetersPerSecond = null,
        double? requestedYawVelocityDegreesPerSecond = null,
        RobotCommandSource? source = null)
        : base(source)
    {
        if (requestedLinearVelocityMillimetersPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "Drone requested linear velocity must be greater than zero. " +
                $"Invalid value: {requestedLinearVelocityMillimetersPerSecond:0.###} mm/s. " +
                "Expected value: greater than zero.");
        }

        if (requestedYawVelocityDegreesPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "Drone requested yaw velocity must be greater than zero. " +
                $"Invalid value: {requestedYawVelocityDegreesPerSecond:0.###} deg/s. " +
                "Expected value: greater than zero.");
        }

        TargetPose = targetPose;
        RequestedLinearVelocityMillimetersPerSecond = requestedLinearVelocityMillimetersPerSecond;
        RequestedYawVelocityDegreesPerSecond = requestedYawVelocityDegreesPerSecond;
    }

    public DronePose TargetPose { get; }

    public double? RequestedLinearVelocityMillimetersPerSecond { get; }

    public double? RequestedYawVelocityDegreesPerSecond { get; }
}
