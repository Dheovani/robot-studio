using System.Globalization;
using System.Windows.Media.Media3D;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop;

public partial class MainWindow
{
    private static PerspectiveCamera CreateCamera(
        CartesianViewportSnapshot viewport,
        double azimuthDegrees,
        double elevationDegrees,
        double distanceMillimeters)
    {
        var azimuthRadians = DegreesToRadians(azimuthDegrees);
        var elevationRadians = DegreesToRadians(elevationDegrees);
        var horizontalDistance = distanceMillimeters * Math.Cos(elevationRadians);
        var cameraPosition = new VisualVector3(
            viewport.Target.XMillimeters + (horizontalDistance * Math.Cos(azimuthRadians)),
            viewport.Target.YMillimeters + (horizontalDistance * Math.Sin(azimuthRadians)),
            viewport.Target.ZMillimeters + (distanceMillimeters * Math.Sin(elevationRadians)));

        return new PerspectiveCamera
        {
            Position = ToPoint3D(cameraPosition),
            LookDirection = ToVector3D(Subtract(viewport.Target, cameraPosition)),
            UpDirection = ToVector3D(viewport.Up),
            NearPlaneDistance = viewport.NearClipMillimeters,
            FarPlaneDistance = viewport.FarClipMillimeters,
            FieldOfView = 45
        };
    }

    private static Point3D ToPoint3D(VisualVector3 vector) =>
        new(vector.XMillimeters, vector.YMillimeters, vector.ZMillimeters);

    private static Vector3D ToVector3D(VisualVector3 vector) =>
        new(vector.XMillimeters, vector.YMillimeters, vector.ZMillimeters);

    private static VisualVector3 Subtract(VisualVector3 left, VisualVector3 right) =>
        new(
            left.XMillimeters - right.XMillimeters,
            left.YMillimeters - right.YMillimeters,
            left.ZMillimeters - right.ZMillimeters);

    private static double CalculateDistance(VisualVector3 left, VisualVector3 right)
    {
        var deltaX = left.XMillimeters - right.XMillimeters;
        var deltaY = left.YMillimeters - right.YMillimeters;
        var deltaZ = left.ZMillimeters - right.ZMillimeters;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    private static string FormatNumber(double value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);
}
