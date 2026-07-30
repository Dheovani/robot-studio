using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record SimpleArmMotionSegment(
    SimpleArmJointPosition Start,
    SimpleArmJointPosition End,
    IReadOnlyList<MotionComponent> InvolvedJoints,
    TimeSpan Duration,
    double EffectiveJointVelocityDegreesPerSecond);
