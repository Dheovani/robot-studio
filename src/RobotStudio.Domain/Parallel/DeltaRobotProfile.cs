namespace RobotStudio.Domain.Parallel;

public sealed record DeltaRobotProfile : IRobotProfile<DeltaActuatorPosition>
{
    public DeltaRobotProfile(
        double baseRadiusMillimeters,
        double toolZOffsetMillimeters,
        double movingComponentCollisionRadiusMillimeters,
        DeltaActuator actuatorA,
        DeltaActuator actuatorB,
        DeltaActuator actuatorC)
    {
        if (baseRadiusMillimeters <= 0)
        {
            throw new ArgumentException("Delta base radius must be greater than zero.");
        }

        if (!double.IsFinite(movingComponentCollisionRadiusMillimeters) || movingComponentCollisionRadiusMillimeters <= 0)
        {
            throw new ArgumentException("Delta moving-component collision radius must be a finite number greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(actuatorA);
        ArgumentNullException.ThrowIfNull(actuatorB);
        ArgumentNullException.ThrowIfNull(actuatorC);

        BaseRadiusMillimeters = baseRadiusMillimeters;
        ToolZOffsetMillimeters = toolZOffsetMillimeters;
        MovingComponentCollisionRadiusMillimeters = movingComponentCollisionRadiusMillimeters;
        ActuatorA = actuatorA;
        ActuatorB = actuatorB;
        ActuatorC = actuatorC;
    }

    public double BaseRadiusMillimeters { get; }

    public double ToolZOffsetMillimeters { get; }

    public double MovingComponentCollisionRadiusMillimeters { get; }

    public DeltaActuator ActuatorA { get; }

    public DeltaActuator ActuatorB { get; }

    public DeltaActuator ActuatorC { get; }

    public IReadOnlyList<DeltaActuator> Actuators => [ActuatorA, ActuatorB, ActuatorC];

    public void ValidatePosition(DeltaActuatorPosition position)
    {
        foreach (var actuator in Actuators)
        {
            actuator.ValidateCoordinate(position.GetCoordinate(actuator.Id));
        }
    }
}
