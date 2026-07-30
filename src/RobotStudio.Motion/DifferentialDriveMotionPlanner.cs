using RobotStudio.Domain.Mobile;

namespace RobotStudio.Motion;

public sealed class DifferentialDriveMotionPlanner
{
    private const double MovementToleranceMillimeters = 0.000_001;
    private const double RotationToleranceDegrees = 0.000_001;

    public DifferentialDriveMotionPlan PlanMove(
        DifferentialDrivePose start,
        DifferentialDrivePose end,
        DifferentialDriveProfile robotProfile,
        double? requestedLinearVelocityMillimetersPerSecond = null,
        double? requestedAngularVelocityDegreesPerSecond = null)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedLinearVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedLinearVelocityMillimetersPerSecond),
                "Requested linear velocity must be greater than zero.");
        }

        if (requestedAngularVelocityDegreesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedAngularVelocityDegreesPerSecond),
                "Requested angular velocity must be greater than zero.");
        }

        robotProfile.ValidatePosition(start);
        robotProfile.ValidatePosition(end);

        var translationDistanceMillimeters = start.DistanceTo(end);
        var rotationDegrees = start.AngularDistanceDegreesTo(end);
        var segments = new List<DifferentialDriveMotionSegment>(capacity: 2);

        if (translationDistanceMillimeters > MovementToleranceMillimeters)
        {
            var linearVelocity = GetEffectiveLinearVelocity(
                robotProfile,
                requestedLinearVelocityMillimetersPerSecond);
            var translationDuration = TimeSpan.FromSeconds(translationDistanceMillimeters / linearVelocity);
            var translationEnd = end with { HeadingDegrees = start.HeadingDegrees };

            segments.Add(new DifferentialDriveMotionSegment(
                DifferentialDriveMotionKind.Translation,
                start,
                translationEnd,
                translationDuration,
                linearVelocity,
                AngularVelocityDegreesPerSecond: 0));
        }

        if (rotationDegrees > RotationToleranceDegrees)
        {
            var angularVelocity = GetEffectiveAngularVelocity(
                robotProfile,
                requestedAngularVelocityDegreesPerSecond);
            var rotationDuration = TimeSpan.FromSeconds(rotationDegrees / angularVelocity);
            var rotationStart = translationDistanceMillimeters > MovementToleranceMillimeters
                ? end with { HeadingDegrees = start.HeadingDegrees }
                : start;

            segments.Add(new DifferentialDriveMotionSegment(
                DifferentialDriveMotionKind.Rotation,
                rotationStart,
                end,
                rotationDuration,
                LinearVelocityMillimetersPerSecond: 0,
                angularVelocity));
        }

        return new DifferentialDriveMotionPlan(
            start,
            end,
            translationDistanceMillimeters,
            rotationDegrees,
            segments);
    }

    private static double GetEffectiveLinearVelocity(
        DifferentialDriveProfile robotProfile,
        double? requestedLinearVelocityMillimetersPerSecond) =>
        requestedLinearVelocityMillimetersPerSecond.HasValue
            ? Math.Min(
                robotProfile.MaximumLinearVelocityMillimetersPerSecond,
                requestedLinearVelocityMillimetersPerSecond.Value)
            : robotProfile.MaximumLinearVelocityMillimetersPerSecond;

    private static double GetEffectiveAngularVelocity(
        DifferentialDriveProfile robotProfile,
        double? requestedAngularVelocityDegreesPerSecond) =>
        requestedAngularVelocityDegreesPerSecond.HasValue
            ? Math.Min(
                robotProfile.MaximumAngularVelocityDegreesPerSecond,
                requestedAngularVelocityDegreesPerSecond.Value)
            : robotProfile.MaximumAngularVelocityDegreesPerSecond;
}
