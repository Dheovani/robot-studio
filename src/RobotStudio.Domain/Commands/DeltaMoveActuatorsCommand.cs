using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Domain.Commands;

public sealed record DeltaMoveActuatorsCommand : RobotCommand
{
    public DeltaMoveActuatorsCommand(
        DeltaActuatorPosition targetActuators,
        double? requestedActuatorVelocityMillimetersPerSecond = null,
        RobotCommandSource? source = null)
        : base(source)
    {
        if (requestedActuatorVelocityMillimetersPerSecond <= 0)
        {
            throw new InvalidRobotCommandException("Requested Delta actuator velocity must be greater than zero.");
        }

        TargetActuators = targetActuators;
        RequestedActuatorVelocityMillimetersPerSecond = requestedActuatorVelocityMillimetersPerSecond;
    }

    public DeltaActuatorPosition TargetActuators { get; }

    public double? RequestedActuatorVelocityMillimetersPerSecond { get; }
}
