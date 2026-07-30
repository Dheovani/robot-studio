using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion;

public sealed class ScaraMotionPlanner
{
    private const double JointToleranceDegrees = 0.000_001;

    public ScaraMotionPlan PlanMove(
        ScaraJointPosition start,
        ScaraJointPosition end,
        ScaraRobotProfile robotProfile,
        double? requestedJointVelocityDegreesPerSecond = null)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);

        if (requestedJointVelocityDegreesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedJointVelocityDegreesPerSecond),
                "Requested SCARA joint velocity must be greater than zero.");
        }

        robotProfile.ValidatePosition(start);
        robotProfile.ValidatePosition(end);

        var maximumJointTravel = start.MaximumJointDeltaTo(end);
        var involvedJoints = GetInvolvedJoints(start, end, robotProfile);

        if (maximumJointTravel <= JointToleranceDegrees)
        {
            return new ScaraMotionPlan(
                start,
                end,
                MaximumJointTravelDegrees: 0,
                Segments: Array.Empty<ScaraMotionSegment>());
        }

        if (involvedJoints.Length == 0)
        {
            throw new ImpossibleMovementException(
                "The SCARA joint movement is greater than zero, but no joint has a measurable displacement.");
        }

        var jointVelocity = GetEffectiveJointVelocity(involvedJoints, requestedJointVelocityDegreesPerSecond);
        var duration = TimeSpan.FromSeconds(maximumJointTravel / jointVelocity);
        var segment = new ScaraMotionSegment(
            start,
            end,
            involvedJoints.Select(joint => new MotionComponent(joint.Id.ToString())).ToArray(),
            duration,
            jointVelocity);

        return new ScaraMotionPlan(start, end, maximumJointTravel, new[] { segment });
    }

    private static ScaraJoint[] GetInvolvedJoints(
        ScaraJointPosition start,
        ScaraJointPosition end,
        ScaraRobotProfile robotProfile) =>
        robotProfile.Joints
            .Where(joint => Math.Abs(end.GetCoordinate(joint.Id) - start.GetCoordinate(joint.Id)) > JointToleranceDegrees)
            .ToArray();

    private static double GetEffectiveJointVelocity(
        IReadOnlyCollection<ScaraJoint> involvedJoints,
        double? requestedJointVelocityDegreesPerSecond)
    {
        var jointLimitedVelocity = involvedJoints.Min(joint => joint.MaximumVelocityDegreesPerSecond);

        return requestedJointVelocityDegreesPerSecond.HasValue
            ? Math.Min(jointLimitedVelocity, requestedJointVelocityDegreesPerSecond.Value)
            : jointLimitedVelocity;
    }
}
