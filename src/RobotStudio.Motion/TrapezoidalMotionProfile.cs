namespace RobotStudio.Motion;

public sealed class TrapezoidalMotionProfile
{
    private readonly double accelerationDurationSeconds;
    private readonly double constantVelocityDurationSeconds;
    private readonly TimeSpan totalDuration;
    private readonly double accelerationDistance;
    private readonly double constantVelocityDistance;

    public TrapezoidalMotionProfile(
        double distance,
        double maximumVelocity,
        double acceleration)
    {
        ValidatePositiveFinite(distance, nameof(distance), "Profile distance");
        ValidatePositiveFinite(maximumVelocity, nameof(maximumVelocity), "Maximum velocity");
        ValidatePositiveFinite(acceleration, nameof(acceleration), "Acceleration");

        Distance = distance;
        MaximumVelocity = maximumVelocity;
        Acceleration = acceleration;

        var distanceNeededToReachMaximumVelocity = maximumVelocity * maximumVelocity / acceleration;

        if (distance <= distanceNeededToReachMaximumVelocity)
        {
            IsTriangular = true;
            PeakVelocity = Math.Sqrt(distance * acceleration);
            accelerationDurationSeconds = PeakVelocity / acceleration;
            constantVelocityDurationSeconds = 0;
        }
        else
        {
            IsTriangular = false;
            PeakVelocity = maximumVelocity;
            accelerationDurationSeconds = maximumVelocity / acceleration;
            constantVelocityDurationSeconds =
                (distance - distanceNeededToReachMaximumVelocity) / maximumVelocity;
        }

        accelerationDistance = 0.5 * acceleration * accelerationDurationSeconds * accelerationDurationSeconds;
        constantVelocityDistance = PeakVelocity * constantVelocityDurationSeconds;
        totalDuration = TimeSpan.FromSeconds(
            (2 * accelerationDurationSeconds) + constantVelocityDurationSeconds);
    }

    public double Distance { get; }

    public double MaximumVelocity { get; }

    public double Acceleration { get; }

    public double PeakVelocity { get; }

    public bool IsTriangular { get; }

    public TimeSpan AccelerationDuration => TimeSpan.FromSeconds(accelerationDurationSeconds);

    public TimeSpan ConstantVelocityDuration => TimeSpan.FromSeconds(constantVelocityDurationSeconds);

    public TimeSpan DecelerationDuration => AccelerationDuration;

    public TimeSpan TotalDuration => totalDuration;

    public MotionProfileSample SampleAt(TimeSpan time)
    {
        var totalDurationSeconds = totalDuration.TotalSeconds;
        var elapsedSeconds = Math.Clamp(time.TotalSeconds, 0, totalDurationSeconds);

        if (elapsedSeconds >= totalDurationSeconds)
        {
            return CreateSample(
                totalDurationSeconds,
                Distance,
                velocity: 0,
                acceleration: 0,
                MotionProfilePhase.Completed);
        }

        if (elapsedSeconds < accelerationDurationSeconds)
        {
            return CreateSample(
                elapsedSeconds,
                0.5 * Acceleration * elapsedSeconds * elapsedSeconds,
                Acceleration * elapsedSeconds,
                Acceleration,
                MotionProfilePhase.Acceleration);
        }

        var constantVelocityEndSeconds =
            accelerationDurationSeconds + constantVelocityDurationSeconds;

        if (elapsedSeconds < constantVelocityEndSeconds)
        {
            var phaseTime = elapsedSeconds - accelerationDurationSeconds;

            return CreateSample(
                elapsedSeconds,
                accelerationDistance + (PeakVelocity * phaseTime),
                PeakVelocity,
                acceleration: 0,
                MotionProfilePhase.ConstantVelocity);
        }

        var decelerationTime = elapsedSeconds - constantVelocityEndSeconds;
        var distance =
            accelerationDistance +
            constantVelocityDistance +
            (PeakVelocity * decelerationTime) -
            (0.5 * Acceleration * decelerationTime * decelerationTime);

        return CreateSample(
            elapsedSeconds,
            distance,
            PeakVelocity - (Acceleration * decelerationTime),
            -Acceleration,
            MotionProfilePhase.Deceleration);
    }

    private MotionProfileSample CreateSample(
        double elapsedSeconds,
        double distance,
        double velocity,
        double acceleration,
        MotionProfilePhase phase) =>
        new(
            TimeSpan.FromSeconds(elapsedSeconds),
            Math.Clamp(distance, 0, Distance),
            Math.Clamp(distance / Distance, 0, 1),
            Math.Max(0, velocity),
            acceleration,
            phase);

    private static void ValidatePositiveFinite(
        double value,
        string parameterName,
        string description)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"{description} must be a finite value greater than zero.");
        }
    }
}
