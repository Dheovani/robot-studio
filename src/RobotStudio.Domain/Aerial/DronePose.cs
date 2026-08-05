namespace RobotStudio.Domain.Aerial;

public readonly record struct DronePose(
    double XMillimeters,
    double YMillimeters,
    double ZMillimeters,
    double YawDegrees) : IRobotPosition
{
    public double DistanceTo(DronePose other)
    {
        var deltaX = other.XMillimeters - XMillimeters;
        var deltaY = other.YMillimeters - YMillimeters;
        var deltaZ = other.ZMillimeters - ZMillimeters;

        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
    }

    public double AngularDistanceDegreesTo(DronePose other)
    {
        var delta = NormalizeYawDegrees(other.YawDegrees) - NormalizeYawDegrees(YawDegrees);
        return Math.Abs(NormalizeSignedDegrees(delta));
    }

    public static double NormalizeYawDegrees(double degrees)
    {
        var normalized = degrees % 360;
        if (normalized < 0)
        {
            normalized += 360;
        }

        return normalized;
    }

    public static double NormalizeSignedDegrees(double degrees)
    {
        var normalized = (degrees + 180) % 360;
        if (normalized < 0)
        {
            normalized += 360;
        }

        return normalized - 180;
    }
}
