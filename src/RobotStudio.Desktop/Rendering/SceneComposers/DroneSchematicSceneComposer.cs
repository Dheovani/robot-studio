using System.Windows.Media;
using System.Windows.Media.Media3D;
using RobotStudio.Domain.Aerial;
using RobotStudio.Simulation;

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
                CreateWorkspace(profile),
                CreatePath(snapshot, frameIndex),
                CreateRobot(snapshot.Frames[frameIndex])
            ]);
    }

    private static Model3DGroup CreateWorkspace(DroneProfile profile)
    {
        var group = new Model3DGroup();
        var min = new Point3D(
            profile.MinimumXMillimeters,
            profile.MinimumYMillimeters,
            profile.MinimumZMillimeters);
        var max = new Point3D(
            profile.MaximumXMillimeters,
            profile.MaximumYMillimeters,
            profile.MaximumZMillimeters);
        var gridColor = Color.FromArgb(95, 51, 65, 85);
        var edgeColor = Color.FromArgb(170, 96, 165, 250);

        for (var x = profile.MinimumXMillimeters; x <= profile.MaximumXMillimeters; x += 50)
        {
            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                new Point3D(x, min.Y, min.Z),
                new Point3D(x, max.Y, min.Z),
                thickness: 1.8,
                gridColor));
        }

        for (var y = profile.MinimumYMillimeters; y <= profile.MaximumYMillimeters; y += 50)
        {
            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                new Point3D(min.X, y, min.Z),
                new Point3D(max.X, y, min.Z),
                thickness: 1.8,
                gridColor));
        }

        var corners = new[]
        {
            new Point3D(min.X, min.Y, min.Z),
            new Point3D(max.X, min.Y, min.Z),
            new Point3D(max.X, max.Y, min.Z),
            new Point3D(min.X, max.Y, min.Z),
            new Point3D(min.X, min.Y, max.Z),
            new Point3D(max.X, min.Y, max.Z),
            new Point3D(max.X, max.Y, max.Z),
            new Point3D(min.X, max.Y, max.Z)
        };

        foreach (var (start, end) in new[]
        {
            (0, 1), (1, 2), (2, 3), (3, 0),
            (4, 5), (5, 6), (6, 7), (7, 4),
            (0, 4), (1, 5), (2, 6), (3, 7)
        })
        {
            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                corners[start],
                corners[end],
                thickness: 4,
                edgeColor));
        }

        return group;
    }

    private static Model3DGroup CreatePath(
        DronePlaybackSnapshot snapshot,
        int frameIndex)
    {
        var group = new Model3DGroup();
        var pathColor = Color.FromArgb(220, 45, 212, 191);

        for (var index = 1; index <= frameIndex; index++)
        {
            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                ToPoint3D(snapshot.Frames[index - 1].Pose),
                ToPoint3D(snapshot.Frames[index].Pose),
                thickness: 4,
                pathColor));
        }

        return group;
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

    private static void ValidateFrameIndex(int frameIndex, int frameCount)
    {
        if (frameIndex < 0 || frameIndex >= frameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frameIndex));
        }
    }
}
