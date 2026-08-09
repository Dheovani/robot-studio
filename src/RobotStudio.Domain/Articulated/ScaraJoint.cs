using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Articulated;

public sealed record ScaraJoint
{
    public ScaraJoint(
        ScaraJointId id,
        double minimumDegrees,
        double maximumDegrees,
        double maximumVelocityDegreesPerSecond,
        double maximumAccelerationDegreesPerSecondSquared)
    {
        if (maximumDegrees <= minimumDegrees)
        {
            throw new ArgumentException("Joint maximum limit must be greater than its minimum limit.");
        }

        if (maximumVelocityDegreesPerSecond <= 0)
        {
            throw new ArgumentException("Joint maximum velocity must be greater than zero.");
        }

        if (maximumAccelerationDegreesPerSecondSquared <= 0)
        {
            throw new ArgumentException("Joint maximum acceleration must be greater than zero.");
        }

        Id = id;
        MinimumDegrees = minimumDegrees;
        MaximumDegrees = maximumDegrees;
        MaximumVelocityDegreesPerSecond = maximumVelocityDegreesPerSecond;
        MaximumAccelerationDegreesPerSecondSquared = maximumAccelerationDegreesPerSecondSquared;
    }

    public ScaraJointId Id { get; }

    public double MinimumDegrees { get; }

    public double MaximumDegrees { get; }

    public double MaximumVelocityDegreesPerSecond { get; }

    public double MaximumAccelerationDegreesPerSecondSquared { get; }

    public void ValidateCoordinate(double coordinateDegrees)
    {
        if (coordinateDegrees < MinimumDegrees || coordinateDegrees > MaximumDegrees)
        {
            throw new InvalidRobotCommandException(
                $"{Id} joint angle is outside its physical limits. " +
                $"Invalid value: {coordinateDegrees:0.###} deg. " +
                $"Expected range: {MinimumDegrees:0.###} deg to {MaximumDegrees:0.###} deg.");
        }
    }
}
