using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Domain.Commands;

public sealed record DifferentialDriveMoveCommand : RobotCommand
{
    public DifferentialDriveMoveCommand(
        DifferentialDrivePose targetPose,
        double? requestedLinearVelocityMillimetersPerSecond = null,
        double? requestedAngularVelocityDegreesPerSecond = null,
        RobotCommandSource? source = null)
        : base(source)
    {
        if (requestedLinearVelocityMillimetersPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "Differential drive requested linear velocity must be greater than zero. " +
                $"Invalid value: {requestedLinearVelocityMillimetersPerSecond:0.###} mm/s. " +
                "Expected value: greater than zero.");
        }

        if (requestedAngularVelocityDegreesPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "Differential drive requested angular velocity must be greater than zero. " +
                $"Invalid value: {requestedAngularVelocityDegreesPerSecond:0.###} deg/s. " +
                "Expected value: greater than zero.");
        }

        TargetPose = targetPose;
        RequestedLinearVelocityMillimetersPerSecond = requestedLinearVelocityMillimetersPerSecond;
        RequestedAngularVelocityDegreesPerSecond = requestedAngularVelocityDegreesPerSecond;
    }

    public DifferentialDrivePose TargetPose { get; }

    public double? RequestedLinearVelocityMillimetersPerSecond { get; }

    public double? RequestedAngularVelocityDegreesPerSecond { get; }
}
