using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record SimpleArmMotionPlan(
    SimpleArmJointPosition Start,
    SimpleArmJointPosition End,
    double MaximumJointTravelDegrees,
    IReadOnlyList<SimpleArmMotionSegment> Segments)
{
    public TimeSpan TotalDuration =>
        TimeSpan.FromTicks(Segments.Sum(segment => segment.Duration.Ticks));

    public bool IsStationary => Segments.Count == 0;
}
