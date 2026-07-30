using RobotStudio.Domain.Mobile;

namespace RobotStudio.Motion;

public sealed record DifferentialDriveMotionSegment(
    DifferentialDriveMotionKind Kind,
    DifferentialDrivePose Start,
    DifferentialDrivePose End,
    TimeSpan Duration,
    double LinearVelocityMillimetersPerSecond,
    double AngularVelocityDegreesPerSecond);
