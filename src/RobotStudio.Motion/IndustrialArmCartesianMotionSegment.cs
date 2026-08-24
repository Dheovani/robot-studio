using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record IndustrialArmCartesianMotionSegment(
    double StartProgress,
    double EndProgress,
    IndustrialArmToolPose StartToolPose,
    IndustrialArmToolPose EndToolPose,
    IndustrialArmJointPosition StartJoints,
    IndustrialArmJointPosition EndJoints);
