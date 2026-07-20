using RobotStudio.Domain;

namespace RobotStudio.Motion;

public sealed record MotionSegment<TPosition>(
    TPosition Start,
    TPosition End,
    IReadOnlyList<MotionComponent> InvolvedComponents,
    TimeSpan Duration,
    double VelocityMillimetersPerSecond)
    where TPosition : IRobotPosition;
