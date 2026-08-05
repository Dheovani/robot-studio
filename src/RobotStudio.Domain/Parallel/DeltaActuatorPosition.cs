namespace RobotStudio.Domain.Parallel;

public readonly record struct DeltaActuatorPosition(
    double AMillimeters,
    double BMillimeters,
    double CMillimeters) : IRobotPosition
{
    public double GetCoordinate(DeltaActuatorId actuator) => actuator switch
    {
        DeltaActuatorId.A => AMillimeters,
        DeltaActuatorId.B => BMillimeters,
        DeltaActuatorId.C => CMillimeters,
        _ => throw new ArgumentOutOfRangeException(nameof(actuator), actuator, "Unknown Delta actuator.")
    };

    public double MaximumActuatorDeltaTo(DeltaActuatorPosition other) =>
        Math.Max(
            Math.Abs(other.AMillimeters - AMillimeters),
            Math.Max(
                Math.Abs(other.BMillimeters - BMillimeters),
                Math.Abs(other.CMillimeters - CMillimeters)));
}
