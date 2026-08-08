namespace RobotStudio.Motion;

public readonly record struct MotionProfileSample(
    TimeSpan Time,
    double Distance,
    double Progress,
    double Velocity,
    double Acceleration,
    MotionProfilePhase Phase);
