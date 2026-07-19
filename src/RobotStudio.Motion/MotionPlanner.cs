using RobotStudio.Domain;

namespace RobotStudio.Motion;

public sealed class MotionPlanner
{
    private const double MovementToleranceMillimeters = 0.000_001;

    public MotionPlan PlanLinearMove(
        CartesianPosition start,
        CartesianPosition end,
        RobotProfile robotProfile,
        double? requestedVelocityMillimetersPerSecond = null)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedVelocityMillimetersPerSecond),
                "Requested movement velocity must be greater than zero.");
        }

        robotProfile.ValidatePosition(start);
        robotProfile.ValidatePosition(end);

        var distanceMillimeters = start.DistanceTo(end);
        if (distanceMillimeters <= MovementToleranceMillimeters)
        {
            return new MotionPlan(start, end, Array.Empty<MotionSegment>());
        }

        var velocityMillimetersPerSecond = GetEffectiveVelocity(
            start,
            end,
            robotProfile,
            requestedVelocityMillimetersPerSecond);
        var duration = TimeSpan.FromSeconds(distanceMillimeters / velocityMillimetersPerSecond);
        var segment = new MotionSegment(
            start,
            end,
            duration,
            velocityMillimetersPerSecond);

        return new MotionPlan(start, end, new[] { segment });
    }

    private static double GetEffectiveVelocity(
        CartesianPosition start,
        CartesianPosition end,
        RobotProfile robotProfile,
        double? requestedVelocityMillimetersPerSecond)
    {
        var involvedAxes = robotProfile.Axes
            .Where(axis => Math.Abs(end.GetCoordinate(axis.Id) - start.GetCoordinate(axis.Id)) > MovementToleranceMillimeters)
            .ToArray();

        if (involvedAxes.Length == 0)
        {
            return 0;
        }

        var axisLimitedVelocity = involvedAxes.Min(axis => axis.MaximumVelocityMillimetersPerSecond);

        return requestedVelocityMillimetersPerSecond.HasValue
            ? Math.Min(axisLimitedVelocity, requestedVelocityMillimetersPerSecond.Value)
            : axisLimitedVelocity;
    }
}
