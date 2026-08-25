using System.Windows.Media.Media3D;

namespace RobotStudio.Desktop.Rendering;

public static class OrbitCameraInteractionMath
{
    public static Point3D PanTarget(
        Point3D target,
        Vector3D lookDirection,
        Vector3D upDirection,
        double distance,
        double fieldOfViewDegrees,
        double viewportHeight,
        double deltaX,
        double deltaY)
    {
        if (distance <= 0 || viewportHeight <= 0)
        {
            return target;
        }

        var forward = lookDirection;
        forward.Normalize();
        var right = Vector3D.CrossProduct(forward, upDirection);
        right.Normalize();
        var cameraUp = Vector3D.CrossProduct(right, forward);
        cameraUp.Normalize();

        var visibleHeight = 2 * distance * Math.Tan(DegreesToRadians(fieldOfViewDegrees) / 2);
        var unitsPerPixel = visibleHeight / viewportHeight;
        return target - (right * deltaX * unitsPerPixel) + (cameraUp * deltaY * unitsPerPixel);
    }

    public static double FitDistance(double boundingRadius, double fieldOfViewDegrees, double margin = 1.15)
    {
        if (boundingRadius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(boundingRadius));
        }

        if (fieldOfViewDegrees is <= 0 or >= 180)
        {
            throw new ArgumentOutOfRangeException(nameof(fieldOfViewDegrees));
        }

        if (margin < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(margin));
        }

        return (boundingRadius / Math.Sin(DegreesToRadians(fieldOfViewDegrees) / 2)) * margin;
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;
}
