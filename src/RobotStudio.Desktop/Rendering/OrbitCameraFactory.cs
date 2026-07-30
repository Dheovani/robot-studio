using System.Windows.Media.Media3D;

namespace RobotStudio.Desktop.Rendering;

public static class OrbitCameraFactory
{
    public static PerspectiveCamera Create(OrbitCameraSettings settings)
    {
        var azimuth = DegreesToRadians(settings.AzimuthDegrees);
        var elevation = DegreesToRadians(settings.ElevationDegrees);
        var horizontalDistance = settings.Distance * Math.Cos(elevation);
        var position = new Point3D(
            settings.Target.X + (horizontalDistance * Math.Cos(azimuth)),
            settings.Target.Y + (horizontalDistance * Math.Sin(azimuth)),
            settings.Target.Z + (settings.Distance * Math.Sin(elevation)));

        return new PerspectiveCamera
        {
            Position = position,
            LookDirection = settings.Target - position,
            UpDirection = new Vector3D(0, 0, 1),
            FieldOfView = settings.FieldOfView,
            NearPlaneDistance = settings.NearPlaneDistance,
            FarPlaneDistance = settings.FarPlaneDistance
        };
    }

    public static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360;
        if (normalized > 180)
        {
            normalized -= 360;
        }

        if (normalized < -180)
        {
            normalized += 360;
        }

        return normalized;
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;
}
