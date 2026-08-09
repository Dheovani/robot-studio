using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion;

public sealed class SimpleArmMotionPlanner
{
    private const double JointToleranceDegrees = 0.000_001;

    public SimpleArmMotionPlan PlanMove(
        SimpleArmJointPosition start,
        SimpleArmJointPosition end,
        SimpleArmRobotProfile robotProfile,
        double? requestedJointVelocityDegreesPerSecond = null)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedJointVelocityDegreesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedJointVelocityDegreesPerSecond),
                "Requested simple arm joint velocity must be greater than zero.");
        }

        robotProfile.ValidatePosition(start);
        robotProfile.ValidatePosition(end);

        var maximumJointTravel = start.MaximumJointDeltaTo(end);
        var involvedJoints = GetInvolvedJoints(start, end, robotProfile);

        if (maximumJointTravel <= JointToleranceDegrees)
        {
            return new SimpleArmMotionPlan(
                start,
                end,
                MaximumJointTravelDegrees: 0,
                Segments: Array.Empty<SimpleArmMotionSegment>());
        }

        if (involvedJoints.Length == 0)
        {
            throw new ImpossibleMovementException(
                "The simple arm joint movement is greater than zero, but no joint has a measurable displacement.");
        }

        var jointVelocity = GetEffectiveJointVelocity(involvedJoints, requestedJointVelocityDegreesPerSecond);
        var jointAcceleration = involvedJoints.Min(joint => joint.MaximumAccelerationDegreesPerSecondSquared);
        var profile = new TrapezoidalMotionProfile(maximumJointTravel, jointVelocity, jointAcceleration);
        var segment = new SimpleArmMotionSegment(
            start,
            end,
            involvedJoints.Select(joint => new MotionComponent(joint.Id.ToString())).ToArray(),
            profile);

        return new SimpleArmMotionPlan(start, end, maximumJointTravel, new[] { segment });
    }

    private static SimpleArmJoint[] GetInvolvedJoints(
        SimpleArmJointPosition start,
        SimpleArmJointPosition end,
        SimpleArmRobotProfile robotProfile) =>
        robotProfile.Joints
            .Where(joint => Math.Abs(end.GetCoordinate(joint.Id) - start.GetCoordinate(joint.Id)) > JointToleranceDegrees)
            .ToArray();

    private static double GetEffectiveJointVelocity(
        IReadOnlyCollection<SimpleArmJoint> involvedJoints,
        double? requestedJointVelocityDegreesPerSecond)
    {
        var jointLimitedVelocity = involvedJoints.Min(joint => joint.MaximumVelocityDegreesPerSecond);

        return requestedJointVelocityDegreesPerSecond.HasValue
            ? Math.Min(jointLimitedVelocity, requestedJointVelocityDegreesPerSecond.Value)
            : jointLimitedVelocity;
    }
}
