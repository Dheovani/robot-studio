using RobotStudio.Domain;

namespace RobotStudio.Motion;

public sealed class MotionPlanner
{
    private const double MovementToleranceMillimeters = 0.000_001;

    public MotionPlan PlanLinearMove(
        CartesianPosition start,
        CartesianPosition end,
        RobotProfile robotProfile)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        robotProfile.ValidatePosition(start);
        robotProfile.ValidatePosition(end);

        var distanceMillimeters = start.DistanceTo(end);
        if (distanceMillimeters <= MovementToleranceMillimeters)
        {
            return new MotionPlan(start, end, Array.Empty<MotionSegment>());
        }

        var velocityMillimetersPerSecond = GetLimitingVelocity(start, end, robotProfile);
        var duration = TimeSpan.FromSeconds(distanceMillimeters / velocityMillimetersPerSecond);
        var segment = new MotionSegment(
            start,
            end,
            duration,
            velocityMillimetersPerSecond);

        return new MotionPlan(start, end, new[] { segment });
    }

    private static double GetLimitingVelocity(
        CartesianPosition start,
        CartesianPosition end,
        RobotProfile robotProfile)
    {
        var involvedAxes = robotProfile.Axes
            .Where(axis => Math.Abs(end.GetCoordinate(axis.Id) - start.GetCoordinate(axis.Id)) > MovementToleranceMillimeters)
            .ToArray();

        if (involvedAxes.Length == 0)
        {
            return 0;
        }

        return involvedAxes.Min(axis => axis.MaximumVelocityMillimetersPerSecond);
    }
}
