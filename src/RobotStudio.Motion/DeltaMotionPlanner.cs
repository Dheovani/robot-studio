using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Motion;

public sealed class DeltaMotionPlanner
{
    private const double ActuatorToleranceMillimeters = 0.000_001;

    public DeltaMotionPlan PlanMove(
        DeltaActuatorPosition start,
        DeltaActuatorPosition end,
        DeltaRobotProfile robotProfile,
        double? requestedActuatorVelocityMillimetersPerSecond = null)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedActuatorVelocityMillimetersPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedActuatorVelocityMillimetersPerSecond),
                "Requested Delta actuator velocity must be greater than zero.");
        }

        robotProfile.ValidatePosition(start);
        robotProfile.ValidatePosition(end);

        var maximumActuatorTravel = start.MaximumActuatorDeltaTo(end);
        var involvedActuators = GetInvolvedActuators(start, end, robotProfile);

        if (maximumActuatorTravel <= ActuatorToleranceMillimeters)
        {
            return new DeltaMotionPlan(
                start,
                end,
                MaximumActuatorTravelMillimeters: 0,
                Segments: Array.Empty<DeltaMotionSegment>());
        }

        if (involvedActuators.Length == 0)
        {
            throw new ImpossibleMovementException(
                "The Delta actuator movement is greater than zero, but no actuator has a measurable displacement.");
        }

        var actuatorVelocity = GetEffectiveActuatorVelocity(
            involvedActuators,
            requestedActuatorVelocityMillimetersPerSecond);
        var actuatorAcceleration = involvedActuators.Min(
            actuator => actuator.MaximumAccelerationMillimetersPerSecondSquared);
        var profile = new TrapezoidalMotionProfile(
            maximumActuatorTravel,
            actuatorVelocity,
            actuatorAcceleration);
        var segment = new DeltaMotionSegment(
            start,
            end,
            involvedActuators.Select(actuator => new MotionComponent(actuator.Id.ToString())).ToArray(),
            profile);

        return new DeltaMotionPlan(start, end, maximumActuatorTravel, new[] { segment });
    }

    private static DeltaActuator[] GetInvolvedActuators(
        DeltaActuatorPosition start,
        DeltaActuatorPosition end,
        DeltaRobotProfile robotProfile) =>
        robotProfile.Actuators
            .Where(actuator => Math.Abs(end.GetCoordinate(actuator.Id) - start.GetCoordinate(actuator.Id)) > ActuatorToleranceMillimeters)
            .ToArray();

    private static double GetEffectiveActuatorVelocity(
        IReadOnlyCollection<DeltaActuator> involvedActuators,
        double? requestedActuatorVelocityMillimetersPerSecond)
    {
        var actuatorLimitedVelocity = involvedActuators.Min(actuator => actuator.MaximumVelocityMillimetersPerSecond);

        return requestedActuatorVelocityMillimetersPerSecond.HasValue
            ? Math.Min(actuatorLimitedVelocity, requestedActuatorVelocityMillimetersPerSecond.Value)
            : actuatorLimitedVelocity;
    }
}
