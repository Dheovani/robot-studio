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
    private void DifferentialDriveCanvas_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!IsLoaded || differentialDriveSnapshot is null)
        {
            return;
        }

        RenderDifferentialDriveFrame(differentialDriveFrameIndex);
    }

    private void DifferentialDriveCanvas_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        e.Handled = true;
        ZoomActiveCamera(e.Delta > 0 ? -0.12 : 0.12);
    }

    private void DifferentialDriveTimelineSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || differentialDriveSnapshot is null)
        {
            return;
        }

        RenderDifferentialDriveFrame((int)Math.Round(e.NewValue));
    }

    private void DifferentialDrivePreviousFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderDifferentialDriveFrame(differentialDriveFrameIndex - 1);
    }

    private void DifferentialDriveNextFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderDifferentialDriveFrame(differentialDriveFrameIndex + 1);
    }

    private void ScaraViewport_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!IsLoaded || scaraSnapshot is null)
        {
            return;
        }

        RenderScaraFrame(scaraFrameIndex);
    }

    private void ScaraViewport_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        scaraOrbitInteraction.BeginDrag(ScaraViewportHost, ScaraViewport, e);
    }

    private void ScaraViewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        scaraOrbitInteraction.EndDrag(ScaraViewportHost, e);
    }

    private void ScaraViewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!scaraOrbitInteraction.TryGetDragDelta(ScaraViewport, e, out var deltaX, out var deltaY))
        {
            return;
        }

        scaraAzimuthDegrees = OrbitCameraFactory.NormalizeDegrees(scaraAzimuthDegrees - (deltaX * 0.35));
        scaraElevationDegrees = Math.Clamp(scaraElevationDegrees + (deltaY * 0.25), 5, 85);
        RenderScaraFrame(scaraFrameIndex);
    }

    private void ScaraViewport_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        e.Handled = true;
        ZoomActiveCamera(e.Delta > 0 ? -0.12 : 0.12);
    }

    private void ScaraTimelineSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || scaraSnapshot is null)
        {
            return;
        }

        RenderScaraFrame((int)Math.Round(e.NewValue));
    }

    private void ScaraPreviousFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderScaraFrame(scaraFrameIndex - 1);
    }

    private void ScaraNextFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderScaraFrame(scaraFrameIndex + 1);
    }

    private void SimpleArmViewport_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!IsLoaded || simpleArmSnapshot is null)
        {
            return;
        }

        RenderSimpleArmFrame(simpleArmFrameIndex);
    }

    private void SimpleArmViewport_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        simpleArmOrbitInteraction.BeginDrag(SimpleArmViewportHost, SimpleArmViewport, e);
    }

    private void SimpleArmViewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        simpleArmOrbitInteraction.EndDrag(SimpleArmViewportHost, e);
    }

    private void SimpleArmViewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!simpleArmOrbitInteraction.TryGetDragDelta(SimpleArmViewport, e, out var deltaX, out var deltaY))
        {
            return;
        }

        simpleArmAzimuthDegrees = OrbitCameraFactory.NormalizeDegrees(simpleArmAzimuthDegrees - (deltaX * 0.35));
        simpleArmElevationDegrees = Math.Clamp(simpleArmElevationDegrees + (deltaY * 0.25), 5, 85);
        RenderSimpleArmFrame(simpleArmFrameIndex);
    }

    private void SimpleArmViewport_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        e.Handled = true;
        ZoomActiveCamera(e.Delta > 0 ? -0.12 : 0.12);
    }

    private void SimpleArmTimelineSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || simpleArmSnapshot is null)
        {
            return;
        }

        RenderSimpleArmFrame((int)Math.Round(e.NewValue));
    }

    private void SimpleArmPreviousFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderSimpleArmFrame(simpleArmFrameIndex - 1);
    }

    private void SimpleArmNextFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderSimpleArmFrame(simpleArmFrameIndex + 1);
    }

    private void DeltaViewport_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!IsLoaded || deltaSnapshot is null)
        {
            return;
        }

        RenderDeltaFrame(deltaFrameIndex);
    }

    private void DeltaViewport_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        deltaOrbitInteraction.BeginDrag(DeltaViewportHost, DeltaViewport, e);
    }

    private void DeltaViewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        deltaOrbitInteraction.EndDrag(DeltaViewportHost, e);
    }

    private void DeltaViewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!deltaOrbitInteraction.TryGetDragDelta(DeltaViewport, e, out var deltaX, out var deltaY))
        {
            return;
        }

        deltaAzimuthDegrees = OrbitCameraFactory.NormalizeDegrees(deltaAzimuthDegrees - (deltaX * 0.35));
        deltaElevationDegrees = Math.Clamp(deltaElevationDegrees + (deltaY * 0.25), 5, 85);
        RenderDeltaFrame(deltaFrameIndex);
    }

    private void DeltaViewport_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        e.Handled = true;
        ZoomActiveCamera(e.Delta > 0 ? -0.12 : 0.12);
    }

    private void DeltaTimelineSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || deltaSnapshot is null)
        {
            return;
        }

        RenderDeltaFrame((int)Math.Round(e.NewValue));
    }

    private void DeltaPreviousFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderDeltaFrame(deltaFrameIndex - 1);
    }

    private void DeltaNextFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderDeltaFrame(deltaFrameIndex + 1);
    }

    private void DroneViewport_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!IsLoaded || droneSnapshot is null)
        {
            return;
        }

        RenderDroneFrame(droneFrameIndex);
    }

    private void DroneViewport_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        droneOrbitInteraction.BeginDrag(DroneViewportHost, DroneViewport, e);
    }

    private void DroneViewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        droneOrbitInteraction.EndDrag(DroneViewportHost, e);
    }

    private void DroneViewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!droneOrbitInteraction.TryGetDragDelta(DroneViewport, e, out var deltaX, out var deltaY))
        {
            return;
        }

        droneAzimuthDegrees = OrbitCameraFactory.NormalizeDegrees(droneAzimuthDegrees - (deltaX * 0.35));
        droneElevationDegrees = Math.Clamp(droneElevationDegrees + (deltaY * 0.25), 5, 85);
        RenderDroneFrame(droneFrameIndex);
    }

    private void DroneViewport_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        e.Handled = true;
        ZoomActiveCamera(e.Delta > 0 ? -0.12 : 0.12);
    }

    private void DroneTimelineSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || droneSnapshot is null)
        {
            return;
        }

        RenderDroneFrame((int)Math.Round(e.NewValue));
    }

    private void DronePreviousFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderDroneFrame(droneFrameIndex - 1);
    }

    private void DroneNextFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderDroneFrame(droneFrameIndex + 1);
    }

    private void IndustrialArmViewport_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (IsLoaded && industrialArmSnapshot is not null)
        {
            RenderIndustrialArmFrame(industrialArmFrameIndex);
        }
    }

    private void IndustrialArmViewport_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e) =>
        industrialArmOrbitInteraction.BeginDrag(IndustrialArmViewportHost, IndustrialArmViewport, e);

    private void IndustrialArmViewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e) =>
        industrialArmOrbitInteraction.EndDrag(IndustrialArmViewportHost, e);

    private void IndustrialArmViewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!industrialArmOrbitInteraction.TryGetDragDelta(
                IndustrialArmViewport,
                e,
                out var deltaX,
                out var deltaY))
        {
            return;
        }

        industrialArmAzimuthDegrees = OrbitCameraFactory.NormalizeDegrees(
            industrialArmAzimuthDegrees - (deltaX * 0.35));
        industrialArmElevationDegrees = Math.Clamp(industrialArmElevationDegrees + (deltaY * 0.25), 5, 85);
        RenderIndustrialArmFrame(industrialArmFrameIndex);
    }

    private void IndustrialArmViewport_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        e.Handled = true;
        ZoomActiveCamera(e.Delta > 0 ? -0.12 : 0.12);
    }

    private void IndustrialArmTimelineSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (IsLoaded && industrialArmSnapshot is not null)
        {
            RenderIndustrialArmFrame((int)Math.Round(e.NewValue));
        }
    }

    private void IndustrialArmPreviousFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderIndustrialArmFrame(industrialArmFrameIndex - 1);
    }

    private void IndustrialArmNextFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderIndustrialArmFrame(industrialArmFrameIndex + 1);
    }

    private void StateChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!IsLoaded || snapshot is null)
        {
            return;
        }

        UpdateStateChart();
    }

    private void RobotCardsScrollViewer_SizeChanged(
        object sender,
        SizeChangedEventArgs e) =>
        UpdateRobotCardColumns(e.NewSize.Width);

    private void ScriptTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (ReferenceEquals(sender, ScriptEditorTextBox))
        {
            RefreshScriptEditorGutter();
        }

        if (!IsLoaded)
        {
            return;
        }

        RefreshGCodeExplanations();
        pendingScriptValidationKind = GetScriptViewerKind(sender);
        scriptValidationTimer.Stop();
        SetScriptStatus(
            pendingScriptValidationKind.Value,
            languageService.GetText("Script.Checking"),
            Color.FromRgb(148, 163, 184));
        scriptValidationTimer.Start();
    }

    private void ScriptValidationTimer_Tick(object? sender, EventArgs e)
    {
        scriptValidationTimer.Stop();
        var viewerKind = pendingScriptValidationKind ?? activeViewerKind;
        pendingScriptValidationKind = null;

        if (TryValidateScript(viewerKind, out var message))
        {
            SetScriptStatus(viewerKind, message, Color.FromRgb(74, 222, 128));
            return;
        }

        SetScriptStatus(viewerKind, message, Color.FromRgb(248, 113, 113));
    }

    private RobotViewerKind GetScriptViewerKind(object sender)
    {
        if (ReferenceEquals(sender, DifferentialDriveScriptTextBox))
        {
            return RobotViewerKind.DifferentialDriveTwoDimensional;
        }

        if (ReferenceEquals(sender, ScaraScriptTextBox))
        {
            return RobotViewerKind.ScaraThreeDimensional;
        }

        if (ReferenceEquals(sender, SimpleArmScriptTextBox))
        {
            return RobotViewerKind.SimpleArmThreeDimensional;
        }

        if (ReferenceEquals(sender, DeltaScriptTextBox))
        {
            return RobotViewerKind.DeltaThreeDimensional;
        }

        if (ReferenceEquals(sender, DroneScriptTextBox))
        {
            return RobotViewerKind.DroneThreeDimensional;
        }

        if (ReferenceEquals(sender, IndustrialArmScriptTextBox))
        {
            return RobotViewerKind.IndustrialArmThreeDimensional;
        }

        return activeViewerKind is RobotViewerKind.XYPlotterTwoDimensional
            ? RobotViewerKind.XYPlotterTwoDimensional
            : RobotViewerKind.CartesianThreeDimensional;
    }

    private bool TryValidateScript(
        RobotViewerKind viewerKind,
        out string message)
    {
        switch (viewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                return TryCreateDifferentialDriveSnapshotFromScript(
                    DifferentialDriveScriptTextBox.Text,
                    out _,
                    out message);
            case RobotViewerKind.ScaraThreeDimensional:
                return TryCreateScaraSnapshotFromScript(ScaraScriptTextBox.Text, out _, out message);
            case RobotViewerKind.SimpleArmThreeDimensional:
                return TryCreateSimpleArmSnapshotFromScript(SimpleArmScriptTextBox.Text, out _, out message);
            case RobotViewerKind.DeltaThreeDimensional:
                return TryCreateDeltaSnapshotFromScript(DeltaScriptTextBox.Text, out _, out message);
            case RobotViewerKind.DroneThreeDimensional:
                return TryCreateDroneSnapshotFromScript(DroneScriptTextBox.Text, out _, out message);
            case RobotViewerKind.IndustrialArmThreeDimensional:
                return TryCreateIndustrialArmSnapshotFromScript(
                    IndustrialArmScriptTextBox.Text,
                    out _,
                    out message);
            case RobotViewerKind.CartesianThreeDimensional:
            case RobotViewerKind.XYPlotterTwoDimensional:
                return TryCreateSnapshotFromScript(ScriptEditorTextBox.Text, out _, out message);
            default:
                message = "The active robot viewer does not support script validation.";
                return false;
        }
    }

    private void ScriptEditorTextBox_ScrollChanged(
        object sender,
        ScrollChangedEventArgs e) =>
        ScriptEditorGutterScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);

    private void OverlayCheckBox_Changed(
        object sender,
        RoutedEventArgs e)
    {
        if (!IsLoaded || snapshot is null)
        {
            return;
        }

        RenderFrame(currentFrameIndex);
    }

    private void ExecuteCommandButton_Click(
        object sender,
        RoutedEventArgs e) =>
        ExecuteConsoleCommand();

    private void CommandConsoleTextBox_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        e.Handled = true;
        ExecuteConsoleCommand();
    }

    private void RobotViewport_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        cartesianOrbitInteraction.BeginDrag(RobotViewportHost, RobotViewport, e);
    }

    private void RobotViewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        cartesianOrbitInteraction.EndDrag(RobotViewportHost, e);
    }

    private void RobotViewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!cartesianOrbitInteraction.TryGetDragDelta(RobotViewport, e, out var deltaX, out var deltaY))
        {
            return;
        }

        SetCameraControls(
            azimuth: OrbitCameraFactory.NormalizeDegrees(azimuthDegrees - (deltaX * 0.35)),
            elevation: Math.Clamp(elevationDegrees + (deltaY * 0.25), 5, 85),
            zoom: zoomMultiplier);
    }

    private void RobotViewport_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        e.Handled = true;
        ZoomActiveCamera(e.Delta > 0 ? -0.08 : 0.08);
    }

    private void SetCameraControls(
        double azimuth,
        double elevation,
        double zoom)
    {
        azimuthDegrees = azimuth;
        elevationDegrees = elevation;
        zoomMultiplier = zoom;

        AzimuthSlider.Value = azimuthDegrees;
        ElevationSlider.Value = elevationDegrees;
        ZoomSlider.Value = zoomMultiplier;

        if (IsLoaded && snapshot is not null)
        {
            ApplyCamera();
        }
    }

    private void ApplyCamera()
    {
        if (snapshot is null)
        {
            return;
        }

        RobotViewport.Camera = CreateCamera(
            snapshot.Viewport,
            azimuthDegrees,
            elevationDegrees,
            baseCameraDistanceMillimeters * zoomMultiplier);
    }
}
