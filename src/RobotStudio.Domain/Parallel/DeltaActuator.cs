using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Parallel;

public sealed record DeltaActuator
{
    public DeltaActuator(
        DeltaActuatorId id,
        double minimumMillimeters,
        double maximumMillimeters,
        double maximumVelocityMillimetersPerSecond)
    {
        if (maximumMillimeters <= minimumMillimeters)
        {
            throw new ArgumentException("Delta actuator maximum limit must be greater than its minimum limit.");
        }

        if (maximumVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentException("Delta actuator maximum velocity must be greater than zero.");
        }

        Id = id;
        MinimumMillimeters = minimumMillimeters;
        MaximumMillimeters = maximumMillimeters;
        MaximumVelocityMillimetersPerSecond = maximumVelocityMillimetersPerSecond;
    }

    public DeltaActuatorId Id { get; }

    public double MinimumMillimeters { get; }

    public double MaximumMillimeters { get; }

    public double MaximumVelocityMillimetersPerSecond { get; }

    public void ValidateCoordinate(double coordinateMillimeters)
    {
        if (coordinateMillimeters < MinimumMillimeters || coordinateMillimeters > MaximumMillimeters)
        {
            throw new InvalidRobotCommandException(
                $"{Id} actuator position is outside its physical limits. " +
                $"Invalid value: {coordinateMillimeters:0.###} mm. " +
                $"Expected range: {MinimumMillimeters:0.###} mm to {MaximumMillimeters:0.###} mm.");
        }
    }
}
