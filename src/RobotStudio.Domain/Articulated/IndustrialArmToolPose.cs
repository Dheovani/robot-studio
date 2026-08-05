namespace RobotStudio.Domain.Articulated;

public readonly record struct IndustrialArmToolPose(
    double XMillimeters,
    double YMillimeters,
    double ZMillimeters,
    double RollDegrees,
    double PitchDegrees,
    double YawDegrees);
