using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Microsoft.Win32;
using RobotStudio.Desktop.Examples;
using RobotStudio.Desktop.Profiles;
using RobotStudio.Desktop.Rendering;
using RobotStudio.Desktop.Robots;
using RobotStudio.Desktop.Scripting;
using RobotStudio.Desktop.Viewers;
using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Domain.Parallel;
using RobotStudio.Scripting;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop;

public partial class MainWindow
{
    private void RenderDifferentialDriveFrame(int index)
    {
        if (differentialDriveSnapshot is null)
        {
            return;
        }

        differentialDriveFrameIndex = Math.Clamp(index, 0, differentialDriveSnapshot.FrameCount - 1);
        DifferentialDriveTimeline.Value = differentialDriveFrameIndex;

        var frame = differentialDriveSnapshot.Frames[differentialDriveFrameIndex];
        DifferentialDriveCanvas.Children.Clear();

        DrawDifferentialDriveWorkspace(differentialDriveSnapshot.Profile);
        DrawDifferentialDrivePath(differentialDriveSnapshot);
        DrawDifferentialDriveRobot(frame.Pose);

        var status = RobotFramePresenter.Create(
            frame,
            differentialDriveFrameIndex,
            differentialDriveSnapshot.FrameCount,
            differentialDriveSnapshot.TotalDuration);
        DifferentialDriveStateText.Text = status.State;
        DifferentialDrivePoseText.Text = status.PrimaryPose;
        DifferentialDriveCommandText.Text = status.Command;
        DifferentialDriveTimeText.Text = status.Time;
        DifferentialDriveFramesText.Text = status.Frames;
        DifferentialDriveTimeline.Status = status.Footer;
    }

    private void DrawDifferentialDriveWorkspace(DifferentialDriveProfile profile)
    {
        var width = DifferentialDriveCanvas.ActualWidth;
        var height = DifferentialDriveCanvas.ActualHeight;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var topLeft = MapDifferentialDrivePoint(profile.MinimumXMillimeters, profile.MaximumYMillimeters, profile);
        var bottomRight = MapDifferentialDrivePoint(profile.MaximumXMillimeters, profile.MinimumYMillimeters, profile);
        var borderBrush = new SolidColorBrush(Color.FromRgb(71, 85, 105));
        var gridBrush = new SolidColorBrush(Color.FromRgb(30, 41, 59));

        var workspaceRectangle = new Rectangle
        {
            Width = bottomRight.X - topLeft.X,
            Height = bottomRight.Y - topLeft.Y,
            Stroke = borderBrush,
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromArgb(20, 59, 130, 246))
        };
        DifferentialDriveCanvas.Children.Add(workspaceRectangle);
        Canvas.SetLeft(workspaceRectangle, topLeft.X);
        Canvas.SetTop(workspaceRectangle, topLeft.Y);

