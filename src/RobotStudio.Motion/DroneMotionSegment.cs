using RobotStudio.Domain.Aerial;

namespace RobotStudio.Motion;

public sealed record DroneMotionSegment(
    DronePose Start,
    DronePose End,
    TimeSpan Duration,
    double LinearVelocityMillimetersPerSecond,
    double YawVelocityDegreesPerSecond);
