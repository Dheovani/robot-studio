using RobotStudio.Domain;

namespace RobotStudio.Motion;

public sealed record MotionSegment(
    CartesianPosition Start,
    CartesianPosition End,
    IReadOnlyList<AxisId> InvolvedAxes,
    TimeSpan Duration,
    double VelocityMillimetersPerSecond);
