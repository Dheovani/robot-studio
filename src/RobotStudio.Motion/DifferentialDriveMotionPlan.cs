using RobotStudio.Domain.Mobile;

namespace RobotStudio.Motion;

public sealed record DifferentialDriveMotionPlan(
    DifferentialDrivePose Start,
    DifferentialDrivePose End,
    double TranslationDistanceMillimeters,
    double RotationDegrees,
    IReadOnlyList<DifferentialDriveMotionSegment> Segments)
{
    public TimeSpan TotalDuration => TimeSpan.FromTicks(Segments.Sum(segment => segment.Duration.Ticks));

    public bool IsStationary => Segments.Count == 0;
}
