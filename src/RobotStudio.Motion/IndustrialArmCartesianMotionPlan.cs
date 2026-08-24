using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record IndustrialArmCartesianMotionPlan(
    IndustrialArmToolPose StartToolPose,
    IndustrialArmToolPose EndToolPose,
    double ToolDistanceMillimeters,
    double MaximumOrientationTravelDegrees,
    IndustrialArmConfiguration Configuration,
    TrapezoidalMotionProfile? ProgressMotionProfile,
    IReadOnlyList<IndustrialArmCartesianMotionSegment> Segments)
{
    public TimeSpan TotalDuration => ProgressMotionProfile?.TotalDuration ?? TimeSpan.Zero;

    public bool IsStationary => Segments.Count == 0;
}
