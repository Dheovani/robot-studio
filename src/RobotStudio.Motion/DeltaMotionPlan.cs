using RobotStudio.Domain.Parallel;

namespace RobotStudio.Motion;

public sealed record DeltaMotionPlan(
    DeltaActuatorPosition Start,
    DeltaActuatorPosition End,
    double MaximumActuatorTravelMillimeters,
    IReadOnlyList<DeltaMotionSegment> Segments)
{
    public TimeSpan TotalDuration => TimeSpan.FromTicks(Segments.Sum(segment => segment.Duration.Ticks));

    public bool IsStationary => Segments.Count == 0;
}
