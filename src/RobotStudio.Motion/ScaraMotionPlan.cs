using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record ScaraMotionPlan(
    ScaraJointPosition Start,
    ScaraJointPosition End,
    double MaximumJointTravelDegrees,
    IReadOnlyList<ScaraMotionSegment> Segments)
{
    public TimeSpan TotalDuration => TimeSpan.FromTicks(Segments.Sum(segment => segment.Duration.Ticks));

    public bool IsStationary => Segments.Count == 0;
}
