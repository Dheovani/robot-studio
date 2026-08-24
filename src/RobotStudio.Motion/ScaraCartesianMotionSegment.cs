using RobotStudio.Domain.Articulated;

namespace RobotStudio.Motion;

public sealed record ScaraCartesianMotionSegment(
    ScaraToolPose StartToolPose,
    ScaraToolPose EndToolPose,
    ScaraJointPosition StartJoints,
    ScaraJointPosition EndJoints);
