using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion;

public sealed class IndustrialArmMotionPlanner
{
    private const double JointToleranceDegrees = 0.000_001;

    public IndustrialArmMotionPlan PlanMove(
        IndustrialArmJointPosition start,
        IndustrialArmJointPosition end,
        IndustrialArmRobotProfile robotProfile,
        double? requestedJointVelocityDegreesPerSecond = null)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedJointVelocityDegreesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedJointVelocityDegreesPerSecond),
                "Requested industrial arm joint velocity must be greater than zero.");
        }

        robotProfile.ValidatePosition(start);
        robotProfile.ValidatePosition(end);

        var maximumJointTravel = start.MaximumJointDeltaTo(end);
        if (maximumJointTravel <= JointToleranceDegrees)
        {
            return new IndustrialArmMotionPlan(
                start,
                end,
                MaximumJointTravelDegrees: 0,
                Segments: Array.Empty<IndustrialArmMotionSegment>());
        }

        var involvedJoints = robotProfile.Joints
            .Where(joint => Math.Abs(end.GetCoordinate(joint.Id) - start.GetCoordinate(joint.Id)) > JointToleranceDegrees)
            .ToArray();
        if (involvedJoints.Length == 0)
        {
            throw new ImpossibleMovementException(
                "The industrial arm movement is greater than zero, but no joint has a measurable displacement.");
        }

        var jointLimitedVelocity = involvedJoints.Min(joint => joint.MaximumVelocityDegreesPerSecond);
        var effectiveVelocity = requestedJointVelocityDegreesPerSecond.HasValue
            ? Math.Min(jointLimitedVelocity, requestedJointVelocityDegreesPerSecond.Value)
            : jointLimitedVelocity;
        var effectiveAcceleration = involvedJoints.Min(joint => joint.MaximumAccelerationDegreesPerSecondSquared);
        var profile = new TrapezoidalMotionProfile(maximumJointTravel, effectiveVelocity, effectiveAcceleration);
        var segment = new IndustrialArmMotionSegment(
            start,
            end,
            involvedJoints.Select(joint => new MotionComponent(joint.Id.ToString())).ToArray(),
            profile);

        return new IndustrialArmMotionPlan(start, end, maximumJointTravel, [segment]);
    }
}
