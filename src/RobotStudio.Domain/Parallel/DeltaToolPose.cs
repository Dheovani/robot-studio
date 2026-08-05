namespace RobotStudio.Domain.Parallel;

public readonly record struct DeltaToolPose(
    double XMillimeters,
    double YMillimeters,
    double ZMillimeters);
