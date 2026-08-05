using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record IndustrialArmMotionPlan(
    IndustrialArmJointPosition Start,
    IndustrialArmJointPosition End,
    double MaximumJointTravelDegrees,
    IReadOnlyList<IndustrialArmMotionSegment> Segments)
{
    public TimeSpan TotalDuration =>
        TimeSpan.FromTicks(Segments.Sum(segment => segment.Duration.Ticks));

    public bool IsStationary => Segments.Count == 0;
}
