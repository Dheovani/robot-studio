using RobotStudio.Domain;

namespace RobotStudio.Motion;

public sealed record MotionPlan(
    CartesianPosition Start,
    CartesianPosition End,
    IReadOnlyList<MotionSegment> Segments)
{
    public TimeSpan TotalDuration => TimeSpan.FromTicks(Segments.Sum(segment => segment.Duration.Ticks));

    public bool IsStationary => Segments.Count == 0;
}
