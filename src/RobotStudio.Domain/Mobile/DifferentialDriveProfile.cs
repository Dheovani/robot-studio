using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Mobile;

public sealed record DifferentialDriveProfile : IRobotProfile<DifferentialDrivePose>
{
    public DifferentialDriveProfile(
        double minimumXMillimeters,
        double maximumXMillimeters,
        double minimumYMillimeters,
        double maximumYMillimeters,
        double wheelBaseMillimeters,
        double wheelRadiusMillimeters,
        double maximumLinearVelocityMillimetersPerSecond,
        double maximumAngularVelocityDegreesPerSecond)
    {
        if (maximumXMillimeters <= minimumXMillimeters)
        {
            throw new ArgumentException("Maximum X limit must be greater than minimum X limit.");
        }

        if (maximumYMillimeters <= minimumYMillimeters)
        {
            throw new ArgumentException("Maximum Y limit must be greater than minimum Y limit.");
        }

        if (wheelBaseMillimeters <= 0)
        {
            throw new ArgumentException("Wheel base must be greater than zero.");
        }

        if (wheelRadiusMillimeters <= 0)
        {
            throw new ArgumentException("Wheel radius must be greater than zero.");
        }

        if (maximumLinearVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentException("Maximum linear velocity must be greater than zero.");
        }

        if (maximumAngularVelocityDegreesPerSecond <= 0)
        {
            throw new ArgumentException("Maximum angular velocity must be greater than zero.");
        }

        MinimumXMillimeters = minimumXMillimeters;
        MaximumXMillimeters = maximumXMillimeters;
        MinimumYMillimeters = minimumYMillimeters;
        MaximumYMillimeters = maximumYMillimeters;
        WheelBaseMillimeters = wheelBaseMillimeters;
        WheelRadiusMillimeters = wheelRadiusMillimeters;
        MaximumLinearVelocityMillimetersPerSecond = maximumLinearVelocityMillimetersPerSecond;
        MaximumAngularVelocityDegreesPerSecond = maximumAngularVelocityDegreesPerSecond;
    }

    public double MinimumXMillimeters { get; }

    public double MaximumXMillimeters { get; }

    public double MinimumYMillimeters { get; }

    public double MaximumYMillimeters { get; }

    public double WheelBaseMillimeters { get; }

    public double WheelRadiusMillimeters { get; }

    public double MaximumLinearVelocityMillimetersPerSecond { get; }

    public double MaximumAngularVelocityDegreesPerSecond { get; }

    public void ValidatePosition(DifferentialDrivePose position)
    {
        if (position.X < MinimumXMillimeters || position.X > MaximumXMillimeters)
        {
            throw new PositionOutOfRangeException(
                Cartesian.AxisId.X,
                position.X,
                MinimumXMillimeters,
                MaximumXMillimeters);
        }

        if (position.Y < MinimumYMillimeters || position.Y > MaximumYMillimeters)
        {
            throw new PositionOutOfRangeException(
                Cartesian.AxisId.Y,
                position.Y,
                MinimumYMillimeters,
                MaximumYMillimeters);
        }

        _ = DifferentialDrivePose.NormalizeHeadingDegrees(position.HeadingDegrees);
    }
}
