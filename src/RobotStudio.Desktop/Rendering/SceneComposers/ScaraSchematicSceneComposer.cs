using System.Windows.Media;
using System.Windows.Media.Media3D;
using RobotStudio.Domain.Articulated;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop.Rendering.SceneComposers;

internal static class ScaraSchematicSceneComposer
{
    public static SchematicViewportScene Compose(
        ScaraPlaybackSnapshot snapshot,
        int frameIndex,
        double azimuthDegrees,
        double elevationDegrees,
        double zoomMultiplier)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ValidateFrameIndex(frameIndex, snapshot.FrameCount);

        var reach = snapshot.Profile.FirstLinkLengthMillimeters +
                    snapshot.Profile.SecondLinkLengthMillimeters;

        return new SchematicViewportScene(
            OrbitCameraFactory.Create(new OrbitCameraSettings(
                Target: new Point3D(0, 0, 22),
                AzimuthDegrees: azimuthDegrees,
                ElevationDegrees: elevationDegrees,
                Distance: reach * 3.1 * zoomMultiplier,
                FieldOfView: 42,
                NearPlaneDistance: 1,
                FarPlaneDistance: reach * 9)),
            [
                CreateWorkspace(reach),
                CreatePath(snapshot, frameIndex),
                CreateRobot(snapshot.Profile, snapshot.Frames[frameIndex])
            ]);
    }

    private static Model3DGroup CreateWorkspace(double reach) =>
        MeshModelFactory.CreatePlanarWorkspace(
            reach,
            gridSpacing: 50,
            floorZ: -8,
            gridThickness: 1.8,
            ringThickness: 3,
            Color.FromArgb(95, 51, 65, 85),
            Color.FromArgb(170, 59, 130, 246),
            Color.FromRgb(148, 163, 184));

    private static Model3DGroup CreatePath(
        ScaraPlaybackSnapshot snapshot,
        int frameIndex)
    {
        var group = new Model3DGroup();
        var pathColor = Color.FromArgb(210, 45, 212, 191);

        for (var index = 1; index <= frameIndex; index++)
        {
            var previous = snapshot.Frames[index - 1].ToolPose;
            var current = snapshot.Frames[index].ToolPose;
            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                new Point3D(previous.X, previous.Y, 26),
                new Point3D(current.X, current.Y, 26),
                thickness: 5,
                pathColor));
        }

        return group;
    }

    private static Model3DGroup CreateRobot(
        ScaraRobotProfile profile,
        ScaraPlaybackFrame frame)
    {
        const double z = 26;
        var shoulderRadians = frame.Joints.ShoulderDegrees * Math.PI / 180;
        var elbow = new Point3D(
            profile.FirstLinkLengthMillimeters * Math.Cos(shoulderRadians),
            profile.FirstLinkLengthMillimeters * Math.Sin(shoulderRadians),
            z);
        var tool = new Point3D(frame.ToolPose.X, frame.ToolPose.Y, z);
        var basePoint = new Point3D(0, 0, z);
        var group = new Model3DGroup();

        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(0, 0, 8),
            new VisualVector3(42, 42, 38),
            Color.FromRgb(30, 64, 175)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(basePoint, elbow, 18, Color.FromRgb(59, 130, 246)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(elbow, tool, 15, Color.FromRgb(34, 197, 94)));
        group.Children.Add(MeshModelFactory.CreateCube(basePoint, 28, Color.FromRgb(147, 197, 253)));
        group.Children.Add(MeshModelFactory.CreateCube(elbow, 23, Color.FromRgb(134, 239, 172)));
        group.Children.Add(MeshModelFactory.CreateCube(tool, 16, Color.FromRgb(250, 204, 21)));
        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(tool.X, tool.Y, tool.Z - 18),
            new VisualVector3(12, 12, 36),
            Color.FromRgb(248, 113, 113)));

        return group;
    }

    private static void ValidateFrameIndex(int frameIndex, int frameCount)
    {
        if (frameIndex < 0 || frameIndex >= frameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frameIndex));
        }
    }
}
