using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record SimpleArmCartesianMotionPlan(
    SimpleArmToolPose StartToolPose,
    SimpleArmToolPose EndToolPose,
    double ToolDistanceMillimeters,
    double OrientationTravelDegrees,
    TrapezoidalMotionProfile? ProgressMotionProfile,
    IReadOnlyList<SimpleArmCartesianMotionSegment> Segments)
{
    public TimeSpan TotalDuration => ProgressMotionProfile?.TotalDuration ?? TimeSpan.Zero;

    public bool IsStationary => Segments.Count == 0;
}
