using RobotStudio.Domain.Parallel;

namespace RobotStudio.Motion;

public sealed record DeltaCartesianMotionPlan(
    DeltaToolPose StartToolPose,
    DeltaToolPose EndToolPose,
    double ToolDistanceMillimeters,
    TrapezoidalMotionProfile? ToolMotionProfile,
    IReadOnlyList<DeltaCartesianMotionSegment> Segments)
{
    public TimeSpan TotalDuration => ToolMotionProfile?.TotalDuration ?? TimeSpan.Zero;

    public bool IsStationary => Segments.Count == 0;
}
