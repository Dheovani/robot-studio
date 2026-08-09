using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record ScaraMotionSegment(
    ScaraJointPosition Start,
    ScaraJointPosition End,
    IReadOnlyList<MotionComponent> InvolvedComponents,
    TrapezoidalMotionProfile Profile)
{
    public TimeSpan Duration => Profile.TotalDuration;

    public double JointVelocityDegreesPerSecond => Profile.PeakVelocity;

    public double JointVelocityLimitDegreesPerSecond => Profile.MaximumVelocity;

    public double JointAccelerationDegreesPerSecondSquared => Profile.Acceleration;
}
