using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record IndustrialArmMotionSegment(
    IndustrialArmJointPosition Start,
    IndustrialArmJointPosition End,
    IReadOnlyList<MotionComponent> InvolvedJoints,
    TimeSpan Duration,
    double EffectiveJointVelocityDegreesPerSecond);
