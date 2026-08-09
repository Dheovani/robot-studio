namespace RobotStudio.Simulation;

public readonly record struct DifferentialDriveOdometry(
    double LeftWheelTravelMillimeters,
    double RightWheelTravelMillimeters,
    double LeftWheelRotationDegrees,
    double RightWheelRotationDegrees)
{
    public static DifferentialDriveOdometry Zero => new(0, 0, 0, 0);
}
