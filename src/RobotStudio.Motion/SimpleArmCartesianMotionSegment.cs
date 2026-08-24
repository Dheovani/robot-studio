using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record SimpleArmCartesianMotionSegment(
    double StartProgress,
    double EndProgress,
    SimpleArmToolPose StartToolPose,
    SimpleArmToolPose EndToolPose,
    SimpleArmJointPosition StartJoints,
    SimpleArmJointPosition EndJoints);
