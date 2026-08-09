using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Aerial;

public sealed record DroneProfile : IRobotProfile<DronePose>
{
    public DroneProfile(
        double minimumXMillimeters,
        double maximumXMillimeters,
        double minimumYMillimeters,
        double maximumYMillimeters,
        double minimumZMillimeters,
        double maximumZMillimeters,
        double maximumLinearVelocityMillimetersPerSecond,
        double maximumYawVelocityDegreesPerSecond,
        double maximumLinearAccelerationMillimetersPerSecondSquared,
        double maximumYawAccelerationDegreesPerSecondSquared,
        double maximumTiltDegrees,
        double maximumAttitudeVelocityDegreesPerSecond,
        double maximumAttitudeAccelerationDegreesPerSecondSquared)
    {
        if (maximumXMillimeters <= minimumXMillimeters)
        {
            throw new ArgumentException("Maximum X limit must be greater than minimum X limit.");
        }

        if (maximumYMillimeters <= minimumYMillimeters)
        {
            throw new ArgumentException("Maximum Y limit must be greater than minimum Y limit.");
        }

        if (maximumZMillimeters <= minimumZMillimeters)
        {
            throw new ArgumentException("Maximum Z limit must be greater than minimum Z limit.");
        }

        if (maximumLinearVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentException("Maximum linear velocity must be greater than zero.");
        }

        if (maximumYawVelocityDegreesPerSecond <= 0)
        {
            throw new ArgumentException("Maximum yaw velocity must be greater than zero.");
        }

        if (maximumLinearAccelerationMillimetersPerSecondSquared <= 0)
        {
            throw new ArgumentException("Maximum linear acceleration must be greater than zero.");
        }

        if (maximumYawAccelerationDegreesPerSecondSquared <= 0)
        {
            throw new ArgumentException("Maximum yaw acceleration must be greater than zero.");
        }

        if (maximumTiltDegrees <= 0 || maximumTiltDegrees > 90)
        {
            throw new ArgumentException("Maximum tilt must be greater than zero and no greater than 90 degrees.");
        }

        if (maximumAttitudeVelocityDegreesPerSecond <= 0)
        {
            throw new ArgumentException("Maximum attitude velocity must be greater than zero.");
        }

        if (maximumAttitudeAccelerationDegreesPerSecondSquared <= 0)
        {
            throw new ArgumentException("Maximum attitude acceleration must be greater than zero.");
        }

        MinimumXMillimeters = minimumXMillimeters;
        MaximumXMillimeters = maximumXMillimeters;
        MinimumYMillimeters = minimumYMillimeters;
        MaximumYMillimeters = maximumYMillimeters;
        MinimumZMillimeters = minimumZMillimeters;
        MaximumZMillimeters = maximumZMillimeters;
        MaximumLinearVelocityMillimetersPerSecond = maximumLinearVelocityMillimetersPerSecond;
        MaximumYawVelocityDegreesPerSecond = maximumYawVelocityDegreesPerSecond;
        MaximumLinearAccelerationMillimetersPerSecondSquared = maximumLinearAccelerationMillimetersPerSecondSquared;
        MaximumYawAccelerationDegreesPerSecondSquared = maximumYawAccelerationDegreesPerSecondSquared;
        MaximumTiltDegrees = maximumTiltDegrees;
        MaximumAttitudeVelocityDegreesPerSecond = maximumAttitudeVelocityDegreesPerSecond;
        MaximumAttitudeAccelerationDegreesPerSecondSquared = maximumAttitudeAccelerationDegreesPerSecondSquared;
    }

    public double MinimumXMillimeters { get; }

    public double MaximumXMillimeters { get; }

    public double MinimumYMillimeters { get; }

    public double MaximumYMillimeters { get; }

    public double MinimumZMillimeters { get; }

    public double MaximumZMillimeters { get; }

    public double MaximumLinearVelocityMillimetersPerSecond { get; }

    public double MaximumYawVelocityDegreesPerSecond { get; }

    public double MaximumLinearAccelerationMillimetersPerSecondSquared { get; }

    public double MaximumYawAccelerationDegreesPerSecondSquared { get; }

    public double MaximumTiltDegrees { get; }

    public double MaximumAttitudeVelocityDegreesPerSecond { get; }

    public double MaximumAttitudeAccelerationDegreesPerSecondSquared { get; }

    public void ValidatePosition(DronePose position)
    {
        if (position.XMillimeters < MinimumXMillimeters || position.XMillimeters > MaximumXMillimeters)
        {
            throw new PositionOutOfRangeException(
                AxisId.X,
                position.XMillimeters,
                MinimumXMillimeters,
                MaximumXMillimeters);
        }

        if (position.YMillimeters < MinimumYMillimeters || position.YMillimeters > MaximumYMillimeters)
        {
            throw new PositionOutOfRangeException(
                AxisId.Y,
                position.YMillimeters,
                MinimumYMillimeters,
                MaximumYMillimeters);
        }

        if (position.ZMillimeters < MinimumZMillimeters || position.ZMillimeters > MaximumZMillimeters)
        {
            throw new PositionOutOfRangeException(
                AxisId.Z,
                position.ZMillimeters,
                MinimumZMillimeters,
                MaximumZMillimeters);
        }

        _ = DronePose.NormalizeYawDegrees(position.YawDegrees);

        ValidateTilt(nameof(position.RollDegrees), position.RollDegrees);
        ValidateTilt(nameof(position.PitchDegrees), position.PitchDegrees);
    }

    private void ValidateTilt(string component, double degrees)
    {
        if (!double.IsFinite(degrees) || Math.Abs(degrees) > MaximumTiltDegrees)
        {
            throw new InvalidRobotCommandException(
                $"Drone {component} is outside its physical tilt limits. " +
                $"Invalid value: {degrees:0.###} deg. " +
                $"Expected range: {-MaximumTiltDegrees:0.###} deg to {MaximumTiltDegrees:0.###} deg.");
        }
    }
}
