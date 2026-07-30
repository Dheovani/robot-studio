namespace RobotStudio.Domain.Articulated;

public readonly record struct SimpleArmToolPose(
    double X,
    double Y,
    double OrientationDegrees);
