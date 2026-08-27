using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using RobotStudio.Domain.Articulated;
using RobotStudio.Simulation;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Rendering.SceneComposers;

internal static class IndustrialArmSchematicSceneComposer
{
    public static SchematicViewportScene Compose(
        IndustrialArmPlaybackSnapshot snapshot,
        int frameIndex,
        double azimuthDegrees,
        double elevationDegrees,
        double zoomMultiplier)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ValidateFrameIndex(frameIndex, snapshot.FrameCount);

        var reach = GetReach(snapshot.Profile);
        var overlays = SchematicEducationalOverlayComposer.ComposePlanar(
            (float)reach,
            gridSpacing: 80,
            floorZ: -12,
            gridThickness: 1.8f,
            boundaryThickness: 3.5f,
            snapshot.Frames.Take(frameIndex + 1).Select(frame => ToVector(frame.ToolPose)),
            trajectoryThickness: 5);
        return new SchematicViewportScene(
            OrbitCameraFactory.Create(new OrbitCameraSettings(
                Target: new Point3D(0, 0, snapshot.Profile.BaseHeightMillimeters * 0.8),
                AzimuthDegrees: azimuthDegrees,
                ElevationDegrees: elevationDegrees,
                Distance: reach * 2.3 * zoomMultiplier,
                FieldOfView: 42,
                NearPlaneDistance: 1,
                FarPlaneDistance: reach * 10)),
            [
                WpfRobotOverlayAdapter.CreateModelGroup(
                    overlays,
                    Color.FromArgb(175, 96, 165, 250),
                    RobotOverlayKind.CoordinateGrid,
                    RobotOverlayKind.WorkspaceBoundary,
                    RobotOverlayKind.CoordinateOrigin),
                WpfRobotOverlayAdapter.CreateModelGroup(
                    overlays,
                    null,
                    RobotOverlayKind.Trajectory),
                CreateRobot(snapshot.Profile, snapshot.Frames[frameIndex])
            ],
            ambientColor: Color.FromRgb(96, 106, 128));
    }

    private static Model3DGroup CreateRobot(
        IndustrialArmRobotProfile profile,
        IndustrialArmPlaybackFrame frame)
    {
        var yaw = DegreesToRadians(frame.Joints.J1Degrees);
        var shoulderAngle = DegreesToRadians(frame.Joints.J2Degrees);
        var elbowAngle = shoulderAngle + DegreesToRadians(frame.Joints.J3Degrees);
        var wristAngle = elbowAngle + DegreesToRadians(frame.Joints.J5Degrees);
        var shoulder = new Point3D(0, 0, profile.BaseHeightMillimeters);
        var elbow = CreatePoint(shoulder, profile.UpperArmLengthMillimeters, yaw, shoulderAngle);
        var wristRoll = CreatePoint(elbow, profile.ForearmLengthMillimeters, yaw, elbowAngle);
        var tool = CreatePoint(wristRoll, profile.WristLengthMillimeters, yaw, wristAngle);
        var wristPitch = new Point3D(
            wristRoll.X + ((tool.X - wristRoll.X) * 0.45),
            wristRoll.Y + ((tool.Y - wristRoll.Y) * 0.45),
            wristRoll.Z + ((tool.Z - wristRoll.Z) * 0.45));
        var group = new Model3DGroup();

        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(0, 0, profile.BaseHeightMillimeters * 0.36),
            new VisualVector3(92, 92, profile.BaseHeightMillimeters * 0.72),
            Color.FromRgb(30, 64, 175)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(shoulder, elbow, 32, Color.FromRgb(59, 130, 246)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(elbow, wristRoll, 26, Color.FromRgb(34, 197, 94)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(wristRoll, tool, 20, Color.FromRgb(250, 204, 21)));
        group.Children.Add(MeshModelFactory.CreateCube(new Point3D(0, 0, 14), 54, Color.FromRgb(37, 99, 235)));
        group.Children.Add(MeshModelFactory.CreateCube(shoulder, 42, Color.FromRgb(147, 197, 253)));
        group.Children.Add(MeshModelFactory.CreateCube(elbow, 36, Color.FromRgb(134, 239, 172)));
        group.Children.Add(MeshModelFactory.CreateCube(wristRoll, 30, Color.FromRgb(253, 224, 71)));
        group.Children.Add(MeshModelFactory.CreateCube(wristPitch, 24, Color.FromRgb(251, 146, 60)));
        group.Children.Add(MeshModelFactory.CreateCube(tool, 22, Color.FromRgb(248, 113, 113)));

        var roll = DegreesToRadians(frame.ToolPose.RollDegrees);
        var toolAxis = new Vector3D(
            Math.Cos(yaw) * Math.Cos(wristAngle),
            Math.Sin(yaw) * Math.Cos(wristAngle),
            Math.Sin(wristAngle));
        var sideAxis = new Vector3D(
            -Math.Sin(yaw) * Math.Cos(roll),
            Math.Cos(yaw) * Math.Cos(roll),
            Math.Sin(roll));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(
            tool,
            tool + (toolAxis * 58),
            6,
            Color.FromRgb(248, 113, 113)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(
            tool - (sideAxis * 24),
            tool + (sideAxis * 24),
            5,
            Color.FromRgb(192, 132, 252)));

        return group;
    }

    private static Point3D CreatePoint(
        Point3D start,
        double length,
        double yaw,
        double pitch) =>
        new(
            start.X + (length * Math.Cos(pitch) * Math.Cos(yaw)),
            start.Y + (length * Math.Cos(pitch) * Math.Sin(yaw)),
            start.Z + (length * Math.Sin(pitch)));

    private static Point3D ToPoint3D(IndustrialArmToolPose pose) =>
        new(pose.XMillimeters, pose.YMillimeters, pose.ZMillimeters);

    private static Vector3 ToVector(IndustrialArmToolPose pose) =>
        new((float)pose.XMillimeters, (float)pose.YMillimeters, (float)pose.ZMillimeters);

    private static double GetReach(IndustrialArmRobotProfile profile) =>
        profile.BaseHeightMillimeters +
        profile.UpperArmLengthMillimeters +
        profile.ForearmLengthMillimeters +
        profile.WristLengthMillimeters;

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    private static void ValidateFrameIndex(int frameIndex, int frameCount)
    {
        if (frameIndex < 0 || frameIndex >= frameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frameIndex));
        }
    }
}
