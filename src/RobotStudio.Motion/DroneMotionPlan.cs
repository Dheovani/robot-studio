using RobotStudio.Domain.Aerial;

namespace RobotStudio.Motion;

public sealed record DroneMotionPlan(
    DronePose Start,
    DronePose End,
    double DistanceMillimeters,
    double YawRotationDegrees,
    IReadOnlyList<DroneMotionSegment> Segments)
{
    public double MaximumTiltRotationDegrees => Start.MaximumTiltDistanceDegreesTo(End);

    public TimeSpan TotalDuration =>
        TimeSpan.FromTicks(Segments.Sum(segment => segment.Duration.Ticks));
}
