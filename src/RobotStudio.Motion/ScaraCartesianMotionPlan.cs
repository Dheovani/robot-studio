using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record ScaraCartesianMotionPlan(
    ScaraToolPose StartToolPose,
    ScaraToolPose EndToolPose,
    double ToolDistanceMillimeters,
    TrapezoidalMotionProfile? ToolMotionProfile,
    IReadOnlyList<ScaraCartesianMotionSegment> Segments)
{
    public TimeSpan TotalDuration => ToolMotionProfile?.TotalDuration ?? TimeSpan.Zero;

    public bool IsStationary => Segments.Count == 0;
}
