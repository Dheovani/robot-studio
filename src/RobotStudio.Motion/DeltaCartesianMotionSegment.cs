using RobotStudio.Domain.Parallel;

namespace RobotStudio.Motion;

public sealed record DeltaCartesianMotionSegment(
    DeltaToolPose StartToolPose,
    DeltaToolPose EndToolPose,
    DeltaActuatorPosition StartActuators,
    DeltaActuatorPosition EndActuators);
