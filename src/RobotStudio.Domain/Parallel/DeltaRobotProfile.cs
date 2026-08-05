namespace RobotStudio.Domain.Parallel;

public sealed record DeltaRobotProfile : IRobotProfile<DeltaActuatorPosition>
{
    public DeltaRobotProfile(
        double baseRadiusMillimeters,
        double toolZOffsetMillimeters,
        DeltaActuator actuatorA,
        DeltaActuator actuatorB,
        DeltaActuator actuatorC)
    {
        if (baseRadiusMillimeters <= 0)
        {
            throw new ArgumentException("Delta base radius must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(actuatorA);
        ArgumentNullException.ThrowIfNull(actuatorB);
        ArgumentNullException.ThrowIfNull(actuatorC);

        BaseRadiusMillimeters = baseRadiusMillimeters;
        ToolZOffsetMillimeters = toolZOffsetMillimeters;
        ActuatorA = actuatorA;
        ActuatorB = actuatorB;
        ActuatorC = actuatorC;
    }

    public double BaseRadiusMillimeters { get; }

    public double ToolZOffsetMillimeters { get; }

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
