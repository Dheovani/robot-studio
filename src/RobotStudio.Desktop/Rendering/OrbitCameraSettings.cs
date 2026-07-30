using System.Windows.Media.Media3D;

namespace RobotStudio.Desktop.Rendering;

public sealed record OrbitCameraSettings(
    Point3D Target,
    double AzimuthDegrees,
    double ElevationDegrees,
    double Distance,
    double FieldOfView,
    double NearPlaneDistance,
    double FarPlaneDistance);
