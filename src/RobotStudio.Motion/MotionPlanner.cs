using RobotStudio.Domain;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion;

public sealed class MotionPlanner : IMotionPlanner<CartesianPosition, RobotProfile>
{
    private const double MovementToleranceMillimeters = 0.000_001;

    public MotionPlan<CartesianPosition> PlanMove(
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
        var involvedAxes = GetInvolvedAxes(start, end, robotProfile);

        if (distanceMillimeters <= MovementToleranceMillimeters)
        {
            return new MotionPlan<CartesianPosition>(
                start,
                end,
                DistanceMillimeters: 0,
                Segments: Array.Empty<MotionSegment<CartesianPosition>>());
        }

        if (involvedAxes.Length == 0)
        {
            throw new ImpossibleMovementException(
                "The movement distance is greater than zero, but no axis has a measurable displacement.");
        }

        var velocityMillimetersPerSecond = GetEffectiveVelocity(
            involvedAxes,
            requestedVelocityMillimetersPerSecond);
        var duration = TimeSpan.FromSeconds(distanceMillimeters / velocityMillimetersPerSecond);
        var segment = new MotionSegment<CartesianPosition>(
            start,
            end,
            involvedAxes.Select(axis => new MotionComponent(axis.Id.ToString())).ToArray(),
            duration,
            velocityMillimetersPerSecond);

        return new MotionPlan<CartesianPosition>(start, end, distanceMillimeters, new[] { segment });
    }

    public MotionPlan<CartesianPosition> PlanLinearMove(
        CartesianPosition start,
        CartesianPosition end,
        RobotProfile robotProfile,
        double? requestedVelocityMillimetersPerSecond = null) =>
        PlanMove(
            start,
            end,
            robotProfile,
            requestedVelocityMillimetersPerSecond);

    private static Axis[] GetInvolvedAxes(
        CartesianPosition start,
        CartesianPosition end,
        RobotProfile robotProfile) =>
        robotProfile.Axes
            .Where(axis => Math.Abs(end.GetCoordinate(axis.Id) - start.GetCoordinate(axis.Id)) > MovementToleranceMillimeters)
            .ToArray();

    private static double GetEffectiveVelocity(
        IReadOnlyCollection<Axis> involvedAxes,
        double? requestedVelocityMillimetersPerSecond)
    {
        if (involvedAxes.Count == 0)
        {
            return 0;
        }

        var axisLimitedVelocity = involvedAxes.Min(axis => axis.MaximumVelocityMillimetersPerSecond);

        return requestedVelocityMillimetersPerSecond.HasValue
            ? Math.Min(axisLimitedVelocity, requestedVelocityMillimetersPerSecond.Value)
            : axisLimitedVelocity;
    }
}
