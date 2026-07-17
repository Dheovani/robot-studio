using RobotStudio.Domain;

namespace RobotStudio.Motion;

public sealed record MotionSegment(
    CartesianPosition Start,
    CartesianPosition End,
    TimeSpan Duration,
    double VelocityMillimetersPerSecond);
