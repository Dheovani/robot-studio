using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using RobotStudio.Domain.Parallel;
using RobotStudio.Simulation;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Rendering.SceneComposers;

internal static class DeltaSchematicSceneComposer
{
    public static SchematicViewportScene Compose(
        DeltaPlaybackSnapshot snapshot,
        int frameIndex,
        double azimuthDegrees,
        double elevationDegrees,
        double zoomMultiplier)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ValidateFrameIndex(frameIndex, snapshot.FrameCount);

        var reach = snapshot.Profile.BaseRadiusMillimeters * 1.2;
        var overlays = SchematicEducationalOverlayComposer.ComposePlanar(
            (float)reach,
            gridSpacing: 50,
            floorZ: -115,
            gridThickness: 1.8f,
            boundaryThickness: 3,
            snapshot.Frames.Take(frameIndex + 1).Select(frame => ToVector(frame.ToolPose)),
            trajectoryThickness: 4);
        return new SchematicViewportScene(
            OrbitCameraFactory.Create(new OrbitCameraSettings(
                Target: new Point3D(0, 0, -15),
                AzimuthDegrees: azimuthDegrees,
                ElevationDegrees: elevationDegrees,
                Distance: reach * 3.2 * zoomMultiplier,
                FieldOfView: 40,
                NearPlaneDistance: 1,
                FarPlaneDistance: reach * 10)),
            [
                WpfRobotOverlayAdapter.CreateModelGroup(
                    overlays,
                    Color.FromArgb(170, 34, 197, 94),
                    RobotOverlayKind.CoordinateGrid,
                    RobotOverlayKind.WorkspaceBoundary,
                    RobotOverlayKind.CoordinateOrigin),
                WpfRobotOverlayAdapter.CreateModelGroup(
                    overlays,
                    null,
                    RobotOverlayKind.Trajectory),
                CreateRobot(snapshot.Profile, snapshot.Frames[frameIndex])
            ]);
    }

    private static Model3DGroup CreateRobot(
        DeltaRobotProfile profile,
        DeltaPlaybackFrame frame)
    {
        const double topZ = 105;
        const double carriageBaseZ = 82;
        var group = new Model3DGroup();
        var anchors = profile.Actuators
            .Select(actuator => GetActuatorAnchor(profile, actuator.Id, topZ))
            .ToArray();
        var carriages = profile.Actuators
            .Select(actuator => GetCarriagePoint(profile, actuator.Id, frame.Actuators, carriageBaseZ))
            .ToArray();
        var tool = ToPoint3D(frame.ToolPose);

        for (var index = 0; index < anchors.Length; index++)
        {
            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                anchors[index],
                anchors[(index + 1) % anchors.Length],
                thickness: 8,
                Color.FromRgb(59, 130, 246)));
        }

        foreach (var actuator in profile.Actuators)
        {
            var anchor = GetActuatorAnchor(profile, actuator.Id, topZ);
            var railBottom = new Point3D(
                anchor.X,
                anchor.Y,
                carriageBaseZ - actuator.MaximumMillimeters - 12);
            var carriage = GetCarriagePoint(
                profile,
                actuator.Id,
                frame.Actuators,
                carriageBaseZ);

            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                anchor,
                railBottom,
                thickness: 9,
                Color.FromRgb(96, 165, 250)));
            group.Children.Add(MeshModelFactory.CreateCube(
                carriage,
                size: 20,
                Color.FromRgb(34, 197, 94)));
            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                carriage,
                tool,
                thickness: 5,
                Color.FromRgb(250, 204, 21)));
        }

        for (var index = 0; index < carriages.Length; index++)
        {
            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                carriages[index],
                carriages[(index + 1) % carriages.Length],
                thickness: 4,
                Color.FromArgb(180, 34, 197, 94)));
        }

        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(tool.X, tool.Y, tool.Z),
            new VisualVector3(34, 34, 10),
            Color.FromRgb(250, 204, 21)));
        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(tool.X, tool.Y, tool.Z - 18),
            new VisualVector3(12, 12, 32),
            Color.FromRgb(248, 113, 113)));

        return group;
    }

    private static Point3D GetActuatorAnchor(
        DeltaRobotProfile profile,
        DeltaActuatorId actuatorId,
        double z)
    {
        var angleDegrees = actuatorId switch
        {
            DeltaActuatorId.A => 90,
            DeltaActuatorId.B => 210,
            DeltaActuatorId.C => 330,
            _ => 90
        };
        var radians = angleDegrees * Math.PI / 180;

        return new Point3D(
            Math.Cos(radians) * profile.BaseRadiusMillimeters,
            Math.Sin(radians) * profile.BaseRadiusMillimeters,
            z);
    }

    private static Point3D GetCarriagePoint(
        DeltaRobotProfile profile,
        DeltaActuatorId actuatorId,
        DeltaActuatorPosition actuators,
        double carriageBaseZ)
    {
        var anchor = GetActuatorAnchor(profile, actuatorId, carriageBaseZ);
        return new Point3D(
            anchor.X,
            anchor.Y,
            carriageBaseZ - actuators.GetCoordinate(actuatorId));
    }

    private static Point3D ToPoint3D(DeltaToolPose pose) =>
        new(pose.XMillimeters, pose.YMillimeters, pose.ZMillimeters);

    private static Vector3 ToVector(DeltaToolPose pose) =>
        new((float)pose.XMillimeters, (float)pose.YMillimeters, (float)pose.ZMillimeters);

    private static void ValidateFrameIndex(int frameIndex, int frameCount)
    {
        if (frameIndex < 0 || frameIndex >= frameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frameIndex));
        }
    }
}
