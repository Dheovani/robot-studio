using RobotStudio.Domain;

namespace RobotStudio.Motion;

public sealed record MotionPlan<TPosition>(
    TPosition Start,
    TPosition End,
    double DistanceMillimeters,
    IReadOnlyList<MotionSegment<TPosition>> Segments)
    where TPosition : IRobotPosition
{
    public TimeSpan TotalDuration => TimeSpan.FromTicks(Segments.Sum(segment => segment.Duration.Ticks));

    public bool IsStationary => Segments.Count == 0;
}
