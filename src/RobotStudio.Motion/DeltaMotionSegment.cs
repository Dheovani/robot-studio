using RobotStudio.Domain.Parallel;

namespace RobotStudio.Motion;

public sealed record DeltaMotionSegment(
    DeltaActuatorPosition Start,
    DeltaActuatorPosition End,
    IReadOnlyList<MotionComponent> InvolvedActuators,
    TrapezoidalMotionProfile Profile)
{
    public TimeSpan Duration => Profile.TotalDuration;

    public double EffectiveActuatorVelocityMillimetersPerSecond => Profile.PeakVelocity;
}
