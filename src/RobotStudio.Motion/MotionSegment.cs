using RobotStudio.Domain;

namespace RobotStudio.Motion;

public sealed record MotionSegment<TPosition>(
    TPosition Start,
    TPosition End,
    IReadOnlyList<MotionComponent> InvolvedComponents,
    TrapezoidalMotionProfile Profile)
    where TPosition : IRobotPosition
{
    public TimeSpan Duration => Profile.TotalDuration;

    public double VelocityMillimetersPerSecond => Profile.PeakVelocity;

    public double VelocityLimitMillimetersPerSecond => Profile.MaximumVelocity;

    public double AccelerationMillimetersPerSecondSquared => Profile.Acceleration;
}
