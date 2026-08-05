using RobotStudio.Domain.Parallel;

namespace RobotStudio.Motion;

public sealed record DeltaMotionSegment(
    DeltaActuatorPosition Start,
    DeltaActuatorPosition End,
    IReadOnlyList<MotionComponent> InvolvedActuators,
    TimeSpan Duration,
    double EffectiveActuatorVelocityMillimetersPerSecond);
