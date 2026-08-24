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
    private void TimelineSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || snapshot is null)
        {
            return;
        }

        RenderFrame((int)Math.Round(e.NewValue));
    }

    private void PlaybackTimer_Tick(
        object? sender,
        EventArgs e)
    {
        if (activeViewerKind == RobotViewerKind.DifferentialDriveTwoDimensional)
        {
            if (differentialDriveSnapshot is null)
            {
                return;
            }

            var nextDifferentialDriveFrame = differentialDriveFrameIndex + 1;
            if (nextDifferentialDriveFrame >= differentialDriveSnapshot.FrameCount)
            {
                nextDifferentialDriveFrame = 0;
            }

            RenderDifferentialDriveFrame(nextDifferentialDriveFrame);
            return;
        }

        if (activeViewerKind == RobotViewerKind.ScaraThreeDimensional)
        {
            if (scaraSnapshot is null)
            {
                return;
            }

            var nextScaraFrame = scaraFrameIndex + 1;
            if (nextScaraFrame >= scaraSnapshot.FrameCount)
            {
                nextScaraFrame = 0;
            }

            RenderScaraFrame(nextScaraFrame);
            return;
        }

        if (activeViewerKind == RobotViewerKind.SimpleArmThreeDimensional)
        {
            if (simpleArmSnapshot is null)
            {
                return;
            }

            var nextSimpleArmFrame = simpleArmFrameIndex + 1;
            if (nextSimpleArmFrame >= simpleArmSnapshot.FrameCount)
            {
                nextSimpleArmFrame = 0;
            }

            RenderSimpleArmFrame(nextSimpleArmFrame);
            return;
        }

        if (activeViewerKind == RobotViewerKind.DeltaThreeDimensional)
        {
            if (deltaSnapshot is null)
            {
                return;
            }

            var nextDeltaFrame = deltaFrameIndex + 1;
            if (nextDeltaFrame >= deltaSnapshot.FrameCount)
            {
                nextDeltaFrame = 0;
            }

            RenderDeltaFrame(nextDeltaFrame);
            return;
        }

        if (activeViewerKind == RobotViewerKind.DroneThreeDimensional)
        {
            if (droneSnapshot is null)
            {
                return;
            }

            var nextDroneFrame = droneFrameIndex + 1;
            if (nextDroneFrame >= droneSnapshot.FrameCount)
            {
                nextDroneFrame = 0;
            }

            RenderDroneFrame(nextDroneFrame);
            return;
        }

        if (activeViewerKind == RobotViewerKind.IndustrialArmThreeDimensional)
        {
            if (industrialArmSnapshot is null)
            {
                return;
            }

            var nextIndustrialArmFrame = industrialArmFrameIndex + 1;
            if (nextIndustrialArmFrame >= industrialArmSnapshot.FrameCount)
            {
                nextIndustrialArmFrame = 0;
            }

            RenderIndustrialArmFrame(nextIndustrialArmFrame);
            return;
        }

        if (snapshot is null)
        {
            return;
        }

        var nextFrame = currentFrameIndex + 1;
        if (nextFrame >= snapshot.SceneFrameCount)
        {
            nextFrame = 0;
        }

        RenderFrame(nextFrame);
    }

    private void RenderFrame(int index)
    {
        if (snapshot is null)
        {
            return;
        }

        currentFrameIndex = Math.Clamp(index, 0, snapshot.SceneFrameCount - 1);
        TimelineSlider.Value = currentFrameIndex;

        var sceneFrame = snapshot.SceneFrames[currentFrameIndex];

        RobotViewport.Children.Clear();
        ApplyCamera();

        var sceneRoot = SceneLightingFactory.CreateDefault(ambientColor: Color.FromRgb(92, 105, 130));

        if (ShowGridCheckBox.IsChecked == true)
        {
            sceneRoot.Children.Add(CreateGridModel(snapshot.WorkspaceBounds));
        }

        if (ShowGlobalAxesCheckBox.IsChecked == true)
        {
            sceneRoot.Children.Add(CreateGlobalAxesModel(snapshot.WorkspaceBounds));
        }

        if (ShowPlannedPathCheckBox.IsChecked == true)
        {
            sceneRoot.Children.Add(CreatePlannedPathModel(snapshot));
        }

        if (ShowStartEndMarkersCheckBox.IsChecked == true)
        {
            sceneRoot.Children.Add(CreateStartEndMarkersModel(snapshot));
        }

        foreach (CartesianScenePrimitive primitive in sceneFrame.Primitives.Where(IsPrimitiveVisible))
        {
            sceneRoot.Children.Add(CreateModel(primitive));
        }

        RobotViewport.Children.Add(new ModelVisual3D { Content = sceneRoot });

        if (ShowAxisLabelsCheckBox.IsChecked == true &&
            RobotViewport.Camera is PerspectiveCamera camera)
        {
            foreach (var label in CreateAxisLabelVisuals(snapshot.WorkspaceBounds, camera))
            {
                RobotViewport.Children.Add(label);
            }
        }

        StatusText.Text =
            $"Frame {currentFrameIndex + 1}/{snapshot.SceneFrameCount} | " +
            $"t={sceneFrame.Time.TotalSeconds:0.###}s | {sceneFrame.State}";
        UpdateStatePanel(sceneFrame);
        UpdateScriptLineIndicator(sceneFrame);
        UpdateMovementExplanation(sceneFrame);
        UpdatePositionChart();
        UpdateVelocityChart();
        UpdateVelocityComparisonChart();
        UpdateAccelerationChart();
        UpdateDistanceChart();
        UpdateStateChart();
    }

    private void CameraSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || snapshot is null)
        {
            return;
        }

        azimuthDegrees = AzimuthSlider.Value;
        elevationDegrees = ElevationSlider.Value;
        zoomMultiplier = ZoomSlider.Value;
        ApplyCamera();
    }

    private void IsoViewButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SetCameraControls(azimuth: -45, elevation: 35, zoom: 1);

    private void FrontViewButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SetCameraControls(azimuth: -90, elevation: 20, zoom: 1);

    private void SideViewButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SetCameraControls(azimuth: 0, elevation: 20, zoom: 1);

    private void TopViewButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SetCameraControls(azimuth: -90, elevation: 80, zoom: 1.1);

    private void ResetCameraButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SetCameraControls(azimuth: -45, elevation: 35, zoom: 1);

    private void SessionRecoveryPanel_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is FrameworkElement panel && !sessionRecoveryPanels.Contains(panel))
        {
            sessionRecoveryPanels.Add(panel);
        }

        UpdateSessionRecoveryControls();
    }

    private void SessionHomeButton_Click(
        object sender,
        RoutedEventArgs e) =>
        ExecuteSessionRecoveryCommand("HOME");

    private void SessionResetFaultButton_Click(
        object sender,
        RoutedEventArgs e) =>
        ExecuteSessionRecoveryCommand("RESET");

    private void BackToSelectionButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        CartesianViewerView.Visibility = Visibility.Collapsed;
        DifferentialDriveViewerView.Visibility = Visibility.Collapsed;
        ScaraViewerView.Visibility = Visibility.Collapsed;
        SimpleArmViewerView.Visibility = Visibility.Collapsed;
        DeltaViewerView.Visibility = Visibility.Collapsed;
        DroneViewerView.Visibility = Visibility.Collapsed;
        IndustrialArmViewerView.Visibility = Visibility.Collapsed;
        RobotSelectionView.Visibility = Visibility.Visible;
    }

    private void OpenRobotButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button { Tag: RobotTemplate template })
        {
            OpenRobot(template);
        }
    }

    private void ValidateScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (TryCreateSnapshotFromScript(ScriptEditorTextBox.Text, out _, out var message))
        {
            SetScriptStatus(message, Color.FromRgb(74, 222, 128));
            return;
        }

        SetScriptStatus(message, Color.FromRgb(248, 113, 113));
    }

    private void SimulateScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryCreateSnapshotFromScript(
            ScriptEditorTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
        {
            SetScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        StopPlayback();
        snapshot = nextSnapshot;
        InitializeTimelineForSnapshot();
        RenderFrame(index: 0);
        SetScriptStatus(message, Color.FromRgb(74, 222, 128));
    }

    private void ManualHomeButton_Click(
        object sender,
        RoutedEventArgs e) =>
        AppendManualCommandAndSimulate(GetCartesianHomeCommandText());

    private void JogPositiveXButton_Click(
        object sender,
        RoutedEventArgs e) =>
        Jog(AxisId.X, direction: 1);

    private void JogNegativeXButton_Click(
        object sender,
        RoutedEventArgs e) =>
        Jog(AxisId.X, direction: -1);

    private void JogPositiveYButton_Click(
        object sender,
        RoutedEventArgs e) =>
        Jog(AxisId.Y, direction: 1);

    private void JogNegativeYButton_Click(
        object sender,
        RoutedEventArgs e) =>
        Jog(AxisId.Y, direction: -1);

    private void JogPositiveZButton_Click(
        object sender,
        RoutedEventArgs e) =>
        Jog(AxisId.Z, direction: 1);

    private void JogNegativeZButton_Click(
        object sender,
        RoutedEventArgs e) =>
        Jog(AxisId.Z, direction: -1);

    private void StopPlaybackButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        SetManualControlStatus("Playback stopped.", Color.FromRgb(147, 197, 253));
    }

    private void PreviousFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderFrame(currentFrameIndex - 1);
    }

    private void NextFrameButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        RenderFrame(currentFrameIndex + 1);
    }

    private void PlaybackSpeedComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        ApplyPlaybackSpeed();
    }

    private void TimelineMarkerListBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (sender is not ListBox { SelectedItem: TimelineMarker marker })
        {
            return;
        }

        StopPlayback();
        RenderFrame(marker.FrameIndex);
    }

    private void PositionChartCanvas_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!IsLoaded || snapshot is null)
        {
            return;
        }

        UpdatePositionChart();
    }

    private void VelocityChartCanvas_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!IsLoaded || snapshot is null)
        {
            return;
        }

        UpdateVelocityChart();
    }

    private void VelocityComparisonChartCanvas_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!IsLoaded || snapshot is null)
        {
            return;
        }

        UpdateVelocityComparisonChart();
    }

    private void AccelerationChartCanvas_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!IsLoaded || snapshot is null)
        {
            return;
        }

        UpdateAccelerationChart();
    }

    private void DistanceChartCanvas_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        if (!IsLoaded || snapshot is null)
        {
            return;
        }

        UpdateDistanceChart();
    }


}
