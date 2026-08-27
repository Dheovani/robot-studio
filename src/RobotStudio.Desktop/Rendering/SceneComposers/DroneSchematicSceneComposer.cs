using System.Windows.Media;
using System.Windows.Media.Media3D;
using RobotStudio.Domain.Aerial;
using RobotStudio.Simulation;
using RobotStudio.Visualization;
using Vector3 = System.Numerics.Vector3;

namespace RobotStudio.Desktop.Rendering.SceneComposers;

internal static class DroneSchematicSceneComposer
{
    public static SchematicViewportScene Compose(
        DronePlaybackSnapshot snapshot,
        int frameIndex,
        double azimuthDegrees,
        double elevationDegrees,
        double zoomMultiplier)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ValidateFrameIndex(frameIndex, snapshot.FrameCount);

        var profile = snapshot.Profile;
        var width = profile.MaximumXMillimeters - profile.MinimumXMillimeters;
        var depth = profile.MaximumYMillimeters - profile.MinimumYMillimeters;
        var height = profile.MaximumZMillimeters - profile.MinimumZMillimeters;
        var diagonal = Math.Sqrt((width * width) + (depth * depth) + (height * height));
        var overlays = SchematicEducationalOverlayComposer.ComposeBox(
            new Vector3(
                (float)profile.MinimumXMillimeters,
                (float)profile.MinimumYMillimeters,
                (float)profile.MinimumZMillimeters),
            new Vector3(
                (float)profile.MaximumXMillimeters,
                (float)profile.MaximumYMillimeters,
                (float)profile.MaximumZMillimeters),
            gridSpacing: 50,
            gridThickness: 1.8f,
            boundaryThickness: 4,
            snapshot.Frames.Take(frameIndex + 1).Select(frame => ToVector(frame.Pose)),
            trajectoryThickness: 4);

        return new SchematicViewportScene(
            OrbitCameraFactory.Create(new OrbitCameraSettings(
                Target: new Point3D(width / 2, depth / 2, height / 2),
                AzimuthDegrees: azimuthDegrees,
                ElevationDegrees: elevationDegrees,
                Distance: diagonal * 1.85 * zoomMultiplier,
                FieldOfView: 42,
                NearPlaneDistance: 1,
                FarPlaneDistance: diagonal * 8)),
            [
                WpfRobotOverlayAdapter.CreateModelGroup(
                    overlays,
                    Color.FromArgb(170, 96, 165, 250),
                    RobotOverlayKind.CoordinateGrid,
                    RobotOverlayKind.WorkspaceBoundary),
                WpfRobotOverlayAdapter.CreateModelGroup(
                    overlays,
                    null,
                    RobotOverlayKind.Trajectory),
                CreateRobot(snapshot.Frames[frameIndex])
            ]);
    }

    private static Model3DGroup CreateRobot(DronePlaybackFrame frame)
    {
        var group = new Model3DGroup();
        var center = ToPoint3D(frame.Pose);
        var attitude = Matrix3D.Identity;
        attitude.Rotate(new Quaternion(new Vector3D(1, 0, 0), frame.Pose.RollDegrees));
        attitude.Rotate(new Quaternion(new Vector3D(0, 1, 0), frame.Pose.PitchDegrees));
        attitude.Rotate(new Quaternion(new Vector3D(0, 0, 1), frame.Pose.YawDegrees));
        var forward = attitude.Transform(new Vector3D(1, 0, 0));
        var right = attitude.Transform(new Vector3D(0, 1, 0));
        var up = attitude.Transform(new Vector3D(0, 0, 1));
        const double armLength = 56;
        const double rotorOffset = 42;
        var front = center + (forward * armLength);
        var back = center - (forward * armLength);
        var left = center - (right * armLength);
        var rightPoint = center + (right * armLength);

        group.Children.Add(MeshModelFactory.CreateOrientedBox(back, front, 8, Color.FromRgb(59, 130, 246)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(left, rightPoint, 8, Color.FromRgb(34, 197, 94)));
        group.Children.Add(MeshModelFactory.CreateCube(center, 26, Color.FromRgb(147, 197, 253)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(center, center + (forward * 74), 5, Color.FromRgb(250, 204, 21)));

        foreach (var rotor in new[] { front, back, left, rightPoint })
        {
            group.Children.Add(MeshModelFactory.CreateBox(
                new VisualVector3(rotor.X, rotor.Y, rotor.Z),
                new VisualVector3(30, 30, 5),
                Color.FromRgb(226, 232, 240)));
            group.Children.Add(MeshModelFactory.CreateBox(
                new VisualVector3(rotor.X + (up.X * 6), rotor.Y + (up.Y * 6), rotor.Z + (up.Z * 6)),
                new VisualVector3(rotorOffset, 7, 4),
                Color.FromRgb(30, 41, 59)));
            group.Children.Add(MeshModelFactory.CreateBox(
                new VisualVector3(rotor.X + (up.X * 6), rotor.Y + (up.Y * 6), rotor.Z + (up.Z * 6)),
                new VisualVector3(7, rotorOffset, 4),
                Color.FromRgb(30, 41, 59)));
        }

        group.Children.Add(MeshModelFactory.CreateOrientedBox(
            center,
            center - (up * 48),
            thickness: 8,
            Color.FromRgb(248, 113, 113)));
        return group;
    }

    private static Point3D ToPoint3D(DronePose pose) =>
        new(pose.XMillimeters, pose.YMillimeters, pose.ZMillimeters);

    private static Vector3 ToVector(DronePose pose) =>
        new((float)pose.XMillimeters, (float)pose.YMillimeters, (float)pose.ZMillimeters);

    private static void ValidateFrameIndex(int frameIndex, int frameCount)
    {
        if (frameIndex < 0 || frameIndex >= frameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frameIndex));
        }
    }
}