        for (var x = profile.MinimumXMillimeters; x <= profile.MaximumXMillimeters; x += 50)
        {
            var start = MapDifferentialDrivePoint(x, profile.MinimumYMillimeters, profile);
            var end = MapDifferentialDrivePoint(x, profile.MaximumYMillimeters, profile);
            DifferentialDriveCanvas.Children.Add(new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = gridBrush,
                StrokeThickness = 1
            });
        }

        for (var y = profile.MinimumYMillimeters; y <= profile.MaximumYMillimeters; y += 50)
        {
            var start = MapDifferentialDrivePoint(profile.MinimumXMillimeters, y, profile);
            var end = MapDifferentialDrivePoint(profile.MaximumXMillimeters, y, profile);
            DifferentialDriveCanvas.Children.Add(new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = gridBrush,
                StrokeThickness = 1
            });
        }
    }

    private void DrawDifferentialDrivePath(DifferentialDrivePlaybackSnapshot playbackSnapshot)
    {
        if (playbackSnapshot.Frames.Count < 2)
        {
            return;
        }

        var pathBrush = new SolidColorBrush(Color.FromRgb(45, 212, 191));
        for (var index = 1; index <= differentialDriveFrameIndex; index++)
        {
            var previous = playbackSnapshot.Frames[index - 1].Pose;
            var current = playbackSnapshot.Frames[index].Pose;
            var start = MapDifferentialDrivePoint(previous.X, previous.Y, playbackSnapshot.Profile);
            var end = MapDifferentialDrivePoint(current.X, current.Y, playbackSnapshot.Profile);

            DifferentialDriveCanvas.Children.Add(new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = pathBrush,
                StrokeThickness = 3
            });
        }
    }

    private void DrawDifferentialDriveRobot(DifferentialDrivePose pose)
    {
        if (differentialDriveSnapshot is null)
        {
            return;
        }

        var center = MapDifferentialDrivePoint(pose.X, pose.Y, differentialDriveSnapshot.Profile);
        const double bodyRadius = 18;
        const double headingLength = 34;
        var headingRadians = pose.HeadingDegrees * Math.PI / 180;
        var headingEnd = new Point(
            center.X + (Math.Cos(headingRadians) * headingLength),
            center.Y - (Math.Sin(headingRadians) * headingLength));

        var body = new Ellipse
        {
            Width = bodyRadius * 2,
            Height = bodyRadius * 2,
            Fill = new SolidColorBrush(Color.FromRgb(37, 99, 235)),
            Stroke = new SolidColorBrush(Color.FromRgb(147, 197, 253)),
            StrokeThickness = 2
        };
        DifferentialDriveCanvas.Children.Add(body);
        Canvas.SetLeft(body, center.X - bodyRadius);
        Canvas.SetTop(body, center.Y - bodyRadius);

        DifferentialDriveCanvas.Children.Add(new Line
        {
            X1 = center.X,
            Y1 = center.Y,
            X2 = headingEnd.X,
            Y2 = headingEnd.Y,
            Stroke = new SolidColorBrush(Color.FromRgb(250, 204, 21)),
            StrokeThickness = 4
        });

        var headingDot = new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = new SolidColorBrush(Color.FromRgb(250, 204, 21))
        };
        DifferentialDriveCanvas.Children.Add(headingDot);
        Canvas.SetLeft(headingDot, headingEnd.X - 4);
        Canvas.SetTop(headingDot, headingEnd.Y - 4);

        DrawDifferentialDriveWheel(center, xOffset: -18);
        DrawDifferentialDriveWheel(center, xOffset: 10);
    }

    private void DrawDifferentialDriveWheel(Point center, double xOffset)
    {
        var wheel = new Rectangle
        {
            Width = 8,
            Height = 28,
            RadiusX = 2,
            RadiusY = 2,
            Fill = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            Stroke = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            StrokeThickness = 1
        };
        DifferentialDriveCanvas.Children.Add(wheel);
        Canvas.SetLeft(wheel, center.X + xOffset);
        Canvas.SetTop(wheel, center.Y - 14);
    }

    private Point MapDifferentialDrivePoint(
        double xMillimeters,
        double yMillimeters,
        DifferentialDriveProfile profile)
    {
        const double padding = 36;
        var width = Math.Max(DifferentialDriveCanvas.ActualWidth, 1);
        var height = Math.Max(DifferentialDriveCanvas.ActualHeight, 1);
        var workspaceWidth = profile.MaximumXMillimeters - profile.MinimumXMillimeters;
        var workspaceHeight = profile.MaximumYMillimeters - profile.MinimumYMillimeters;
        var scale = Math.Min(
            (width - (padding * 2)) / workspaceWidth,
            (height - (padding * 2)) / workspaceHeight) *
            differentialDriveZoomMultiplier;
        var contentWidth = workspaceWidth * scale;
        var contentHeight = workspaceHeight * scale;
        var originX = (width - contentWidth) / 2;
        var originY = (height + contentHeight) / 2;

        return new Point(
            originX + ((xMillimeters - profile.MinimumXMillimeters) * scale),
            originY - ((yMillimeters - profile.MinimumYMillimeters) * scale));
    }

    private void RenderScaraFrame(int index)
    {
        if (scaraSnapshot is null)
        {
            return;
        }

        scaraFrameIndex = Math.Clamp(index, 0, scaraSnapshot.FrameCount - 1);
        ScaraTimeline.Value = scaraFrameIndex;

        var frame = scaraSnapshot.Frames[scaraFrameIndex];
        scaraViewportPresenter.Present(new SchematicViewportScene(
            CreateScaraCamera(scaraSnapshot.Profile),
            [
                CreateScaraWorkspaceModel(scaraSnapshot.Profile),
                CreateScaraPathModel(scaraSnapshot),
                CreateScaraRobotModel(scaraSnapshot.Profile, frame)
            ]));

        var status = RobotFramePresenter.Create(
            frame,
            scaraFrameIndex,
            scaraSnapshot.FrameCount,
            scaraSnapshot.TotalDuration);
        ScaraStateText.Text = status.State;
        ScaraJointsText.Text = status.PrimaryPose;
        ScaraToolText.Text = RobotFramePresenter.FormatScaraToolPose(frame);
        ScaraCommandText.Text = status.Command;
        ScaraTimeText.Text = status.Time;
        ScaraTimeline.Status = status.Footer;
        ScaraMovementExplanationText.Text = status.MovementExplanation;
    }

    private PerspectiveCamera CreateScaraCamera(ScaraRobotProfile profile)
    {
        var reach = GetScaraReach(profile);
        return OrbitCameraFactory.Create(new OrbitCameraSettings(
            Target: new Point3D(0, 0, 22),
            AzimuthDegrees: scaraAzimuthDegrees,
            ElevationDegrees: scaraElevationDegrees,
            Distance: reach * 3.1 * scaraZoomMultiplier,
            FieldOfView: 42,
            NearPlaneDistance: 1,
            FarPlaneDistance: reach * 9));
    }

    private static Model3DGroup CreateScaraWorkspaceModel(ScaraRobotProfile profile)
    {
        var reach = GetScaraReach(profile);
        return MeshModelFactory.CreatePlanarWorkspace(
            reach,
            gridSpacing: 50,
            floorZ: -8,
            gridThickness: 1.8,
            ringThickness: 3,
            Color.FromArgb(95, 51, 65, 85),
            Color.FromArgb(170, 59, 130, 246),
            Color.FromRgb(148, 163, 184));
    }

    private Model3DGroup CreateScaraPathModel(ScaraPlaybackSnapshot playbackSnapshot)
    {
        var group = new Model3DGroup();
        var pathColor = Color.FromArgb(210, 45, 212, 191);
        for (var index = 1; index <= scaraFrameIndex; index++)
        {
            var previous = playbackSnapshot.Frames[index - 1].ToolPose;
            var current = playbackSnapshot.Frames[index].ToolPose;

            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                new Point3D(previous.X, previous.Y, 26),
                new Point3D(current.X, current.Y, 26),
                thickness: 5,
                pathColor));
        }

        return group;
    }

    private static Model3DGroup CreateScaraRobotModel(
        ScaraRobotProfile profile,
        ScaraPlaybackFrame frame)
    {
        const double z = 26;
        var shoulderRadians = frame.Joints.ShoulderDegrees * Math.PI / 180;
        var elbowPose = new Point3D(
            profile.FirstLinkLengthMillimeters * Math.Cos(shoulderRadians),
            profile.FirstLinkLengthMillimeters * Math.Sin(shoulderRadians),
            z);
        var toolPoint = new Point3D(frame.ToolPose.X, frame.ToolPose.Y, z);
        var basePoint = new Point3D(0, 0, z);
        var group = new Model3DGroup();

        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(0, 0, 8),
            new VisualVector3(42, 42, 38),
            Color.FromRgb(30, 64, 175)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(basePoint, elbowPose, thickness: 18, Color.FromRgb(59, 130, 246)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(elbowPose, toolPoint, thickness: 15, Color.FromRgb(34, 197, 94)));
        group.Children.Add(MeshModelFactory.CreateCube(basePoint, size: 28, Color.FromRgb(147, 197, 253)));
        group.Children.Add(MeshModelFactory.CreateCube(elbowPose, size: 23, Color.FromRgb(134, 239, 172)));
        group.Children.Add(MeshModelFactory.CreateCube(toolPoint, size: 16, Color.FromRgb(250, 204, 21)));
        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(toolPoint.X, toolPoint.Y, toolPoint.Z - 18),
            new VisualVector3(12, 12, 36),
            Color.FromRgb(248, 113, 113)));

        return group;
    }

    private static double GetScaraReach(ScaraRobotProfile profile) =>
        profile.FirstLinkLengthMillimeters + profile.SecondLinkLengthMillimeters;

    private void RenderSimpleArmFrame(int index)
    {
        if (simpleArmSnapshot is null)
        {
            return;
        }

        simpleArmFrameIndex = Math.Clamp(index, 0, simpleArmSnapshot.FrameCount - 1);
        SimpleArmTimeline.Value = simpleArmFrameIndex;

        var frame = simpleArmSnapshot.Frames[simpleArmFrameIndex];
        simpleArmViewportPresenter.Present(new SchematicViewportScene(
            CreateSimpleArmCamera(simpleArmSnapshot.Profile),
            [
                CreateSimpleArmWorkspaceModel(simpleArmSnapshot.Profile),
                CreateSimpleArmPathModel(simpleArmSnapshot),
                CreateSimpleArmRobotModel(simpleArmSnapshot.Profile, frame)
            ]));

        var status = RobotFramePresenter.Create(
            frame,
            simpleArmFrameIndex,
            simpleArmSnapshot.FrameCount,
            simpleArmSnapshot.TotalDuration);
        SimpleArmStateText.Text = status.State;
        SimpleArmJointsText.Text = status.PrimaryPose;
        SimpleArmToolText.Text = RobotFramePresenter.FormatSimpleArmToolPose(frame);
        SimpleArmCommandText.Text = status.Command;
        SimpleArmTimeText.Text = status.Time;
        SimpleArmTimeline.Status = status.Footer;
        SimpleArmMovementExplanationText.Text = status.MovementExplanation;
    }

    private PerspectiveCamera CreateSimpleArmCamera(SimpleArmRobotProfile profile)
    {
        var reach = GetSimpleArmReach(profile);
        return OrbitCameraFactory.Create(new OrbitCameraSettings(
            Target: new Point3D(0, 0, 18),
            AzimuthDegrees: simpleArmAzimuthDegrees,
            ElevationDegrees: simpleArmElevationDegrees,
            Distance: reach * 3.05 * simpleArmZoomMultiplier,
            FieldOfView: 40,
            NearPlaneDistance: 1,
            FarPlaneDistance: reach * 8));
    }

    private static Model3DGroup CreateSimpleArmWorkspaceModel(SimpleArmRobotProfile profile)
    {
        var reach = GetSimpleArmReach(profile);
        return MeshModelFactory.CreatePlanarWorkspace(
            reach,
            gridSpacing: 50,
            floorZ: -8,
            gridThickness: 1.8,
            ringThickness: 3,
            Color.FromArgb(95, 51, 65, 85),
            Color.FromArgb(170, 34, 197, 94),
            Color.FromRgb(148, 163, 184));
    }

    private Model3DGroup CreateSimpleArmPathModel(SimpleArmPlaybackSnapshot playbackSnapshot)
    {
        var group = new Model3DGroup();
        var pathColor = Color.FromArgb(210, 45, 212, 191);
        for (var index = 1; index <= simpleArmFrameIndex; index++)
        {
            var previous = playbackSnapshot.Frames[index - 1].ToolPose;
            var current = playbackSnapshot.Frames[index].ToolPose;

            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                new Point3D(previous.X, previous.Y, 22),
                new Point3D(current.X, current.Y, 22),
                thickness: 5,
                pathColor));
        }

        return group;
    }

    private static Model3DGroup CreateSimpleArmRobotModel(
        SimpleArmRobotProfile profile,
        SimpleArmPlaybackFrame frame)
    {
        const double z = 24;
        var baseRadians = frame.Joints.BaseDegrees * Math.PI / 180;
        var shoulderRadians = baseRadians + (frame.Joints.ShoulderDegrees * Math.PI / 180);
        var elbowRadians = shoulderRadians + (frame.Joints.ElbowDegrees * Math.PI / 180);

        var shoulder = new Point3D(
            profile.FirstLinkLengthMillimeters * Math.Cos(baseRadians),
            profile.FirstLinkLengthMillimeters * Math.Sin(baseRadians),
            z);
        var elbow = new Point3D(
            shoulder.X + (profile.SecondLinkLengthMillimeters * Math.Cos(shoulderRadians)),
            shoulder.Y + (profile.SecondLinkLengthMillimeters * Math.Sin(shoulderRadians)),
            z);
        var tool = new Point3D(frame.ToolPose.X, frame.ToolPose.Y, z);
        var basePoint = new Point3D(0, 0, z);
        var group = new Model3DGroup();

        group.Children.Add(MeshModelFactory.CreateBox(
            new VisualVector3(0, 0, 5),
            new VisualVector3(38, 38, 34),
            Color.FromRgb(30, 64, 175)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(basePoint, shoulder, thickness: 16, Color.FromRgb(59, 130, 246)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(shoulder, elbow, thickness: 14, Color.FromRgb(34, 197, 94)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(elbow, tool, thickness: 12, Color.FromRgb(250, 204, 21)));
        group.Children.Add(MeshModelFactory.CreateCube(basePoint, size: 26, Color.FromRgb(147, 197, 253)));
        group.Children.Add(MeshModelFactory.CreateCube(shoulder, size: 22, Color.FromRgb(134, 239, 172)));
        group.Children.Add(MeshModelFactory.CreateCube(elbow, size: 20, Color.FromRgb(253, 224, 71)));
        group.Children.Add(MeshModelFactory.CreateCube(tool, size: 16, Color.FromRgb(248, 113, 113)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(
            tool,
            new Point3D(
                tool.X + (Math.Cos(elbowRadians) * 42),
                tool.Y + (Math.Sin(elbowRadians) * 42),
                tool.Z),
            thickness: 5,
            Color.FromRgb(248, 113, 113)));

        return group;
    }

    private static double GetSimpleArmReach(SimpleArmRobotProfile profile) =>
        profile.FirstLinkLengthMillimeters +
        profile.SecondLinkLengthMillimeters +
        profile.ThirdLinkLengthMillimeters;

    private void RenderDeltaFrame(int index)
    {
        if (deltaSnapshot is null)
        {
            return;
        }

        deltaFrameIndex = Math.Clamp(index, 0, deltaSnapshot.FrameCount - 1);
        DeltaTimeline.Value = deltaFrameIndex;

        var frame = deltaSnapshot.Frames[deltaFrameIndex];
        deltaViewportPresenter.Present(new SchematicViewportScene(
            CreateDeltaCamera(deltaSnapshot.Profile),
            [
                CreateDeltaWorkspaceModel(deltaSnapshot.Profile),
                CreateDeltaPathModel(deltaSnapshot),
                CreateDeltaRobotModel(deltaSnapshot.Profile, frame)
            ]));

        var status = RobotFramePresenter.Create(
            frame,
            deltaFrameIndex,
            deltaSnapshot.FrameCount,
            deltaSnapshot.TotalDuration);
        DeltaStateText.Text = status.State;
        DeltaActuatorsText.Text = status.PrimaryPose;
        DeltaToolText.Text = RobotFramePresenter.FormatDeltaToolPose(frame);
        DeltaCommandText.Text = status.Command;
        DeltaTimeText.Text = status.Time;
        DeltaTimeline.Status = status.Footer;
        DeltaMovementExplanationText.Text = status.MovementExplanation;
    }

    private PerspectiveCamera CreateDeltaCamera(DeltaRobotProfile profile)
    {
        var reach = GetDeltaReach(profile);
        return OrbitCameraFactory.Create(new OrbitCameraSettings(
            Target: new Point3D(0, 0, -15),
            AzimuthDegrees: deltaAzimuthDegrees,
            ElevationDegrees: deltaElevationDegrees,
            Distance: reach * 3.2 * deltaZoomMultiplier,
            FieldOfView: 40,
            NearPlaneDistance: 1,
            FarPlaneDistance: reach * 10));
    }

    private static Model3DGroup CreateDeltaWorkspaceModel(DeltaRobotProfile profile)
    {
        var reach = GetDeltaReach(profile);
        return MeshModelFactory.CreatePlanarWorkspace(
            reach,
            gridSpacing: 50,
            floorZ: -115,
            gridThickness: 1.8,
            ringThickness: 3,
            Color.FromArgb(95, 51, 65, 85),
            Color.FromArgb(170, 34, 197, 94),
            Color.FromRgb(148, 163, 184));
    }

    private Model3DGroup CreateDeltaPathModel(DeltaPlaybackSnapshot playbackSnapshot)
    {
        var group = new Model3DGroup();
        var pathColor = Color.FromArgb(220, 45, 212, 191);

        for (var index = 1; index <= deltaFrameIndex; index++)
        {
            var previous = ToPoint3D(playbackSnapshot.Frames[index - 1].ToolPose);
            var current = ToPoint3D(playbackSnapshot.Frames[index].ToolPose);

            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                previous,
                current,
                thickness: 4,
                pathColor));
        }

        return group;
    }

    private static Model3DGroup CreateDeltaRobotModel(
        DeltaRobotProfile profile,
        DeltaPlaybackFrame frame)
    {
        const double topZ = 105;
        const double carriageBaseZ = 82;
        var group = new Model3DGroup();
        var anchors = profile.Actuators
            .Select(actuator => GetDeltaActuatorAnchor(profile, actuator.Id, topZ))
            .ToArray();
        var carriages = profile.Actuators
            .Select(actuator => GetDeltaCarriagePoint(profile, actuator.Id, frame.Actuators, carriageBaseZ))
            .ToArray();
        var tool = ToPoint3D(frame.ToolPose);

        for (var index = 0; index < anchors.Length; index++)
        {
            var next = anchors[(index + 1) % anchors.Length];
            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                anchors[index],
                next,
                thickness: 8,
                Color.FromRgb(59, 130, 246)));
        }

        foreach (var actuator in profile.Actuators)
        {
            var anchor = GetDeltaActuatorAnchor(profile, actuator.Id, topZ);
            var railBottom = new Point3D(anchor.X, anchor.Y, carriageBaseZ - actuator.MaximumMillimeters - 12);
            var carriage = GetDeltaCarriagePoint(profile, actuator.Id, frame.Actuators, carriageBaseZ);

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

    private static Point3D GetDeltaActuatorAnchor(
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

    private static Point3D GetDeltaCarriagePoint(
        DeltaRobotProfile profile,
        DeltaActuatorId actuatorId,
        DeltaActuatorPosition actuators,
        double carriageBaseZ)
    {
        var anchor = GetDeltaActuatorAnchor(profile, actuatorId, z: carriageBaseZ);

        return new Point3D(
            anchor.X,
            anchor.Y,
            carriageBaseZ - actuators.GetCoordinate(actuatorId));
    }

    private static Point3D ToPoint3D(DeltaToolPose pose) =>
        new(pose.XMillimeters, pose.YMillimeters, pose.ZMillimeters);

    private static double GetDeltaReach(DeltaRobotProfile profile) =>
        profile.BaseRadiusMillimeters * 1.2;

    private void RenderDroneFrame(int index)
    {
        if (droneSnapshot is null)
        {
            return;
        }

        droneFrameIndex = Math.Clamp(index, 0, droneSnapshot.FrameCount - 1);
        DroneTimeline.Value = droneFrameIndex;

        var frame = droneSnapshot.Frames[droneFrameIndex];
        droneViewportPresenter.Present(new SchematicViewportScene(
            CreateDroneCamera(droneSnapshot.Profile),
            [
                CreateDroneWorkspaceModel(droneSnapshot.Profile),
                CreateDronePathModel(droneSnapshot),
                CreateDroneModel(frame)
            ]));

        var status = RobotFramePresenter.Create(
            frame,
            droneFrameIndex,
            droneSnapshot.FrameCount,
            droneSnapshot.TotalDuration);
        DroneStateText.Text = status.State;
        DronePoseText.Text = status.PrimaryPose;
        DroneYawText.Text = RobotFramePresenter.FormatDroneAttitude(frame);
        DroneCommandText.Text = status.Command;
        DroneTimeText.Text = status.Time;
        DroneTimeline.Status = status.Footer;
        DroneMovementExplanationText.Text = status.MovementExplanation;
    }

    private PerspectiveCamera CreateDroneCamera(DroneProfile profile)
    {
        var width = profile.MaximumXMillimeters - profile.MinimumXMillimeters;
        var depth = profile.MaximumYMillimeters - profile.MinimumYMillimeters;
        var height = profile.MaximumZMillimeters - profile.MinimumZMillimeters;
        var diagonal = Math.Sqrt((width * width) + (depth * depth) + (height * height));

        return OrbitCameraFactory.Create(new OrbitCameraSettings(
            Target: new Point3D(width / 2, depth / 2, height / 2),
            AzimuthDegrees: droneAzimuthDegrees,
            ElevationDegrees: droneElevationDegrees,
            Distance: diagonal * 1.85 * droneZoomMultiplier,
            FieldOfView: 42,
            NearPlaneDistance: 1,
            FarPlaneDistance: diagonal * 8));
    }

    private static Model3DGroup CreateDroneWorkspaceModel(DroneProfile profile)
    {
        var group = new Model3DGroup();
        var min = new Point3D(profile.MinimumXMillimeters, profile.MinimumYMillimeters, profile.MinimumZMillimeters);
        var max = new Point3D(profile.MaximumXMillimeters, profile.MaximumYMillimeters, profile.MaximumZMillimeters);
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

    private Model3DGroup CreateDronePathModel(DronePlaybackSnapshot playbackSnapshot)
    {
        var group = new Model3DGroup();
        var pathColor = Color.FromArgb(220, 45, 212, 191);

        for (var index = 1; index <= droneFrameIndex; index++)
        {
            var previous = ToPoint3D(playbackSnapshot.Frames[index - 1].Pose);
            var current = ToPoint3D(playbackSnapshot.Frames[index].Pose);

            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                previous,
                current,
                thickness: 4,
                pathColor));
        }

        return group;
    }

    private static Model3DGroup CreateDroneModel(DronePlaybackFrame frame)
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

        group.Children.Add(MeshModelFactory.CreateOrientedBox(back, front, thickness: 8, Color.FromRgb(59, 130, 246)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(left, rightPoint, thickness: 8, Color.FromRgb(34, 197, 94)));
        group.Children.Add(MeshModelFactory.CreateCube(center, size: 26, Color.FromRgb(147, 197, 253)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(center, center + (forward * 74), thickness: 5, Color.FromRgb(250, 204, 21)));

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

    private void RenderIndustrialArmFrame(int index)
    {
        if (industrialArmSnapshot is null)
        {
            return;
        }

        industrialArmFrameIndex = Math.Clamp(index, 0, industrialArmSnapshot.FrameCount - 1);
        IndustrialArmTimeline.Value = industrialArmFrameIndex;
        var frame = industrialArmSnapshot.Frames[industrialArmFrameIndex];

        industrialArmViewportPresenter.Present(new SchematicViewportScene(
            CreateIndustrialArmCamera(industrialArmSnapshot.Profile),
            [
                CreateIndustrialArmWorkspaceModel(industrialArmSnapshot.Profile),
                CreateIndustrialArmPathModel(industrialArmSnapshot),
                CreateIndustrialArmRobotModel(industrialArmSnapshot.Profile, frame)
            ],
            ambientColor: Color.FromRgb(96, 106, 128)));

        var status = RobotFramePresenter.Create(
            frame,
            industrialArmFrameIndex,
            industrialArmSnapshot.FrameCount,
            industrialArmSnapshot.TotalDuration);
        IndustrialArmStateText.Text = status.State;
        IndustrialArmJointsText.Text = status.PrimaryPose;
        IndustrialArmToolText.Text = RobotFramePresenter.FormatIndustrialArmToolPose(frame);
        IndustrialArmCommandText.Text = status.Command;
        IndustrialArmTimeText.Text = status.Time;
        IndustrialArmTimeline.Status = status.Footer;
        IndustrialArmMovementExplanationText.Text = status.MovementExplanation;
    }

    private PerspectiveCamera CreateIndustrialArmCamera(IndustrialArmRobotProfile profile)
    {
        var reach = GetIndustrialArmReach(profile);
        return OrbitCameraFactory.Create(new OrbitCameraSettings(
            Target: new Point3D(0, 0, profile.BaseHeightMillimeters * 0.8),
            AzimuthDegrees: industrialArmAzimuthDegrees,
            ElevationDegrees: industrialArmElevationDegrees,
            Distance: reach * 2.3 * industrialArmZoomMultiplier,
            FieldOfView: 42,
            NearPlaneDistance: 1,
            FarPlaneDistance: reach * 10));
    }

    private static Model3DGroup CreateIndustrialArmWorkspaceModel(IndustrialArmRobotProfile profile)
    {
        var reach = GetIndustrialArmReach(profile);
        return MeshModelFactory.CreatePlanarWorkspace(
            reach,
            gridSpacing: 80,
            floorZ: -12,
            gridThickness: 1.8,
            ringThickness: 3.5,
            Color.FromArgb(90, 51, 65, 85),
            Color.FromArgb(175, 96, 165, 250),
            Color.FromRgb(148, 163, 184));
    }

    private Model3DGroup CreateIndustrialArmPathModel(IndustrialArmPlaybackSnapshot snapshot)
    {
        var group = new Model3DGroup();
        for (var index = 1; index <= industrialArmFrameIndex; index++)
        {
            group.Children.Add(MeshModelFactory.CreateOrientedBox(
                ToPoint3D(snapshot.Frames[index - 1].ToolPose),
                ToPoint3D(snapshot.Frames[index].ToolPose),
                thickness: 5,
                Color.FromArgb(220, 45, 212, 191)));
        }

        return group;
    }

    private static Model3DGroup CreateIndustrialArmRobotModel(
        IndustrialArmRobotProfile profile,
        IndustrialArmPlaybackFrame frame)
    {
        var yaw = DegreesToRadians(frame.Joints.J1Degrees);
        var shoulderAngle = DegreesToRadians(frame.Joints.J2Degrees);
        var elbowAngle = shoulderAngle + DegreesToRadians(frame.Joints.J3Degrees);
        var wristAngle = elbowAngle + DegreesToRadians(frame.Joints.J5Degrees);
        var shoulder = new Point3D(0, 0, profile.BaseHeightMillimeters);
        var elbow = CreateIndustrialArmPoint(shoulder, profile.UpperArmLengthMillimeters, yaw, shoulderAngle);
        var wristRoll = CreateIndustrialArmPoint(elbow, profile.ForearmLengthMillimeters, yaw, elbowAngle);
        var tool = CreateIndustrialArmPoint(wristRoll, profile.WristLengthMillimeters, yaw, wristAngle);
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
        var sideAxis = new Vector3D(-Math.Sin(yaw) * Math.Cos(roll), Math.Cos(yaw) * Math.Cos(roll), Math.Sin(roll));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(tool, tool + (toolAxis * 58), 6, Color.FromRgb(248, 113, 113)));
        group.Children.Add(MeshModelFactory.CreateOrientedBox(tool - (sideAxis * 24), tool + (sideAxis * 24), 5, Color.FromRgb(192, 132, 252)));

        return group;
    }

    private static Point3D CreateIndustrialArmPoint(
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

    private static double GetIndustrialArmReach(IndustrialArmRobotProfile profile) =>
        profile.BaseHeightMillimeters +
        profile.UpperArmLengthMillimeters +
        profile.ForearmLengthMillimeters +
        profile.WristLengthMillimeters;
}
