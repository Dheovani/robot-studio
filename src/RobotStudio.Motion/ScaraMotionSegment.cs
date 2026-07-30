using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record ScaraMotionSegment(
    ScaraJointPosition Start,
    ScaraJointPosition End,
    IReadOnlyList<MotionComponent> InvolvedComponents,
    TimeSpan Duration,
    double JointVelocityDegreesPerSecond);
