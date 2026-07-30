namespace RobotStudio.Domain.Mobile;

public readonly record struct DifferentialDrivePose(
    double X,
    double Y,
    double HeadingDegrees) : IRobotPosition
{
    public double DistanceTo(DifferentialDrivePose other)
    {
        var deltaX = other.X - X;
        var deltaY = other.Y - Y;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    public double AngularDistanceDegreesTo(DifferentialDrivePose other) =>
        Math.Abs(NormalizeSignedDegrees(other.HeadingDegrees - HeadingDegrees));

    public static double NormalizeHeadingDegrees(double headingDegrees)
    {
        var normalized = headingDegrees % 360;
        return normalized < 0
            ? normalized + 360
            : normalized;
    }

    private static double NormalizeSignedDegrees(double degrees)
    {
        var normalized = ((degrees + 180) % 360 + 360) % 360 - 180;
        return normalized == -180
            ? 180
            : normalized;
    }
}
