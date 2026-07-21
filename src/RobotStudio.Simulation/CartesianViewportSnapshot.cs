namespace RobotStudio.Simulation;

public sealed record CartesianViewportSnapshot(
    VisualVector3 Target,
    VisualVector3 CameraPosition,
    VisualVector3 Up,
    double NearClipMillimeters,
    double FarClipMillimeters);
