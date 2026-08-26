using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using RobotStudio.Desktop.Rendering.SceneComposers;
using RobotStudio.Desktop.Viewers;
using RobotStudio.Domain.Mobile;
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
            (height - (padding * 2)) / workspaceHeight) * differentialDriveZoomMultiplier;
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
        scaraViewportPresenter.Present(ScaraSchematicSceneComposer.Compose(
            scaraSnapshot,
            scaraFrameIndex,
            scaraAzimuthDegrees,
            scaraElevationDegrees,
            scaraZoomMultiplier));
        var status = RobotFramePresenter.Create(frame, scaraFrameIndex, scaraSnapshot.FrameCount, scaraSnapshot.TotalDuration);
        ScaraStateText.Text = status.State;
        ScaraJointsText.Text = status.PrimaryPose;
        ScaraToolText.Text = RobotFramePresenter.FormatScaraToolPose(frame);
        ScaraCommandText.Text = status.Command;
        ScaraTimeText.Text = status.Time;
        ScaraTimeline.Status = status.Footer;
        ScaraMovementExplanationText.Text = status.MovementExplanation;
    }

    private void RenderSimpleArmFrame(int index)
    {
        if (simpleArmSnapshot is null)
        {
            return;
        }

        simpleArmFrameIndex = Math.Clamp(index, 0, simpleArmSnapshot.FrameCount - 1);
        SimpleArmTimeline.Value = simpleArmFrameIndex;
        var frame = simpleArmSnapshot.Frames[simpleArmFrameIndex];
        simpleArmViewportPresenter.Present(SimpleArmSchematicSceneComposer.Compose(
            simpleArmSnapshot,
            simpleArmFrameIndex,
            simpleArmAzimuthDegrees,
            simpleArmElevationDegrees,
            simpleArmZoomMultiplier));
        var status = RobotFramePresenter.Create(frame, simpleArmFrameIndex, simpleArmSnapshot.FrameCount, simpleArmSnapshot.TotalDuration);
        SimpleArmStateText.Text = status.State;
        SimpleArmJointsText.Text = status.PrimaryPose;
        SimpleArmToolText.Text = RobotFramePresenter.FormatSimpleArmToolPose(frame);
        SimpleArmCommandText.Text = status.Command;
        SimpleArmTimeText.Text = status.Time;
        SimpleArmTimeline.Status = status.Footer;
        SimpleArmMovementExplanationText.Text = status.MovementExplanation;
    }

    private void RenderDeltaFrame(int index)
    {
        if (deltaSnapshot is null)
        {
            return;
        }

        deltaFrameIndex = Math.Clamp(index, 0, deltaSnapshot.FrameCount - 1);
        DeltaTimeline.Value = deltaFrameIndex;
        var frame = deltaSnapshot.Frames[deltaFrameIndex];
        deltaViewportPresenter.Present(DeltaSchematicSceneComposer.Compose(
            deltaSnapshot,
            deltaFrameIndex,
            deltaAzimuthDegrees,
            deltaElevationDegrees,
            deltaZoomMultiplier));
        var status = RobotFramePresenter.Create(frame, deltaFrameIndex, deltaSnapshot.FrameCount, deltaSnapshot.TotalDuration);
        DeltaStateText.Text = status.State;
        DeltaActuatorsText.Text = status.PrimaryPose;
        DeltaToolText.Text = RobotFramePresenter.FormatDeltaToolPose(frame);
        DeltaCommandText.Text = status.Command;
        DeltaTimeText.Text = status.Time;
        DeltaTimeline.Status = status.Footer;
        DeltaMovementExplanationText.Text = status.MovementExplanation;
    }

    private void RenderDroneFrame(int index)
    {
        if (droneSnapshot is null)
        {
            return;
        }

        droneFrameIndex = Math.Clamp(index, 0, droneSnapshot.FrameCount - 1);
        DroneTimeline.Value = droneFrameIndex;
        var frame = droneSnapshot.Frames[droneFrameIndex];
        droneViewportPresenter.Present(DroneSchematicSceneComposer.Compose(
            droneSnapshot,
            droneFrameIndex,
            droneAzimuthDegrees,
            droneElevationDegrees,
            droneZoomMultiplier));
        var status = RobotFramePresenter.Create(frame, droneFrameIndex, droneSnapshot.FrameCount, droneSnapshot.TotalDuration);
        DroneStateText.Text = status.State;
        DronePoseText.Text = status.PrimaryPose;
        DroneYawText.Text = RobotFramePresenter.FormatDroneAttitude(frame);
        DroneCommandText.Text = status.Command;
        DroneTimeText.Text = status.Time;
        DroneTimeline.Status = status.Footer;
        DroneMovementExplanationText.Text = status.MovementExplanation;
    }

    private void RenderIndustrialArmFrame(int index)
    {
        if (industrialArmSnapshot is null)
        {
            return;
        }

        industrialArmFrameIndex = Math.Clamp(index, 0, industrialArmSnapshot.FrameCount - 1);
        IndustrialArmTimeline.Value = industrialArmFrameIndex;
        var frame = industrialArmSnapshot.Frames[industrialArmFrameIndex];
        industrialArmViewportPresenter.Present(IndustrialArmSchematicSceneComposer.Compose(
            industrialArmSnapshot,
            industrialArmFrameIndex,
            industrialArmAzimuthDegrees,
            industrialArmElevationDegrees,
            industrialArmZoomMultiplier));
        var status = RobotFramePresenter.Create(frame, industrialArmFrameIndex, industrialArmSnapshot.FrameCount, industrialArmSnapshot.TotalDuration);
        IndustrialArmStateText.Text = status.State;
        IndustrialArmJointsText.Text = status.PrimaryPose;
        IndustrialArmToolText.Text = RobotFramePresenter.FormatIndustrialArmToolPose(frame);
        IndustrialArmCommandText.Text = status.Command;
        IndustrialArmTimeText.Text = status.Time;
        IndustrialArmTimeline.Status = status.Footer;
        IndustrialArmMovementExplanationText.Text = status.MovementExplanation;
    }
}
