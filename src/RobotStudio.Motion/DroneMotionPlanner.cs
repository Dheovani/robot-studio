using RobotStudio.Domain.Aerial;

namespace RobotStudio.Motion;

public sealed class DroneMotionPlanner
{
    private const double MovementToleranceMillimeters = 0.000_001;
    private const double RotationToleranceDegrees = 0.000_001;

    public DroneMotionPlan PlanMove(
        DronePose start,
        DronePose end,
        DroneProfile robotProfile,
        double? requestedLinearVelocityMillimetersPerSecond = null,
        double? requestedYawVelocityDegreesPerSecond = null,
        double? requestedAttitudeVelocityDegreesPerSecond = null)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedLinearVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedLinearVelocityMillimetersPerSecond),
                "Requested linear velocity must be greater than zero.");
        }

        if (requestedYawVelocityDegreesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedYawVelocityDegreesPerSecond),
                "Requested yaw velocity must be greater than zero.");
        }

        if (requestedAttitudeVelocityDegreesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedAttitudeVelocityDegreesPerSecond),
                "Requested attitude velocity must be greater than zero.");
        }

        robotProfile.ValidatePosition(start);
        robotProfile.ValidatePosition(end);

        var distanceMillimeters = start.DistanceTo(end);
        var yawRotationDegrees = start.AngularDistanceDegreesTo(end);
        var attitudeRotationDegrees = start.MaximumTiltDistanceDegreesTo(end);
        if (distanceMillimeters <= MovementToleranceMillimeters &&
            yawRotationDegrees <= RotationToleranceDegrees &&
            attitudeRotationDegrees <= RotationToleranceDegrees)
        {
            return new DroneMotionPlan(
                start,
                end,
                DistanceMillimeters: 0,
                YawRotationDegrees: 0,
                Segments: Array.Empty<DroneMotionSegment>());
        }

        var linearVelocity = GetEffectiveLinearVelocity(
            robotProfile,
            requestedLinearVelocityMillimetersPerSecond);
        var yawVelocity = GetEffectiveYawVelocity(
            robotProfile,
            requestedYawVelocityDegreesPerSecond);
        var attitudeVelocity = GetEffectiveAttitudeVelocity(
            robotProfile,
            requestedAttitudeVelocityDegreesPerSecond);
        var translationProfile = distanceMillimeters <= MovementToleranceMillimeters
            ? null
            : new TrapezoidalMotionProfile(
                distanceMillimeters,
                linearVelocity,
                robotProfile.MaximumLinearAccelerationMillimetersPerSecondSquared);
        var yawProfile = yawRotationDegrees <= RotationToleranceDegrees
            ? null
            : new TrapezoidalMotionProfile(
                yawRotationDegrees,
                yawVelocity,
                robotProfile.MaximumYawAccelerationDegreesPerSecondSquared);
        var attitudeProfile = attitudeRotationDegrees <= RotationToleranceDegrees
            ? null
            : new TrapezoidalMotionProfile(
                attitudeRotationDegrees,
                attitudeVelocity,
                robotProfile.MaximumAttitudeAccelerationDegreesPerSecondSquared);

        var segment = new DroneMotionSegment(
            start,
            end,
            translationProfile,
            attitudeProfile,
            yawProfile);

        return new DroneMotionPlan(
            start,
            end,
            distanceMillimeters,
            yawRotationDegrees,
            new[] { segment });
    }

    private static double GetEffectiveLinearVelocity(
        DroneProfile robotProfile,
        double? requestedLinearVelocityMillimetersPerSecond) =>
        requestedLinearVelocityMillimetersPerSecond.HasValue
            ? Math.Min(
                robotProfile.MaximumLinearVelocityMillimetersPerSecond,
                requestedLinearVelocityMillimetersPerSecond.Value)
            : robotProfile.MaximumLinearVelocityMillimetersPerSecond;

    private static double GetEffectiveYawVelocity(
        DroneProfile robotProfile,
        double? requestedYawVelocityDegreesPerSecond) =>
        requestedYawVelocityDegreesPerSecond.HasValue
            ? Math.Min(
                robotProfile.MaximumYawVelocityDegreesPerSecond,
                requestedYawVelocityDegreesPerSecond.Value)
            : robotProfile.MaximumYawVelocityDegreesPerSecond;

    private static double GetEffectiveAttitudeVelocity(
        DroneProfile robotProfile,
        double? requestedAttitudeVelocityDegreesPerSecond) =>
        requestedAttitudeVelocityDegreesPerSecond.HasValue
            ? Math.Min(
                robotProfile.MaximumAttitudeVelocityDegreesPerSecond,
                requestedAttitudeVelocityDegreesPerSecond.Value)
            : robotProfile.MaximumAttitudeVelocityDegreesPerSecond;
}
