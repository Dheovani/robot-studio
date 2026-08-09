using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record SimpleArmMotionSegment(
    SimpleArmJointPosition Start,
    SimpleArmJointPosition End,
    IReadOnlyList<MotionComponent> InvolvedJoints,
    TrapezoidalMotionProfile Profile)
{
    public TimeSpan Duration => Profile.TotalDuration;

    public double EffectiveJointVelocityDegreesPerSecond => Profile.PeakVelocity;

    public double JointVelocityLimitDegreesPerSecond => Profile.MaximumVelocity;

    public double JointAccelerationDegreesPerSecondSquared => Profile.Acceleration;
}
