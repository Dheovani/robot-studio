using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using RobotStudio.Desktop.Examples;
using RobotStudio.Desktop.Rendering;
using RobotStudio.Desktop.Robots;
using RobotStudio.Desktop.Scripting;
using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Scripting;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop;

public partial class MainWindow : Window
{
    private const double GridSpacingMillimeters = 25;
    private const double GridLineThicknessMillimeters = 1.2;
    private const double AxisLineThicknessMillimeters = 4;
    private const double PathPointSizeMillimeters = 5;
    private const double StartEndMarkerSizeMillimeters = 14;
    private const double AxisLabelOffsetMillimeters = 24;
    private const double AxisLabelWidthMillimeters = 22;
    private const double AxisLabelHeightMillimeters = 16;
    private const double ChartPaddingLeft = 28;
    private const double ChartPaddingTop = 12;
    private const double ChartPaddingRight = 10;
    private const double ChartPaddingBottom = 24;
    private const double StateChartPaddingLeft = 78;
    private const double StateChartRowGap = 4;
    private const double RobotCardGap = 18;
    private const double RobotCardMinimumWidth = 280;
    private const int MaximumPathPointCount = 140;

    private static readonly SolidColorBrush RobotCardBackgroundBrush =
        new(Color.FromRgb(15, 23, 42));

    private static readonly SolidColorBrush RobotCardHighlightBackgroundBrush =
        new(Color.FromRgb(17, 32, 55));

    private static readonly SolidColorBrush RobotCardPlannedBorderBrush =
        new(Color.FromRgb(51, 65, 85));

    private static readonly SolidColorBrush RobotCardPlannedHighlightBorderBrush =
        new(Color.FromRgb(71, 85, 105));

    private static readonly SolidColorBrush RobotCardAvailableBorderBrush =
        new(Color.FromRgb(37, 99, 235));

    private static readonly SolidColorBrush RobotCardAvailableHighlightBorderBrush =
        new(Color.FromRgb(96, 165, 250));

    private readonly DispatcherTimer playbackTimer;
    private readonly TimeSpan basePlaybackInterval = TimeSpan.FromMilliseconds(120);
    private readonly IRobotScriptDialect scriptDialect = new RobotScriptParser();
    private CartesianRobotProfile profile = CreateCartesianProfile();
    private XYPlotterProfile? xyPlotterProfile;
    private CartesianPosition initialPosition = new(X: 40, Y: 30, Z: 20);
    private RobotViewerKind activeViewerKind = RobotViewerKind.CartesianThreeDimensional;
    private CartesianPlaybackSnapshot? snapshot;
    private DifferentialDrivePlaybackSnapshot? differentialDriveSnapshot;
    private ScaraPlaybackSnapshot? scaraSnapshot;
    private SimpleArmPlaybackSnapshot? simpleArmSnapshot;
    private int currentFrameIndex;
    private int differentialDriveFrameIndex;
    private int scaraFrameIndex;
    private int simpleArmFrameIndex;
    private bool isPlaying;
    private double baseCameraDistanceMillimeters;
    private double azimuthDegrees = -45;
    private double elevationDegrees = 35;
    private double zoomMultiplier = 1;
    private bool isRotatingCamera;
    private Point lastMousePosition;
    private double scaraAzimuthDegrees = -45;
    private double scaraElevationDegrees = 32;
    private double scaraZoomMultiplier = 1.8;
    private bool isRotatingScaraCamera;
    private Point lastScaraMousePosition;
    private double simpleArmAzimuthDegrees = -45;
    private double simpleArmElevationDegrees = 30;
    private double simpleArmZoomMultiplier = 2.15;
    private bool isRotatingSimpleArmCamera;
    private Point lastSimpleArmMousePosition;

    private sealed class TimelineMarker
    {
        public TimelineMarker(
            string label,
            int frameIndex)
        {
            Label = label;
            FrameIndex = frameIndex;
        }

        public string Label { get; }

        public int FrameIndex { get; }

        public override string ToString() => Label;
    }

    private sealed record VelocitySample(
        TimeSpan Time,
        double VelocityMillimetersPerSecond);

    private sealed record ScalarSample(
        TimeSpan Time,
        double Value);

    private sealed record StateSegment(
        RobotState State,
        TimeSpan Start,
        TimeSpan End);

    public MainWindow()
    {
        InitializeComponent();

        playbackTimer = new DispatcherTimer
        {
            Interval = basePlaybackInterval
        };
        playbackTimer.Tick += PlaybackTimer_Tick;

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        ScriptEditorTextBox.Text = GetDefaultExampleScript(RobotViewerKind.CartesianThreeDimensional);
        RefreshScriptEditorGutter();
        BuildRobotSelectionCards();
    }

    private void PlayPauseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        isPlaying = !isPlaying;
        PlayPauseButton.Content = isPlaying ? "Pause" : "Play";
        DifferentialDrivePlayPauseButton.Content = isPlaying ? "Pause" : "Play";
        ScaraPlayPauseButton.Content = isPlaying ? "Pause" : "Play";
        SimpleArmPlayPauseButton.Content = isPlaying ? "Pause" : "Play";

        if (isPlaying)
        {
            playbackTimer.Start();
        }
        else
        {
            playbackTimer.Stop();
        }
    }

    private void ResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (snapshot is null)
        {
            return;
        }

        playbackTimer.Stop();
        isPlaying = false;
        PlayPauseButton.Content = "Play";
        RenderFrame(index: 0);
    }

    private void DifferentialDriveResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (differentialDriveSnapshot is null)
        {
            return;
        }

        StopPlayback();
        RenderDifferentialDriveFrame(index: 0);
    }

    private void ScaraResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (scaraSnapshot is null)
        {
            return;
        }

        StopPlayback();
        RenderScaraFrame(index: 0);
    }

    private void SimpleArmResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (simpleArmSnapshot is null)
        {
            return;
        }

        StopPlayback();
        RenderSimpleArmFrame(index: 0);
    }

    private void ValidateDifferentialDriveScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (TryCreateDifferentialDriveSnapshotFromScript(
            DifferentialDriveScriptTextBox.Text,
            out _,
            out var message))
        {
            SetDifferentialDriveScriptStatus(message, Color.FromRgb(74, 222, 128));
            return;
        }

        SetDifferentialDriveScriptStatus(message, Color.FromRgb(248, 113, 113));
    }

    private void SimulateDifferentialDriveScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryCreateDifferentialDriveSnapshotFromScript(
            DifferentialDriveScriptTextBox.Text,
            out var nextSnapshot,
            out var message))
        {
            SetDifferentialDriveScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        if (nextSnapshot is null)
        {
            SetDifferentialDriveScriptStatus("Script did not produce a playback snapshot.", Color.FromRgb(248, 113, 113));
            return;
        }

        StopPlayback();
        differentialDriveSnapshot = nextSnapshot;
        DifferentialDriveTimelineSlider.Maximum = differentialDriveSnapshot.FrameCount - 1;
        DifferentialDriveTimelineSlider.TickFrequency = 1;
        RenderDifferentialDriveFrame(index: 0);
        SetDifferentialDriveScriptStatus(message, Color.FromRgb(74, 222, 128));
    }

    private void ValidateScaraScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (TryCreateScaraSnapshotFromScript(ScaraScriptTextBox.Text, out _, out var message))
        {
            SetScaraScriptStatus(message, Color.FromRgb(74, 222, 128));
            return;
        }

        SetScaraScriptStatus(message, Color.FromRgb(248, 113, 113));
    }

    private void SimulateScaraScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryCreateScaraSnapshotFromScript(ScaraScriptTextBox.Text, out var nextSnapshot, out var message))
        {
            SetScaraScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        if (nextSnapshot is null)
        {
            SetScaraScriptStatus("Script did not produce a SCARA playback snapshot.", Color.FromRgb(248, 113, 113));
            return;
        }

        StopPlayback();
        scaraSnapshot = nextSnapshot;
        ScaraTimelineSlider.Maximum = scaraSnapshot.FrameCount - 1;
        ScaraTimelineSlider.TickFrequency = 1;
        RenderScaraFrame(index: 0);
        SetScaraScriptStatus(message, Color.FromRgb(74, 222, 128));
    }

    private void LoadScaraExampleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        ScaraScriptTextBox.Text = GetDefaultExampleScript(RobotViewerKind.ScaraThreeDimensional);
        scaraSnapshot = CreateScaraSnapshot(ScaraScriptTextBox.Text);
        ScaraTimelineSlider.Maximum = scaraSnapshot.FrameCount - 1;
        ScaraTimelineSlider.TickFrequency = 1;
        RenderScaraFrame(index: 0);
        SetScaraScriptStatus("Loaded the default SCARA example.", Color.FromRgb(74, 222, 128));
    }

    private void ValidateSimpleArmScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (TryCreateSimpleArmSnapshotFromScript(SimpleArmScriptTextBox.Text, out _, out var message))
        {
            SetSimpleArmScriptStatus(message, Color.FromRgb(74, 222, 128));
            return;
        }

        SetSimpleArmScriptStatus(message, Color.FromRgb(248, 113, 113));
    }

    private void SimulateSimpleArmScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryCreateSimpleArmSnapshotFromScript(SimpleArmScriptTextBox.Text, out var nextSnapshot, out var message))
        {
            SetSimpleArmScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        if (nextSnapshot is null)
        {
            SetSimpleArmScriptStatus("Script did not produce a simple arm playback snapshot.", Color.FromRgb(248, 113, 113));
            return;
        }

        StopPlayback();
        simpleArmSnapshot = nextSnapshot;
        SimpleArmTimelineSlider.Maximum = simpleArmSnapshot.FrameCount - 1;
        SimpleArmTimelineSlider.TickFrequency = 1;
        RenderSimpleArmFrame(index: 0);
        SetSimpleArmScriptStatus(message, Color.FromRgb(74, 222, 128));
    }

    private void LoadSimpleArmExampleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        SimpleArmScriptTextBox.Text = GetDefaultExampleScript(RobotViewerKind.SimpleArmThreeDimensional);
        simpleArmSnapshot = CreateSimpleArmSnapshot(SimpleArmScriptTextBox.Text);
        SimpleArmTimelineSlider.Maximum = simpleArmSnapshot.FrameCount - 1;
        SimpleArmTimelineSlider.TickFrequency = 1;
        RenderSimpleArmFrame(index: 0);
        SetSimpleArmScriptStatus("Loaded the default articulated arm example.", Color.FromRgb(74, 222, 128));
    }

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

        var sceneRoot = new Model3DGroup();
        sceneRoot.Children.Add(new AmbientLight(Color.FromRgb(92, 105, 130)));
        sceneRoot.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -2)));

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

    private void BackToSelectionButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        CartesianViewerView.Visibility = Visibility.Collapsed;
        DifferentialDriveViewerView.Visibility = Visibility.Collapsed;
        ScaraViewerView.Visibility = Visibility.Collapsed;
        SimpleArmViewerView.Visibility = Visibility.Collapsed;
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
        if (!TryCreateSnapshotFromScript(ScriptEditorTextBox.Text, out var nextSnapshot, out var message))
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
        AppendManualCommandAndSimulate("HOME");

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
        isRotatingScaraCamera = true;
        lastScaraMousePosition = e.GetPosition(ScaraViewport);
        ScaraViewport.CaptureMouse();
        ScaraViewport.Cursor = Cursors.SizeAll;
    }

    private void ScaraViewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        isRotatingScaraCamera = false;
        ScaraViewport.ReleaseMouseCapture();
        ScaraViewport.Cursor = Cursors.Hand;
    }

    private void ScaraViewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!isRotatingScaraCamera)
        {
            return;
        }

        var currentPosition = e.GetPosition(ScaraViewport);
        var deltaX = currentPosition.X - lastScaraMousePosition.X;
        var deltaY = currentPosition.Y - lastScaraMousePosition.Y;
        lastScaraMousePosition = currentPosition;

        scaraAzimuthDegrees = OrbitCameraFactory.NormalizeDegrees(scaraAzimuthDegrees - (deltaX * 0.35));
        scaraElevationDegrees = Math.Clamp(scaraElevationDegrees + (deltaY * 0.25), 5, 85);
        RenderScaraFrame(scaraFrameIndex);
    }

    private void ScaraViewport_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        var zoomDelta = e.Delta > 0 ? -0.12 : 0.12;
        scaraZoomMultiplier = Math.Clamp(scaraZoomMultiplier + zoomDelta, 0.55, 4);
        RenderScaraFrame(scaraFrameIndex);
    }

    private void ScaraZoomOutButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        scaraZoomMultiplier = Math.Clamp(scaraZoomMultiplier + 0.18, 0.55, 4);
        RenderScaraFrame(scaraFrameIndex);
    }

    private void ScaraZoomInButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        scaraZoomMultiplier = Math.Clamp(scaraZoomMultiplier - 0.18, 0.55, 4);
        RenderScaraFrame(scaraFrameIndex);
    }

    private void ScaraResetCameraButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        scaraAzimuthDegrees = -45;
        scaraElevationDegrees = 32;
        scaraZoomMultiplier = 1.8;
        RenderScaraFrame(scaraFrameIndex);
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
        isRotatingSimpleArmCamera = true;
        lastSimpleArmMousePosition = e.GetPosition(SimpleArmViewport);
        SimpleArmViewport.CaptureMouse();
        SimpleArmViewport.Cursor = Cursors.SizeAll;
    }

    private void SimpleArmViewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        isRotatingSimpleArmCamera = false;
        SimpleArmViewport.ReleaseMouseCapture();
        SimpleArmViewport.Cursor = Cursors.Hand;
    }

    private void SimpleArmViewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!isRotatingSimpleArmCamera)
        {
            return;
        }

        var currentPosition = e.GetPosition(SimpleArmViewport);
        var deltaX = currentPosition.X - lastSimpleArmMousePosition.X;
        var deltaY = currentPosition.Y - lastSimpleArmMousePosition.Y;
        lastSimpleArmMousePosition = currentPosition;

        simpleArmAzimuthDegrees = OrbitCameraFactory.NormalizeDegrees(simpleArmAzimuthDegrees - (deltaX * 0.35));
        simpleArmElevationDegrees = Math.Clamp(simpleArmElevationDegrees + (deltaY * 0.25), 5, 85);
        RenderSimpleArmFrame(simpleArmFrameIndex);
    }

    private void SimpleArmViewport_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        var zoomDelta = e.Delta > 0 ? -0.12 : 0.12;
        simpleArmZoomMultiplier = Math.Clamp(simpleArmZoomMultiplier + zoomDelta, 0.55, 4);
        RenderSimpleArmFrame(simpleArmFrameIndex);
    }

    private void SimpleArmZoomOutButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        simpleArmZoomMultiplier = Math.Clamp(simpleArmZoomMultiplier + 0.18, 0.55, 4);
        RenderSimpleArmFrame(simpleArmFrameIndex);
    }

    private void SimpleArmZoomInButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        simpleArmZoomMultiplier = Math.Clamp(simpleArmZoomMultiplier - 0.18, 0.55, 4);
        RenderSimpleArmFrame(simpleArmFrameIndex);
    }

    private void SimpleArmResetCameraButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        simpleArmAzimuthDegrees = -45;
        simpleArmElevationDegrees = 30;
        simpleArmZoomMultiplier = 2.15;
        RenderSimpleArmFrame(simpleArmFrameIndex);
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

    private void ScriptEditorTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e) =>
        RefreshScriptEditorGutter();

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
        isRotatingCamera = true;
        lastMousePosition = e.GetPosition(RobotViewport);
        RobotViewport.CaptureMouse();
        RobotViewport.Cursor = Cursors.SizeAll;
    }

    private void RobotViewport_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        isRotatingCamera = false;
        RobotViewport.ReleaseMouseCapture();
        RobotViewport.Cursor = Cursors.Hand;
    }

    private void RobotViewport_MouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (!isRotatingCamera)
        {
            return;
        }

        var currentPosition = e.GetPosition(RobotViewport);
        var deltaX = currentPosition.X - lastMousePosition.X;
        var deltaY = currentPosition.Y - lastMousePosition.Y;
        lastMousePosition = currentPosition;

        SetCameraControls(
            azimuth: OrbitCameraFactory.NormalizeDegrees(azimuthDegrees - (deltaX * 0.35)),
            elevation: Math.Clamp(elevationDegrees + (deltaY * 0.25), 5, 85),
            zoom: zoomMultiplier);
    }

    private void RobotViewport_MouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        var zoomDelta = e.Delta > 0 ? -0.08 : 0.08;
        SetCameraControls(
            azimuth: azimuthDegrees,
            elevation: elevationDegrees,
            zoom: Math.Clamp(zoomMultiplier + zoomDelta, ZoomSlider.Minimum, ZoomSlider.Maximum));
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

    private void UpdateStatePanel(CartesianSceneFrame sceneFrame)
    {
        if (snapshot is null)
        {
            return;
        }

        var pose = snapshot.Poses[currentFrameIndex];
        StateValueText.Text = sceneFrame.State.ToString();
        PositionValueText.Text =
            $"X={pose.ToolCenterPoint.XMillimeters:0.###}, " +
            $"Y={pose.ToolCenterPoint.YMillimeters:0.###}, " +
            $"Z={pose.ToolCenterPoint.ZMillimeters:0.###} mm";
        CommandValueText.Text = sceneFrame.CommandName ?? "simulation";
        SourceValueText.Text = sceneFrame.CommandSource is null
            ? "-"
            : $"line {sceneFrame.CommandSource.LineNumber}";
        TimeValueText.Text = $"{sceneFrame.Time.TotalSeconds:0.###} / {snapshot.TotalDuration.TotalSeconds:0.###} s";
        FramesValueText.Text = $"{currentFrameIndex + 1} / {snapshot.SceneFrameCount}";
    }

    private void UpdateScriptLineIndicator(CartesianSceneFrame sceneFrame)
    {
        if (sceneFrame.CommandSource is null)
        {
            CurrentScriptLineText.Text = "Current script line: -";
            ScriptEditorTextBox.Select(0, 0);
            return;
        }

        CurrentScriptLineText.Text =
            $"Current script line: {sceneFrame.CommandSource.LineNumber} | {sceneFrame.CommandSource.Text}";

        var selection = GetLineSelection(ScriptEditorTextBox.Text, sceneFrame.CommandSource.LineNumber);
        ScriptEditorTextBox.Select(selection.Start, selection.Length);
    }

    private CartesianPlaybackSnapshot CreateSnapshot(string script)
    {
        var commands = scriptDialect.Parse(script);
        ValidateCommandSequence(commands);

        var context = SimulationContext.Create(profile, initialPosition);
        var result = new RobotSimulator().Execute(context, commands);

        return new CartesianPlaybackSnapshotBuilder()
            .Build(profile, result, TimeSpan.FromMilliseconds(100));
    }

    private DifferentialDrivePlaybackSnapshot CreateDifferentialDriveSnapshot(string script)
    {
        var profile = CreateDifferentialDriveProfile();
        var commands = scriptDialect.Parse(script);
        ValidateDifferentialDriveCommandSequence(commands, profile);

        var context = DifferentialDriveSimulationContext.Create(
            profile,
            new DifferentialDrivePose(X: 60, Y: 50, HeadingDegrees: 0));
        var result = new DifferentialDriveSimulator().Execute(context, commands);

        return new DifferentialDrivePlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private ScaraPlaybackSnapshot CreateScaraSnapshot(string script)
    {
        var profile = CreateScaraProfile();
        var commands = scriptDialect.Parse(script);
        ValidateScaraCommandSequence(commands, profile);

        var context = ScaraSimulationContext.Create(
            profile,
            new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0));
        var result = new ScaraSimulator().Execute(context, commands);

        return new ScaraPlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private SimpleArmPlaybackSnapshot CreateSimpleArmSnapshot(string script)
    {
        var profile = CreateSimpleArmProfile();
        var commands = scriptDialect.Parse(script);
        ValidateSimpleArmCommandSequence(commands, profile);

        var context = SimpleArmSimulationContext.Create(
            profile,
            new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0));
        var result = new SimpleArmSimulator().Execute(context, commands);

        return new SimpleArmPlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private void BuildRobotSelectionCards()
    {
        RobotCardsPanel.Children.Clear();

        foreach (var template in RobotCatalog.Templates)
        {
            RobotCardsPanel.Children.Add(CreateRobotCard(template));
        }

        UpdateRobotCardColumns(RobotCardsScrollViewer.ActualWidth);
    }

    private UIElement CreateRobotCard(RobotTemplate template)
    {
        var canOpen = RobotCatalog.CanOpen(template);
        var card = new Border
        {
            MinWidth = RobotCardMinimumWidth,
            MinHeight = 392,
            Margin = new Thickness(0, 0, RobotCardGap, RobotCardGap),
            Padding = new Thickness(20),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            FocusVisualStyle = null,
            Focusable = true,
            Cursor = canOpen ? Cursors.Hand : Cursors.Arrow,
            BorderBrush = canOpen
                ? RobotCardAvailableBorderBrush
                : RobotCardPlannedBorderBrush,
            BorderThickness = new Thickness(1),
            Background = RobotCardBackgroundBrush,
            CornerRadius = new CornerRadius(8)
        };
        card.MouseEnter += (_, _) => ApplyRobotCardVisualState(card, template, isHighlighted: true);
        card.MouseLeave += (_, _) => ApplyRobotCardVisualState(
            card,
            template,
            isHighlighted: card.IsKeyboardFocusWithin);
        card.GotKeyboardFocus += (_, _) => ApplyRobotCardVisualState(card, template, isHighlighted: true);
        card.LostKeyboardFocus += (_, _) => ApplyRobotCardVisualState(card, template, isHighlighted: false);
        card.KeyDown += (_, e) =>
        {
            if (!canOpen || e.Key is not (Key.Enter or Key.Space))
            {
                return;
            }

            e.Handled = true;
            OpenRobot(template);
        };

        var content = new Grid();
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        card.Child = content;

        var topContent = new StackPanel();
        Grid.SetRow(topContent, 0);
        content.Children.Add(topContent);

        topContent.Children.Add(new TextBlock
        {
            Text = template.Name,
            Foreground = new SolidColorBrush(Color.FromRgb(249, 250, 251)),
            FontSize = 21,
            FontWeight = FontWeights.SemiBold
        });

        topContent.Children.Add(new TextBlock
        {
            Text = template.Family.Name,
            Margin = new Thickness(0, 2, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 13
        });

        topContent.Children.Add(CreateStatusBadge(template.Status));
        topContent.Children.Add(CreateComplexityBadge(template.Complexity));

        var middleContent = new StackPanel
        {
            Margin = new Thickness(0, 14, 0, 12)
        };
        Grid.SetRow(middleContent, 1);
        content.Children.Add(middleContent);

        middleContent.Children.Add(new TextBlock
        {
            Text = template.Description,
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            LineHeight = 18,
            TextWrapping = TextWrapping.Wrap
        });

        middleContent.Children.Add(new TextBlock
        {
            Text = "Capabilities",
            Margin = new Thickness(0, 16, 0, 8),
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold
        });

        middleContent.Children.Add(CreateCapabilityTags(template.Capabilities));

        var button = new Button
        {
            Height = 36,
            Margin = new Thickness(0, 12, 0, 0),
            Content = canOpen
                ? "Open Robot"
                : template.Status.ToString(),
            IsEnabled = canOpen,
            Tag = template
        };
        button.Click += OpenRobotButton_Click;
        Grid.SetRow(button, 2);
        content.Children.Add(button);

        return card;
    }

    private static void ApplyRobotCardVisualState(
        Border card,
        RobotTemplate template,
        bool isHighlighted)
    {
        var canOpen = RobotCatalog.CanOpen(template);
        card.Background = isHighlighted
            ? RobotCardHighlightBackgroundBrush
            : RobotCardBackgroundBrush;
        card.BorderBrush = canOpen
            ? isHighlighted
                ? RobotCardAvailableHighlightBorderBrush
                : RobotCardAvailableBorderBrush
            : isHighlighted
                ? RobotCardPlannedHighlightBorderBrush
                : RobotCardPlannedBorderBrush;
        card.BorderThickness = isHighlighted
            ? new Thickness(1.5)
            : new Thickness(1);
    }

    private void UpdateRobotCardColumns(double availableWidth)
    {
        if (availableWidth <= 0)
        {
            return;
        }

        var columns = availableWidth >= 1020
            ? 3
            : availableWidth >= 660
                ? 2
                : 1;

        RobotCardsPanel.Columns = columns;
    }

    private static Border CreateStatusBadge(RobotAvailabilityStatus status) =>
        new()
        {
            Margin = new Thickness(0, 14, 0, 0),
            Padding = new Thickness(9, 4, 9, 4),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = GetStatusBackgroundBrush(status),
            BorderBrush = GetStatusBorderBrush(status),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(999),
            Child = new TextBlock
            {
                Text = status.ToString(),
                Foreground = GetStatusBrush(status),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            }
        };

    private static Border CreateComplexityBadge(RobotComplexityLevel complexity) =>
        new()
        {
            Margin = new Thickness(0, 8, 0, 0),
            Padding = new Thickness(9, 4, 9, 4),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(999),
            Child = new TextBlock
            {
                Text = complexity.ToString(),
                Foreground = new SolidColorBrush(Color.FromRgb(191, 219, 254)),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            }
        };

    private static WrapPanel CreateCapabilityTags(IReadOnlyList<RobotCapability> capabilities)
    {
        var panel = new WrapPanel();

        foreach (var capability in capabilities)
        {
            panel.Children.Add(new Border
            {
                Margin = new Thickness(0, 0, 6, 6),
                Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(999),
                Child = new TextBlock
                {
                    Text = FormatCapability(capability),
                    Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                    FontSize = 12
                }
            });
        }

        return panel;
    }

    private void OpenRobot(RobotTemplate template)
    {
        if (!RobotCatalog.CanOpen(template))
        {
            return;
        }

        ConfigureActiveViewer(template.Viewer.Kind);
        RobotSelectionView.Visibility = Visibility.Collapsed;

        if (template.Viewer.Kind == RobotViewerKind.DifferentialDriveTwoDimensional)
        {
            DifferentialDriveViewerView.Visibility = Visibility.Visible;
            EnsureDifferentialDriveSnapshot();
            RenderDifferentialDriveFrame(index: 0);
            return;
        }

        if (template.Viewer.Kind == RobotViewerKind.ScaraThreeDimensional)
        {
            ScaraViewerView.Visibility = Visibility.Visible;
            EnsureScaraSnapshot();
            RenderScaraFrame(index: 0);
            return;
        }

        if (template.Viewer.Kind == RobotViewerKind.SimpleArmThreeDimensional)
        {
            SimpleArmViewerView.Visibility = Visibility.Visible;
            EnsureSimpleArmSnapshot();
            RenderSimpleArmFrame(index: 0);
            return;
        }

        CartesianViewerView.Visibility = Visibility.Visible;
        EnsureCartesianSnapshot();
        RenderFrame(index: 0);
    }

    private void ConfigureActiveViewer(RobotViewerKind viewerKind)
    {
        activeViewerKind = viewerKind;
        snapshot = null;
        differentialDriveSnapshot = null;
        scaraSnapshot = null;
        simpleArmSnapshot = null;
        currentFrameIndex = 0;
        differentialDriveFrameIndex = 0;
        scaraFrameIndex = 0;
        simpleArmFrameIndex = 0;
        CommandHistoryListBox.Items.Clear();

        switch (viewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                ConfigureDifferentialDriveViewer();
                break;

            case RobotViewerKind.ScaraThreeDimensional:
                ConfigureScaraViewer();
                break;

            case RobotViewerKind.SimpleArmThreeDimensional:
                ConfigureSimpleArmViewer();
                break;

            case RobotViewerKind.XYPlotterTwoDimensional:
                ConfigureXYPlotterViewer();
                break;

            case RobotViewerKind.CartesianThreeDimensional:
                ConfigureCartesianViewer();
                break;

            default:
                ConfigureCartesianViewer();
                break;
        }

        RefreshScriptEditorGutter();
    }

    private void ConfigureCartesianViewer()
    {
        profile = CreateCartesianProfile();
        xyPlotterProfile = null;
        initialPosition = new CartesianPosition(X: 40, Y: 30, Z: 20);
        ViewerSubtitleText.Text = "Cartesian robot simulation";
        ScriptEditorTextBox.Text = GetDefaultExampleScript(RobotViewerKind.CartesianThreeDimensional);
        CommandConsoleTextBox.Text = "MOVE X=100 Y=50 Z=20 SPEED=80";
        JogNegativeZButton.IsEnabled = true;
        JogPositiveZButton.IsEnabled = true;
        ManualControlStatusText.Text = "Manual actions append DSL commands and resimulate the robot.";
    }

    private void ConfigureXYPlotterViewer()
    {
        xyPlotterProfile = CreateXYPlotterProfile();
        profile = xyPlotterProfile.ToCartesianProfile();
        initialPosition = new CartesianPosition(X: 40, Y: 30, Z: 0);
        ViewerSubtitleText.Text = "XY plotter simulation";
        ScriptEditorTextBox.Text = GetDefaultExampleScript(RobotViewerKind.XYPlotterTwoDimensional);
        CommandConsoleTextBox.Text = "MOVE X=100 Y=50 Z=0 SPEED=80";
        JogNegativeZButton.IsEnabled = false;
        JogPositiveZButton.IsEnabled = false;
        ManualControlStatusText.Text = "XY Plotter uses X/Y jog commands. Z remains fixed at 0 mm.";
    }

    private void ConfigureDifferentialDriveViewer()
    {
        DifferentialDriveScriptTextBox.Text = GetDefaultExampleScript(RobotViewerKind.DifferentialDriveTwoDimensional);
        SetDifferentialDriveScriptStatus(
            "Edit DRIVE commands and simulate the mobile robot.",
            Color.FromRgb(148, 163, 184));
    }

    private void ConfigureScaraViewer()
    {
        ScaraScriptTextBox.Text = GetDefaultExampleScript(RobotViewerKind.ScaraThreeDimensional);
        SetScaraScriptStatus(
            "Edit SCARA joint commands and simulate the articulated robot.",
            Color.FromRgb(148, 163, 184));
    }

    private void ConfigureSimpleArmViewer()
    {
        SimpleArmScriptTextBox.Text = GetDefaultExampleScript(RobotViewerKind.SimpleArmThreeDimensional);
        SetSimpleArmScriptStatus(
            "Edit ARM joint commands and simulate the articulated arm.",
            Color.FromRgb(148, 163, 184));
    }

    private static string GetDefaultExampleScript(RobotViewerKind viewerKind) =>
        RobotExampleCatalog.GetDefaultFor(viewerKind).Script;

    private void EnsureCartesianSnapshot()
    {
        if (snapshot is not null)
        {
            return;
        }

        snapshot = CreateSnapshot(ScriptEditorTextBox.Text);
        InitializeTimelineForSnapshot();
    }

    private void EnsureDifferentialDriveSnapshot()
    {
        if (differentialDriveSnapshot is not null)
        {
            return;
        }

        differentialDriveSnapshot = CreateDifferentialDriveSnapshot(DifferentialDriveScriptTextBox.Text);
        DifferentialDriveTimelineSlider.Maximum = differentialDriveSnapshot.FrameCount - 1;
        DifferentialDriveTimelineSlider.TickFrequency = 1;
    }

    private void EnsureScaraSnapshot()
    {
        if (scaraSnapshot is not null)
        {
            return;
        }

        scaraSnapshot = CreateScaraSnapshot(ScaraScriptTextBox.Text);
        ScaraTimelineSlider.Maximum = scaraSnapshot.FrameCount - 1;
        ScaraTimelineSlider.TickFrequency = 1;
    }

    private void EnsureSimpleArmSnapshot()
    {
        if (simpleArmSnapshot is not null)
        {
            return;
        }

        simpleArmSnapshot = CreateSimpleArmSnapshot(SimpleArmScriptTextBox.Text);
        SimpleArmTimelineSlider.Maximum = simpleArmSnapshot.FrameCount - 1;
        SimpleArmTimelineSlider.TickFrequency = 1;
    }

    private void InitializeTimelineForSnapshot()
    {
        if (snapshot is null)
        {
            return;
        }

        baseCameraDistanceMillimeters = CalculateDistance(
            snapshot.Viewport.Target,
            snapshot.Viewport.CameraPosition);
        TimelineSlider.Maximum = snapshot.SceneFrameCount - 1;
        TimelineSlider.TickFrequency = 1;
        ApplyPlaybackSpeed();
        UpdateTimelineMarkers();
        SetCameraControls(
            azimuth: azimuthDegrees,
            elevation: elevationDegrees,
            zoom: zoomMultiplier);
    }

    private void StopPlayback()
    {
        playbackTimer.Stop();
        isPlaying = false;
        PlayPauseButton.Content = "Play";
        DifferentialDrivePlayPauseButton.Content = "Play";
        ScaraPlayPauseButton.Content = "Play";
        SimpleArmPlayPauseButton.Content = "Play";
    }

    private void ApplyPlaybackSpeed()
    {
        var speed = GetSelectedPlaybackSpeed();
        playbackTimer.Interval = TimeSpan.FromMilliseconds(basePlaybackInterval.TotalMilliseconds / speed);
    }

    private void Jog(AxisId axis, int direction)
    {
        if (activeViewerKind == RobotViewerKind.XYPlotterTwoDimensional &&
            axis == AxisId.Z)
        {
            SetManualControlStatus("XY Plotter does not expose a Z jog axis.", Color.FromRgb(248, 113, 113));
            return;
        }

        if (!TryReadManualInputs(out var stepMillimeters, out var speedMillimetersPerSecond))
        {
            return;
        }

        var currentPosition = GetCurrentToolPosition();
        var targetPosition = axis switch
        {
            AxisId.X => currentPosition with { X = currentPosition.X + (stepMillimeters * direction) },
            AxisId.Y => currentPosition with { Y = currentPosition.Y + (stepMillimeters * direction) },
            AxisId.Z => currentPosition with { Z = currentPosition.Z + (stepMillimeters * direction) },
            _ => currentPosition
        };
        var command =
            $"MOVE X={FormatNumber(targetPosition.X)} " +
            $"Y={FormatNumber(targetPosition.Y)} " +
            $"Z={FormatNumber(targetPosition.Z)} " +
            $"SPEED={FormatNumber(speedMillimetersPerSecond)}";

        AppendManualCommandAndSimulate(command);
    }

    private CartesianPosition GetCurrentToolPosition()
    {
        if (snapshot is null)
        {
            return new CartesianPosition(X: 40, Y: 30, Z: 20);
        }

        var pose = snapshot.Poses[currentFrameIndex];
        return new CartesianPosition(
            pose.ToolCenterPoint.XMillimeters,
            pose.ToolCenterPoint.YMillimeters,
            pose.ToolCenterPoint.ZMillimeters);
    }

    private bool TryReadManualInputs(
        out double stepMillimeters,
        out double speedMillimetersPerSecond)
    {
        if (!double.TryParse(
            ManualStepTextBox.Text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out stepMillimeters) ||
            stepMillimeters <= 0)
        {
            speedMillimetersPerSecond = 0;
            SetManualControlStatus("Step must be a positive number in millimeters.", Color.FromRgb(248, 113, 113));
            return false;
        }

        if (!double.TryParse(
            ManualSpeedTextBox.Text,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out speedMillimetersPerSecond) ||
            speedMillimetersPerSecond <= 0)
        {
            SetManualControlStatus("Speed must be a positive number in millimeters per second.", Color.FromRgb(248, 113, 113));
            return false;
        }

        return true;
    }

    private void AppendManualCommandAndSimulate(string commandText)
    {
        if (!AppendCommandAndSimulate(commandText, out var message))
        {
            SetManualControlStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        SetManualControlStatus($"Added command: {commandText}", Color.FromRgb(74, 222, 128));
    }

    private void ExecuteConsoleCommand()
    {
        var commandText = CommandConsoleTextBox.Text.Trim();
        if (commandText.Length == 0)
        {
            SetCommandConsoleStatus("Command cannot be empty.", Color.FromRgb(248, 113, 113));
            return;
        }

        if (!AppendCommandAndSimulate(commandText, out var message))
        {
            AddCommandHistoryEntry($"> {commandText} | rejected");
            SetCommandConsoleStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        AddCommandHistoryEntry($"> {commandText}");
        CommandConsoleTextBox.SelectAll();
        SetCommandConsoleStatus("Command executed and appended to the script.", Color.FromRgb(74, 222, 128));
    }

    private bool AppendCommandAndSimulate(
        string commandText,
        out string message)
    {
        var currentScript = ScriptEditorTextBox.Text.TrimEnd();
        var nextScript = currentScript.Length == 0
            ? commandText
            : $"{currentScript}{Environment.NewLine}{commandText}";

        if (!TryCreateSnapshotFromScript(nextScript, out var nextSnapshot, out message))
        {
            return false;
        }

        if (nextSnapshot is null)
        {
            message = "Command did not produce a playback snapshot.";
            return false;
        }

        StopPlayback();
        ScriptEditorTextBox.Text = nextScript;
        RefreshScriptEditorGutter();
        snapshot = nextSnapshot;
        InitializeTimelineForSnapshot();
        RenderFrame(nextSnapshot.SceneFrameCount - 1);
        SetScriptStatus(message, Color.FromRgb(74, 222, 128));
        return true;
    }

    private void SetManualControlStatus(string message, Color color)
    {
        ManualControlStatusText.Text = message;
        ManualControlStatusText.Foreground = new SolidColorBrush(color);
    }

    private void SetCommandConsoleStatus(
        string message,
        Color color)
    {
        CommandConsoleStatusText.Text = message;
        CommandConsoleStatusText.Foreground = new SolidColorBrush(color);
    }

    private void AddCommandHistoryEntry(string text)
    {
        CommandHistoryListBox.Items.Add(text);
        CommandHistoryListBox.ScrollIntoView(text);
    }

    private void UpdateTimelineMarkers()
    {
        CommandMarkersListBox.Items.Clear();
        StateMarkersListBox.Items.Clear();

        if (snapshot is null)
        {
            return;
        }

        AddCommandTimelineMarkers(snapshot);
        AddStateTimelineMarkers(snapshot);
    }

    private void AddCommandTimelineMarkers(CartesianPlaybackSnapshot snapshot)
    {
        int? previousCommandIndex = null;

        for (var index = 0; index < snapshot.SceneFrames.Count; index++)
        {
            var frame = snapshot.SceneFrames[index];
            if (frame.CommandIndex is null || frame.CommandIndex == previousCommandIndex)
            {
                continue;
            }

            previousCommandIndex = frame.CommandIndex;
            var sourceText = frame.CommandSource is null
                ? frame.CommandName ?? "command"
                : $"line {frame.CommandSource.LineNumber}: {frame.CommandSource.Text}";
            CommandMarkersListBox.Items.Add(new TimelineMarker(
                $"{FormatFrameMarker(index, frame.Time)} | {sourceText}",
                index));
        }
    }

    private void AddStateTimelineMarkers(CartesianPlaybackSnapshot snapshot)
    {
        RobotState? previousState = null;

        for (var index = 0; index < snapshot.SceneFrames.Count; index++)
        {
            var frame = snapshot.SceneFrames[index];
            if (frame.State == previousState)
            {
                continue;
            }

            previousState = frame.State;
            StateMarkersListBox.Items.Add(new TimelineMarker(
                $"{FormatFrameMarker(index, frame.Time)} | {frame.State}",
                index));
        }
    }

    private static string FormatFrameMarker(
        int frameIndex,
        TimeSpan time) =>
        $"#{frameIndex + 1} t={time.TotalSeconds:0.###}s";

    private void UpdatePositionChart()
    {
        PositionChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.Poses.Count == 0)
        {
            return;
        }

        var width = PositionChartCanvas.ActualWidth;
        var height = PositionChartCanvas.ActualHeight;
        if (width <= ChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        DrawPositionChartGrid(width, height);
        DrawPositionSeries(
            pose => pose.ToolCenterPoint.XMillimeters,
            profile.XAxis.MinimumMillimeters,
            profile.XAxis.MaximumMillimeters,
            Color.FromRgb(248, 113, 113),
            width,
            height);
        DrawPositionSeries(
            pose => pose.ToolCenterPoint.YMillimeters,
            profile.YAxis.MinimumMillimeters,
            profile.YAxis.MaximumMillimeters,
            Color.FromRgb(34, 197, 94),
            width,
            height);
        DrawPositionSeries(
            pose => pose.ToolCenterPoint.ZMillimeters,
            profile.ZAxis.MinimumMillimeters,
            profile.ZAxis.MaximumMillimeters,
            Color.FromRgb(96, 165, 250),
            width,
            height);
        DrawPositionChartCursor(width, height);
    }

    private void DrawPositionChartGrid(
        double width,
        double height)
    {
        var plotLeft = ChartPaddingLeft;
        var plotTop = ChartPaddingTop;
        var plotRight = width - ChartPaddingRight;
        var plotBottom = height - ChartPaddingBottom;
        var gridBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85));

        for (var index = 0; index <= 3; index++)
        {
            var y = plotTop + ((plotBottom - plotTop) * index / 3);
            PositionChartCanvas.Children.Add(new Line
            {
                X1 = plotLeft,
                X2 = plotRight,
                Y1 = y,
                Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1
            });
        }

        var positionLabel = new TextBlock
        {
            Text = "pos",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        PositionChartCanvas.Children.Add(positionLabel);
        Canvas.SetLeft(positionLabel, 4);
        Canvas.SetTop(positionLabel, 2);

        var timeLabel = new TextBlock
        {
            Text = "time",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        PositionChartCanvas.Children.Add(timeLabel);
        Canvas.SetLeft(timeLabel, plotRight - 28);
        Canvas.SetTop(timeLabel, plotBottom + 4);
    }

    private void DrawPositionSeries(
        Func<CartesianRobotPose, double> selectValue,
        double minimum,
        double maximum,
        Color color,
        double width,
        double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var points = new PointCollection();
        foreach (var pose in snapshot.Poses)
        {
            points.Add(ToChartPoint(
                pose.Time,
                selectValue(pose),
                minimum,
                maximum,
                width,
                height));
        }

        PositionChartCanvas.Children.Add(new Polyline
        {
            Points = points,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2
        });
    }

    private void DrawPositionChartCursor(
        double width,
        double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var sceneFrame = snapshot.SceneFrames[currentFrameIndex];
        var cursorX = ToChartX(sceneFrame.Time, width);
        PositionChartCanvas.Children.Add(new Line
        {
            X1 = cursorX,
            X2 = cursorX,
            Y1 = ChartPaddingTop,
            Y2 = height - ChartPaddingBottom,
            Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            StrokeThickness = 1.5
        });
    }

    private Point ToChartPoint(
        TimeSpan time,
        double value,
        double minimum,
        double maximum,
        double width,
        double height)
    {
        var normalizedValue = maximum <= minimum
            ? 0
            : Math.Clamp((value - minimum) / (maximum - minimum), 0, 1);

        return new Point(
            ToChartX(time, width),
            ChartPaddingTop + ((1 - normalizedValue) * (height - ChartPaddingTop - ChartPaddingBottom)));
    }

    private double ToChartX(
        TimeSpan time,
        double width)
    {
        if (snapshot is null || snapshot.TotalDuration <= TimeSpan.Zero)
        {
            return ChartPaddingLeft;
        }

        var normalizedTime = Math.Clamp(
            time.TotalSeconds / snapshot.TotalDuration.TotalSeconds,
            0,
            1);
        return ChartPaddingLeft + (normalizedTime * (width - ChartPaddingLeft - ChartPaddingRight));
    }

    private void UpdateVelocityChart()
    {
        VelocityChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.Poses.Count == 0)
        {
            return;
        }

        var width = VelocityChartCanvas.ActualWidth;
        var height = VelocityChartCanvas.ActualHeight;
        if (width <= ChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        var samples = CreateVelocitySamples(snapshot);
        var maximumVelocity = Math.Max(1, samples.Count == 0 ? 0 : samples.Max(sample => sample.VelocityMillimetersPerSecond));

        DrawVelocityChartGrid(width, height, maximumVelocity);
        if (samples.Count > 0)
        {
            DrawVelocitySeries(samples, maximumVelocity, width, height);
        }

        DrawVelocityChartCursor(width, height);
    }

    private static IReadOnlyList<VelocitySample> CreateVelocitySamples(CartesianPlaybackSnapshot snapshot)
    {
        var samples = new List<VelocitySample>();
        for (var index = 1; index < snapshot.Poses.Count; index++)
        {
            var previous = snapshot.Poses[index - 1];
            var current = snapshot.Poses[index];
            var elapsedSeconds = (current.Time - previous.Time).TotalSeconds;
            if (elapsedSeconds <= 0)
            {
                continue;
            }

            var distance = CalculateDistance(previous.ToolCenterPoint, current.ToolCenterPoint);
            samples.Add(new VelocitySample(
                current.Time,
                distance / elapsedSeconds));
        }

        return samples;
    }

    private void DrawVelocityChartGrid(
        double width,
        double height,
        double maximumVelocity)
    {
        var plotLeft = ChartPaddingLeft;
        var plotTop = ChartPaddingTop;
        var plotRight = width - ChartPaddingRight;
        var plotBottom = height - ChartPaddingBottom;
        var gridBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85));

        for (var index = 0; index <= 2; index++)
        {
            var y = plotTop + ((plotBottom - plotTop) * index / 2);
            VelocityChartCanvas.Children.Add(new Line
            {
                X1 = plotLeft,
                X2 = plotRight,
                Y1 = y,
                Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1
            });
        }

        var velocityLabel = new TextBlock
        {
            Text = $"{maximumVelocity:0.#}",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        VelocityChartCanvas.Children.Add(velocityLabel);
        Canvas.SetLeft(velocityLabel, 4);
        Canvas.SetTop(velocityLabel, 2);

        var zeroLabel = new TextBlock
        {
            Text = "0",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        VelocityChartCanvas.Children.Add(zeroLabel);
        Canvas.SetLeft(zeroLabel, 12);
        Canvas.SetTop(zeroLabel, plotBottom - 10);

        var timeLabel = new TextBlock
        {
            Text = "time",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        VelocityChartCanvas.Children.Add(timeLabel);
        Canvas.SetLeft(timeLabel, plotRight - 28);
        Canvas.SetTop(timeLabel, plotBottom + 4);
    }

    private void DrawVelocitySeries(
        IReadOnlyList<VelocitySample> samples,
        double maximumVelocity,
        double width,
        double height)
    {
        var points = new PointCollection();
        foreach (var sample in samples)
        {
            points.Add(ToVelocityChartPoint(sample, maximumVelocity, width, height));
        }

        VelocityChartCanvas.Children.Add(new Polyline
        {
            Points = points,
            Stroke = new SolidColorBrush(Color.FromRgb(250, 204, 21)),
            StrokeThickness = 2
        });
    }

    private void DrawVelocityChartCursor(
        double width,
        double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var sceneFrame = snapshot.SceneFrames[currentFrameIndex];
        var cursorX = ToChartX(sceneFrame.Time, width);
        VelocityChartCanvas.Children.Add(new Line
        {
            X1 = cursorX,
            X2 = cursorX,
            Y1 = ChartPaddingTop,
            Y2 = height - ChartPaddingBottom,
            Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            StrokeThickness = 1.5
        });
    }

    private Point ToVelocityChartPoint(
        VelocitySample sample,
        double maximumVelocity,
        double width,
        double height)
    {
        var normalizedVelocity = Math.Clamp(
            sample.VelocityMillimetersPerSecond / maximumVelocity,
            0,
            1);

        return new Point(
            ToChartX(sample.Time, width),
            ChartPaddingTop + ((1 - normalizedVelocity) * (height - ChartPaddingTop - ChartPaddingBottom)));
    }

    private void UpdateVelocityComparisonChart()
    {
        VelocityComparisonChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.SceneFrames.Count == 0)
        {
            return;
        }

        var width = VelocityComparisonChartCanvas.ActualWidth;
        var height = VelocityComparisonChartCanvas.ActualHeight;
        if (width <= ChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        var effectiveSamples = CreateVelocitySamples(snapshot)
            .Select(sample => new ScalarSample(sample.Time, sample.VelocityMillimetersPerSecond))
            .ToArray();
        var requestedSamples = CreateRequestedVelocitySamples(snapshot);
        var maximumVelocity = Math.Max(
            1,
            Math.Max(
                effectiveSamples.Length == 0 ? 0 : effectiveSamples.Max(sample => sample.Value),
                requestedSamples.Count == 0 ? 0 : requestedSamples.Max(sample => sample.Value)));

        DrawScalarChartGrid(
            VelocityComparisonChartCanvas,
            width,
            height,
            maximumVelocity,
            "mm/s");
        DrawScalarSeries(
            VelocityComparisonChartCanvas,
            requestedSamples,
            maximumVelocity,
            Color.FromRgb(56, 189, 248),
            width,
            height);
        DrawScalarSeries(
            VelocityComparisonChartCanvas,
            effectiveSamples,
            maximumVelocity,
            Color.FromRgb(250, 204, 21),
            width,
            height);
        DrawScalarChartCursor(VelocityComparisonChartCanvas, width, height);
    }

    private IReadOnlyList<ScalarSample> CreateRequestedVelocitySamples(CartesianPlaybackSnapshot snapshot)
    {
        var samples = new List<ScalarSample>();
        foreach (var frame in snapshot.SceneFrames)
        {
            var requestedVelocity = 0d;
            if (frame.State == RobotState.Moving &&
                frame.CommandSource is not null &&
                TryParseSingleCommand(frame.CommandSource.Text) is MoveToCommand moveCommand)
            {
                requestedVelocity = moveCommand.RequestedVelocityMillimetersPerSecond ?? 0;
            }

            samples.Add(new ScalarSample(frame.Time, requestedVelocity));
        }

        return samples;
    }

    private void UpdateDistanceChart()
    {
        DistanceChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.Poses.Count == 0)
        {
            return;
        }

        var width = DistanceChartCanvas.ActualWidth;
        var height = DistanceChartCanvas.ActualHeight;
        if (width <= ChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        var samples = CreateDistanceSamples(snapshot);
        var maximumDistance = Math.Max(1, samples.Count == 0 ? 0 : samples.Max(sample => sample.Value));

        DrawScalarChartGrid(
            DistanceChartCanvas,
            width,
            height,
            maximumDistance,
            "mm");
        DrawScalarSeries(
            DistanceChartCanvas,
            samples,
            maximumDistance,
            Color.FromRgb(45, 212, 191),
            width,
            height);
        DrawScalarChartCursor(DistanceChartCanvas, width, height);
    }

    private static IReadOnlyList<ScalarSample> CreateDistanceSamples(CartesianPlaybackSnapshot snapshot)
    {
        var samples = new List<ScalarSample>
        {
            new(snapshot.Poses[0].Time, 0)
        };
        var totalDistance = 0d;

        for (var index = 1; index < snapshot.Poses.Count; index++)
        {
            var previous = snapshot.Poses[index - 1];
            var current = snapshot.Poses[index];
            totalDistance += CalculateDistance(previous.ToolCenterPoint, current.ToolCenterPoint);
            samples.Add(new ScalarSample(current.Time, totalDistance));
        }

        return samples;
    }

    private void DrawScalarChartGrid(
        Canvas canvas,
        double width,
        double height,
        double maximumValue,
        string unit)
    {
        var plotLeft = ChartPaddingLeft;
        var plotTop = ChartPaddingTop;
        var plotRight = width - ChartPaddingRight;
        var plotBottom = height - ChartPaddingBottom;
        var gridBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85));

        for (var index = 0; index <= 2; index++)
        {
            var y = plotTop + ((plotBottom - plotTop) * index / 2);
            canvas.Children.Add(new Line
            {
                X1 = plotLeft,
                X2 = plotRight,
                Y1 = y,
                Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1
            });
        }

        var maximumLabel = new TextBlock
        {
            Text = $"{maximumValue:0.#} {unit}",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        canvas.Children.Add(maximumLabel);
        Canvas.SetLeft(maximumLabel, 4);
        Canvas.SetTop(maximumLabel, 2);

        var zeroLabel = new TextBlock
        {
            Text = "0",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        canvas.Children.Add(zeroLabel);
        Canvas.SetLeft(zeroLabel, 12);
        Canvas.SetTop(zeroLabel, plotBottom - 10);

        var timeLabel = new TextBlock
        {
            Text = "time",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        canvas.Children.Add(timeLabel);
        Canvas.SetLeft(timeLabel, plotRight - 28);
        Canvas.SetTop(timeLabel, plotBottom + 4);
    }

    private void DrawScalarSeries(
        Canvas canvas,
        IReadOnlyList<ScalarSample> samples,
        double maximumValue,
        Color color,
        double width,
        double height)
    {
        if (samples.Count == 0)
        {
            return;
        }

        var points = new PointCollection();
        foreach (var sample in samples)
        {
            points.Add(ToScalarChartPoint(sample, maximumValue, width, height));
        }

        canvas.Children.Add(new Polyline
        {
            Points = points,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2
        });
    }

    private void DrawScalarChartCursor(
        Canvas canvas,
        double width,
        double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var sceneFrame = snapshot.SceneFrames[currentFrameIndex];
        var cursorX = ToChartX(sceneFrame.Time, width);
        canvas.Children.Add(new Line
        {
            X1 = cursorX,
            X2 = cursorX,
            Y1 = ChartPaddingTop,
            Y2 = height - ChartPaddingBottom,
            Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            StrokeThickness = 1.5
        });
    }

    private Point ToScalarChartPoint(
        ScalarSample sample,
        double maximumValue,
        double width,
        double height)
    {
        var normalizedValue = maximumValue <= 0
            ? 0
            : Math.Clamp(sample.Value / maximumValue, 0, 1);

        return new Point(
            ToChartX(sample.Time, width),
            ChartPaddingTop + ((1 - normalizedValue) * (height - ChartPaddingTop - ChartPaddingBottom)));
    }

    private void UpdateStateChart()
    {
        StateChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.SceneFrames.Count == 0)
        {
            return;
        }

        var width = StateChartCanvas.ActualWidth;
        var height = StateChartCanvas.ActualHeight;
        if (width <= StateChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        DrawStateChartRows(width, height);
        DrawStateChartSegments(width, height);
        DrawStateChartCursor(width, height);
    }

    private void DrawStateChartRows(double width, double height)
    {
        var states = Enum.GetValues<RobotState>();
        var rowHeight = GetStateChartRowHeight(height, states.Length);
        var plotRight = width - ChartPaddingRight;
        var rowBrush = new SolidColorBrush(Color.FromRgb(30, 41, 59));
        var labelBrush = new SolidColorBrush(Color.FromRgb(148, 163, 184));

        for (var index = 0; index < states.Length; index++)
        {
            var y = ChartPaddingTop + (index * (rowHeight + StateChartRowGap));
            var rowBackground = new Rectangle
            {
                Width = plotRight - StateChartPaddingLeft,
                Height = rowHeight,
                Fill = rowBrush
            };
            StateChartCanvas.Children.Add(rowBackground);
            Canvas.SetLeft(rowBackground, StateChartPaddingLeft);
            Canvas.SetTop(rowBackground, y);

            var label = new TextBlock
            {
                Text = states[index].ToString(),
                Foreground = labelBrush,
                FontSize = 11
            };
            StateChartCanvas.Children.Add(label);
            Canvas.SetLeft(label, 6);
            Canvas.SetTop(label, y + Math.Max(0, (rowHeight - 14) / 2));
        }
    }

    private void DrawStateChartSegments(double width, double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var states = Enum.GetValues<RobotState>();
        var rowHeight = GetStateChartRowHeight(height, states.Length);
        var segments = CreateStateSegments(snapshot);

        foreach (var segment in segments)
        {
            var stateIndex = Array.IndexOf(states, segment.State);
            if (stateIndex < 0)
            {
                continue;
            }

            var x = ToStateChartX(segment.Start, width);
            var segmentWidth = Math.Max(
                2,
                ToStateChartX(segment.End, width) - x);
            var y = ChartPaddingTop + (stateIndex * (rowHeight + StateChartRowGap));

            var segmentRectangle = new Rectangle
            {
                Width = segmentWidth,
                Height = rowHeight,
                Fill = new SolidColorBrush(GetStateColor(segment.State))
            };
            StateChartCanvas.Children.Add(segmentRectangle);
            Canvas.SetLeft(segmentRectangle, x);
            Canvas.SetTop(segmentRectangle, y);
        }
    }

    private static IReadOnlyList<StateSegment> CreateStateSegments(CartesianPlaybackSnapshot snapshot)
    {
        var segments = new List<StateSegment>();
        var startTime = snapshot.SceneFrames[0].Time;
        var currentState = snapshot.SceneFrames[0].State;

        for (var index = 1; index < snapshot.SceneFrames.Count; index++)
        {
            var frame = snapshot.SceneFrames[index];
            if (frame.State == currentState)
            {
                continue;
            }

            segments.Add(new StateSegment(currentState, startTime, frame.Time));
            currentState = frame.State;
            startTime = frame.Time;
        }

        segments.Add(new StateSegment(currentState, startTime, snapshot.TotalDuration));
        return segments;
    }

    private void DrawStateChartCursor(double width, double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var sceneFrame = snapshot.SceneFrames[currentFrameIndex];
        var cursorX = ToStateChartX(sceneFrame.Time, width);
        StateChartCanvas.Children.Add(new Line
        {
            X1 = cursorX,
            X2 = cursorX,
            Y1 = ChartPaddingTop,
            Y2 = height - ChartPaddingBottom,
            Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            StrokeThickness = 1.5
        });
    }

    private double ToStateChartX(
        TimeSpan time,
        double width)
    {
        if (snapshot is null || snapshot.TotalDuration <= TimeSpan.Zero)
        {
            return StateChartPaddingLeft;
        }

        var normalizedTime = Math.Clamp(
            time.TotalSeconds / snapshot.TotalDuration.TotalSeconds,
            0,
            1);
        return StateChartPaddingLeft + (normalizedTime * (width - StateChartPaddingLeft - ChartPaddingRight));
    }

    private static double GetStateChartRowHeight(
        double height,
        int rowCount) =>
        Math.Max(
            8,
            (height - ChartPaddingTop - ChartPaddingBottom - (StateChartRowGap * (rowCount - 1))) / rowCount);

    private static Color GetStateColor(RobotState state) => state switch
    {
        RobotState.Idle => Color.FromRgb(148, 163, 184),
        RobotState.Homing => Color.FromRgb(96, 165, 250),
        RobotState.Moving => Color.FromRgb(34, 197, 94),
        RobotState.Waiting => Color.FromRgb(250, 204, 21),
        RobotState.Completed => Color.FromRgb(45, 212, 191),
        RobotState.Faulted => Color.FromRgb(248, 113, 113),
        _ => Colors.White
    };

    private void UpdateMovementExplanation(CartesianSceneFrame sceneFrame)
    {
        MovementExplanationText.Text = CreateMovementExplanation(sceneFrame);
    }

    private string CreateMovementExplanation(CartesianSceneFrame sceneFrame)
    {
        if (sceneFrame.CommandSource is null)
        {
            return "This frame was generated by the simulator before a script command started.";
        }

        var commandText = sceneFrame.CommandSource.Text;
        var explanation = new StringBuilder();
        explanation.AppendLine($"Command line {sceneFrame.CommandSource.LineNumber}: {commandText}");
        explanation.AppendLine($"Current state: {sceneFrame.State}.");

        var parsedCommand = TryParseSingleCommand(commandText);
        switch (parsedCommand)
        {
            case MoveToCommand moveToCommand:
                AppendMoveExplanation(explanation, sceneFrame, moveToCommand);
                break;

            case HomeCommand:
                explanation.AppendLine("HOME requests a return to the Cartesian origin at X=0, Y=0, Z=0.");
                explanation.AppendLine("The simulator treats homing as a normal planned movement with the Homing state.");
                break;

            case WaitCommand waitCommand:
                explanation.AppendLine($"WAIT keeps the current position fixed for {waitCommand.Duration.TotalMilliseconds:0.###} ms.");
                explanation.AppendLine("Only simulated time advances while the robot is waiting.");
                break;

            default:
                explanation.AppendLine("This command type is not recognized by the explanation panel yet.");
                break;
        }

        return explanation.ToString().TrimEnd();
    }

    private void AppendMoveExplanation(
        StringBuilder explanation,
        CartesianSceneFrame sceneFrame,
        MoveToCommand command)
    {
        if (snapshot is null || sceneFrame.CommandIndex is null)
        {
            explanation.AppendLine("The movement cannot be analyzed because command timing data is missing.");
            return;
        }

        var commandFrames = snapshot.SceneFrames
            .Select((frame, index) => (Frame: frame, Index: index))
            .Where(item => item.Frame.CommandIndex == sceneFrame.CommandIndex)
            .ToArray();
        if (commandFrames.Length == 0)
        {
            explanation.AppendLine("No playback frames were found for this movement.");
            return;
        }

        var firstIndex = commandFrames[0].Index;
        var lastIndex = commandFrames[^1].Index;
        var startPosition = ToCartesianPosition(snapshot.Poses[firstIndex]);
        var endPosition = ToCartesianPosition(snapshot.Poses[lastIndex]);
        var duration = commandFrames[^1].Frame.Time - commandFrames[0].Frame.Time;
        var distance = startPosition.DistanceTo(endPosition);
        var involvedAxes = GetInvolvedAxes(startPosition, endPosition);

        if (involvedAxes.Count == 0)
        {
            explanation.AppendLine("The target position is equal to the command start position.");
            explanation.AppendLine("The simulator handles this as a predictable zero-distance movement.");
            return;
        }

        var effectiveVelocity = duration > TimeSpan.Zero
            ? distance / duration.TotalSeconds
            : 0;
        var slowestAxis = involvedAxes
            .Select(axis => profile.GetAxis(axis))
            .OrderBy(axis => axis.MaximumVelocityMillimetersPerSecond)
            .First();

        explanation.AppendLine($"The command moves the {string.Join(", ", involvedAxes)} axis set.");
        explanation.AppendLine($"Distance covered by this command: {distance:0.###} mm.");
        explanation.AppendLine($"Duration in the generated playback: {duration.TotalSeconds:0.###} s.");
        explanation.AppendLine($"Effective velocity: {effectiveVelocity:0.###} mm/s.");

        if (command.RequestedVelocityMillimetersPerSecond is null)
        {
            explanation.AppendLine($"No speed was requested, so the slowest involved axis limit is used: {slowestAxis.Id} at {slowestAxis.MaximumVelocityMillimetersPerSecond:0.###} mm/s.");
            return;
        }

        explanation.AppendLine($"Requested velocity: {command.RequestedVelocityMillimetersPerSecond.Value:0.###} mm/s.");
        if (command.RequestedVelocityMillimetersPerSecond.Value > slowestAxis.MaximumVelocityMillimetersPerSecond)
        {
            explanation.AppendLine($"The requested velocity is capped by the {slowestAxis.Id} axis limit of {slowestAxis.MaximumVelocityMillimetersPerSecond:0.###} mm/s.");
        }
        else
        {
            explanation.AppendLine("The requested velocity is within the involved axis limits.");
        }
    }

    private RobotCommand? TryParseSingleCommand(string commandText)
    {
        try
        {
            return scriptDialect.Parse(commandText).Commands[0];
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            return null;
        }
    }

    private static CartesianPosition ToCartesianPosition(CartesianRobotPose pose) =>
        new(
            pose.ToolCenterPoint.XMillimeters,
            pose.ToolCenterPoint.YMillimeters,
            pose.ToolCenterPoint.ZMillimeters);

    private static IReadOnlyList<AxisId> GetInvolvedAxes(
        CartesianPosition start,
        CartesianPosition end)
    {
        const double tolerance = 0.0001;
        var axes = new List<AxisId>();

        if (Math.Abs(end.X - start.X) > tolerance)
        {
            axes.Add(AxisId.X);
        }

        if (Math.Abs(end.Y - start.Y) > tolerance)
        {
            axes.Add(AxisId.Y);
        }

        if (Math.Abs(end.Z - start.Z) > tolerance)
        {
            axes.Add(AxisId.Z);
        }

        return axes;
    }

    private double GetSelectedPlaybackSpeed()
    {
        if (PlaybackSpeedComboBox.SelectedItem is ComboBoxItem { Tag: string tag } &&
            double.TryParse(tag, NumberStyles.Float, CultureInfo.InvariantCulture, out var speed) &&
            speed > 0)
        {
            return speed;
        }

        return 1;
    }

    private void RenderDifferentialDriveFrame(int index)
    {
        if (differentialDriveSnapshot is null)
        {
            return;
        }

        differentialDriveFrameIndex = Math.Clamp(index, 0, differentialDriveSnapshot.FrameCount - 1);
        DifferentialDriveTimelineSlider.Value = differentialDriveFrameIndex;

        var frame = differentialDriveSnapshot.Frames[differentialDriveFrameIndex];
        DifferentialDriveCanvas.Children.Clear();

        DrawDifferentialDriveWorkspace(differentialDriveSnapshot.Profile);
        DrawDifferentialDrivePath(differentialDriveSnapshot);
        DrawDifferentialDriveRobot(frame.Pose);

        DifferentialDriveStateText.Text = frame.State.ToString();
        DifferentialDrivePoseText.Text =
            $"X={frame.Pose.X:0.###}, Y={frame.Pose.Y:0.###}, H={frame.Pose.HeadingDegrees:0.###} deg";
        DifferentialDriveCommandText.Text = frame.CommandName ?? "simulation";
        DifferentialDriveTimeText.Text =
            $"{frame.Time.TotalSeconds:0.###} / {differentialDriveSnapshot.TotalDuration.TotalSeconds:0.###} s";
        DifferentialDriveFramesText.Text = $"{differentialDriveFrameIndex + 1} / {differentialDriveSnapshot.FrameCount}";
        DifferentialDriveStatusText.Text =
            $"Frame {differentialDriveFrameIndex + 1}/{differentialDriveSnapshot.FrameCount} | " +
            $"t={frame.Time.TotalSeconds:0.###}s | {frame.State}";
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
            (height - (padding * 2)) / workspaceHeight);

        return new Point(
            padding + ((xMillimeters - profile.MinimumXMillimeters) * scale),
            height - padding - ((yMillimeters - profile.MinimumYMillimeters) * scale));
    }

    private void RenderScaraFrame(int index)
    {
        if (scaraSnapshot is null)
        {
            return;
        }

        scaraFrameIndex = Math.Clamp(index, 0, scaraSnapshot.FrameCount - 1);
        ScaraTimelineSlider.Value = scaraFrameIndex;

        var frame = scaraSnapshot.Frames[scaraFrameIndex];
        ScaraViewport.Children.Clear();
        ScaraViewport.Camera = CreateScaraCamera(scaraSnapshot.Profile);

        var sceneRoot = new Model3DGroup();
        sceneRoot.Children.Add(new AmbientLight(Color.FromRgb(82, 94, 116)));
        sceneRoot.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -2)));
        sceneRoot.Children.Add(CreateScaraWorkspaceModel(scaraSnapshot.Profile));
        sceneRoot.Children.Add(CreateScaraPathModel(scaraSnapshot));
        sceneRoot.Children.Add(CreateScaraRobotModel(scaraSnapshot.Profile, frame));
        ScaraViewport.Children.Add(new ModelVisual3D { Content = sceneRoot });

        ScaraStateText.Text = frame.State.ToString();
        ScaraJointsText.Text =
            $"S={frame.Joints.ShoulderDegrees:0.###}, E={frame.Joints.ElbowDegrees:0.###} deg";
        ScaraToolText.Text = $"X={frame.ToolPose.X:0.###}, Y={frame.ToolPose.Y:0.###} mm";
        ScaraCommandText.Text = frame.CommandName ?? "simulation";
        ScaraTimeText.Text = $"{frame.Time.TotalSeconds:0.###} / {scaraSnapshot.TotalDuration.TotalSeconds:0.###} s";
        ScaraStatusText.Text =
            $"Frame {scaraFrameIndex + 1}/{scaraSnapshot.FrameCount} | " +
            $"t={frame.Time.TotalSeconds:0.###}s | {frame.State}";
        ScaraMovementExplanationText.Text = CreateScaraMovementExplanation(frame);
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
        SimpleArmTimelineSlider.Value = simpleArmFrameIndex;

        var frame = simpleArmSnapshot.Frames[simpleArmFrameIndex];
        SimpleArmViewport.Children.Clear();
        SimpleArmViewport.Camera = CreateSimpleArmCamera(simpleArmSnapshot.Profile);

        var sceneRoot = new Model3DGroup();
        sceneRoot.Children.Add(new AmbientLight(Color.FromRgb(82, 94, 116)));
        sceneRoot.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -2)));
        sceneRoot.Children.Add(CreateSimpleArmWorkspaceModel(simpleArmSnapshot.Profile));
        sceneRoot.Children.Add(CreateSimpleArmPathModel(simpleArmSnapshot));
        sceneRoot.Children.Add(CreateSimpleArmRobotModel(simpleArmSnapshot.Profile, frame));
        SimpleArmViewport.Children.Add(new ModelVisual3D { Content = sceneRoot });

        SimpleArmStateText.Text = frame.State.ToString();
        SimpleArmJointsText.Text =
            $"B={frame.Joints.BaseDegrees:0.###}, S={frame.Joints.ShoulderDegrees:0.###}, E={frame.Joints.ElbowDegrees:0.###} deg";
        SimpleArmToolText.Text =
            $"X={frame.ToolPose.X:0.###}, Y={frame.ToolPose.Y:0.###}, O={frame.ToolPose.OrientationDegrees:0.###} deg";
        SimpleArmCommandText.Text = frame.CommandName ?? "simulation";
        SimpleArmTimeText.Text = $"{frame.Time.TotalSeconds:0.###} / {simpleArmSnapshot.TotalDuration.TotalSeconds:0.###} s";
        SimpleArmStatusText.Text =
            $"Frame {simpleArmFrameIndex + 1}/{simpleArmSnapshot.FrameCount} | " +
            $"t={frame.Time.TotalSeconds:0.###}s | {frame.State}";
        SimpleArmMovementExplanationText.Text = CreateSimpleArmMovementExplanation(frame);
    }

    private static string CreateScaraMovementExplanation(ScaraPlaybackFrame frame)
    {
        var commandName = frame.CommandName ?? "simulation";
        return
            $"{commandName} is represented as joint-space motion. " +
            $"The shoulder and elbow angles define the current planar arm shape, " +
            $"and forward kinematics maps those angles to the tool position " +
            $"X={frame.ToolPose.X:0.###}, Y={frame.ToolPose.Y:0.###} mm.";
    }

    private static string CreateSimpleArmMovementExplanation(SimpleArmPlaybackFrame frame)
    {
        var commandName = frame.CommandName ?? "simulation";
        return
            $"{commandName} is represented as joint-space motion. " +
            $"The base angle rotates the arm on the floor plane, while shoulder and elbow angles compose the links. " +
            $"Forward kinematics maps those joint angles to the tool pose " +
            $"X={frame.ToolPose.X:0.###}, Y={frame.ToolPose.Y:0.###}, O={frame.ToolPose.OrientationDegrees:0.###} deg.";
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

    private static CartesianRobotProfile CreateCartesianProfile() =>
        CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));

    private static XYPlotterProfile CreateXYPlotterProfile() =>
        XYPlotterProfile.Create(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200));

    private static DifferentialDriveProfile CreateDifferentialDriveProfile() =>
        new(
            minimumXMillimeters: 0,
            maximumXMillimeters: 500,
            minimumYMillimeters: 0,
            maximumYMillimeters: 350,
            wheelBaseMillimeters: 120,
            wheelRadiusMillimeters: 30,
            maximumLinearVelocityMillimetersPerSecond: 250,
            maximumAngularVelocityDegreesPerSecond: 180);

    private static ScaraRobotProfile CreateScaraProfile() =>
        new(
            firstLinkLengthMillimeters: 180,
            secondLinkLengthMillimeters: 120,
            shoulderJoint: new ScaraJoint(
                ScaraJointId.Shoulder,
                minimumDegrees: -180,
                maximumDegrees: 180,
                maximumVelocityDegreesPerSecond: 120),
            elbowJoint: new ScaraJoint(
                ScaraJointId.Elbow,
                minimumDegrees: -150,
                maximumDegrees: 150,
                maximumVelocityDegreesPerSecond: 100));

    private static SimpleArmRobotProfile CreateSimpleArmProfile() =>
        new(
            firstLinkLengthMillimeters: 120,
            secondLinkLengthMillimeters: 90,
            thirdLinkLengthMillimeters: 60,
            baseJoint: new SimpleArmJoint(
                SimpleArmJointId.Base,
                minimumDegrees: -180,
                maximumDegrees: 180,
                maximumVelocityDegreesPerSecond: 100),
            shoulderJoint: new SimpleArmJoint(
                SimpleArmJointId.Shoulder,
                minimumDegrees: -120,
                maximumDegrees: 120,
                maximumVelocityDegreesPerSecond: 90),
            elbowJoint: new SimpleArmJoint(
                SimpleArmJointId.Elbow,
                minimumDegrees: -150,
                maximumDegrees: 150,
                maximumVelocityDegreesPerSecond: 80));

    private void ValidateCommandSequence(RobotCommandSequence commands)
    {
        if (activeViewerKind == RobotViewerKind.XYPlotterTwoDimensional)
        {
            if (xyPlotterProfile is null)
            {
                throw new InvalidOperationException("XY Plotter profile is not configured.");
            }

            foreach (var command in commands.Commands)
            {
                RobotCommandValidator.Validate(command, xyPlotterProfile);
            }

            return;
        }

        foreach (var command in commands.Commands)
        {
            RobotCommandValidator.Validate(command, profile);
        }
    }

    private static void ValidateDifferentialDriveCommandSequence(
        RobotCommandSequence commands,
        DifferentialDriveProfile profile)
    {
        foreach (var command in commands.Commands)
        {
            RobotCommandValidator.Validate(command, profile);
        }
    }

    private static void ValidateScaraCommandSequence(
        RobotCommandSequence commands,
        ScaraRobotProfile profile)
    {
        foreach (var command in commands.Commands)
        {
            RobotCommandValidator.Validate(command, profile);
        }
    }

    private static void ValidateSimpleArmCommandSequence(
        RobotCommandSequence commands,
        SimpleArmRobotProfile profile)
    {
        foreach (var command in commands.Commands)
        {
            RobotCommandValidator.Validate(command, profile);
        }
    }

    private bool TryCreateSnapshotFromScript(
        string script,
        out CartesianPlaybackSnapshot? nextSnapshot,
        out string message)
    {
        try
        {
            nextSnapshot = CreateSnapshot(script);
            message = $"Script is valid. Generated {nextSnapshot.SceneFrameCount} playback frames.";
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = exception.Message;
            return false;
        }
    }

    private bool TryCreateDifferentialDriveSnapshotFromScript(
        string script,
        out DifferentialDrivePlaybackSnapshot? nextSnapshot,
        out string message)
    {
        try
        {
            nextSnapshot = CreateDifferentialDriveSnapshot(script);
            message = $"Mobile script is valid. Generated {nextSnapshot.FrameCount} playback frames.";
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = exception.Message;
            return false;
        }
    }

    private bool TryCreateScaraSnapshotFromScript(
        string script,
        out ScaraPlaybackSnapshot? nextSnapshot,
        out string message)
    {
        try
        {
            nextSnapshot = CreateScaraSnapshot(script);
            message = $"SCARA script is valid. Generated {nextSnapshot.FrameCount} playback frames.";
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = exception.Message;
            return false;
        }
    }

    private bool TryCreateSimpleArmSnapshotFromScript(
        string script,
        out SimpleArmPlaybackSnapshot? nextSnapshot,
        out string message)
    {
        try
        {
            nextSnapshot = CreateSimpleArmSnapshot(script);
            message = $"Simple arm script is valid. Generated {nextSnapshot.FrameCount} playback frames.";
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = exception.Message;
            return false;
        }
    }

    private void SetScriptStatus(
        string message,
        Color color)
    {
        ScriptStatusText.Text = message;
        ScriptStatusText.Foreground = new SolidColorBrush(color);
    }

    private void SetDifferentialDriveScriptStatus(
        string message,
        Color color)
    {
        DifferentialDriveScriptStatusText.Text = message;
        DifferentialDriveScriptStatusText.Foreground = new SolidColorBrush(color);
    }

    private void SetScaraScriptStatus(
        string message,
        Color color)
    {
        ScaraScriptStatusText.Text = message;
        ScaraScriptStatusText.Foreground = new SolidColorBrush(color);
    }

    private void SetSimpleArmScriptStatus(
        string message,
        Color color)
    {
        SimpleArmScriptStatusText.Text = message;
        SimpleArmScriptStatusText.Foreground = new SolidColorBrush(color);
    }

    private void RefreshScriptEditorGutter()
    {
        if (!IsLoaded)
        {
            return;
        }

        ScriptEditorGutterPanel.Children.Clear();

        foreach (var line in ScriptEditorLineMetadataBuilder.Build(ScriptEditorTextBox.Text))
        {
            ScriptEditorGutterPanel.Children.Add(CreateScriptGutterLine(line));
        }
    }

    private static UIElement CreateScriptGutterLine(ScriptEditorLineMetadata line)
    {
        var container = new Grid
        {
            Height = 16.35
        };
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });

        var lineNumber = new TextBlock
        {
            Text = line.LineNumber.ToString(CultureInfo.InvariantCulture),
            Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
            FontFamily = new FontFamily("Consolas"),
            FontSize = 13,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(lineNumber, 0);
        container.Children.Add(lineNumber);

        if (line.Kind != ScriptEditorLineKind.Empty)
        {
            var commandTag = new Border
            {
                Height = 15,
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(5, 0, 5, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(GetScriptCommandBackground(line.Kind)),
                CornerRadius = new CornerRadius(4),
                Child = new TextBlock
                {
                    Text = line.CommandText,
                    Foreground = new SolidColorBrush(GetScriptCommandForeground(line.Kind)),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 9,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            Grid.SetColumn(commandTag, 1);
            container.Children.Add(commandTag);
        }

        return container;
    }

    private static Color GetScriptCommandBackground(ScriptEditorLineKind kind) => kind switch
    {
        ScriptEditorLineKind.Home => Color.FromRgb(30, 64, 175),
        ScriptEditorLineKind.Move => Color.FromRgb(22, 101, 52),
        ScriptEditorLineKind.Wait => Color.FromRgb(133, 77, 14),
        ScriptEditorLineKind.Other => Color.FromRgb(127, 29, 29),
        _ => Color.FromRgb(30, 41, 59)
    };

    private static Color GetScriptCommandForeground(ScriptEditorLineKind kind) => kind switch
    {
        ScriptEditorLineKind.Home => Color.FromRgb(191, 219, 254),
        ScriptEditorLineKind.Move => Color.FromRgb(187, 247, 208),
        ScriptEditorLineKind.Wait => Color.FromRgb(254, 240, 138),
        ScriptEditorLineKind.Other => Color.FromRgb(254, 202, 202),
        _ => Color.FromRgb(203, 213, 225)
    };

    private static Brush GetStatusBrush(RobotAvailabilityStatus status) => status switch
    {
        RobotAvailabilityStatus.Available => new SolidColorBrush(Color.FromRgb(74, 222, 128)),
        RobotAvailabilityStatus.Experimental => new SolidColorBrush(Color.FromRgb(250, 204, 21)),
        RobotAvailabilityStatus.Planned => new SolidColorBrush(Color.FromRgb(148, 163, 184)),
        _ => new SolidColorBrush(Colors.White)
    };

    private static Brush GetStatusBackgroundBrush(RobotAvailabilityStatus status) => status switch
    {
        RobotAvailabilityStatus.Available => new SolidColorBrush(Color.FromRgb(5, 46, 22)),
        RobotAvailabilityStatus.Experimental => new SolidColorBrush(Color.FromRgb(66, 32, 6)),
        RobotAvailabilityStatus.Planned => new SolidColorBrush(Color.FromRgb(30, 41, 59)),
        _ => new SolidColorBrush(Color.FromRgb(30, 41, 59))
    };

    private static Brush GetStatusBorderBrush(RobotAvailabilityStatus status) => status switch
    {
        RobotAvailabilityStatus.Available => new SolidColorBrush(Color.FromRgb(22, 101, 52)),
        RobotAvailabilityStatus.Experimental => new SolidColorBrush(Color.FromRgb(161, 98, 7)),
        RobotAvailabilityStatus.Planned => new SolidColorBrush(Color.FromRgb(71, 85, 105)),
        _ => new SolidColorBrush(Color.FromRgb(71, 85, 105))
    };

    private static string FormatCapability(RobotCapability capability) => capability switch
    {
        RobotCapability.Simulation => "simulation",
        RobotCapability.Dsl => "DSL",
        RobotCapability.ThreeDimensionalView => "3D view",
        RobotCapability.TwoDimensionalView => "2D view",
        RobotCapability.ManualControl => "manual control",
        RobotCapability.Playback => "playback",
        RobotCapability.PathDrawing => "path drawing",
        RobotCapability.PathPlanning => "path planning",
        RobotCapability.Odometry => "odometry",
        RobotCapability.ForwardKinematics => "forward kinematics",
        RobotCapability.InverseKinematics => "inverse kinematics",
        RobotCapability.WorkspaceVisualization => "workspace",
        RobotCapability.AttitudeControl => "attitude control",
        RobotCapability.FutureGCode => "future G-code",
        RobotCapability.HardwareCommunication => "hardware",
        _ => capability.ToString()
    };

    private static (int Start, int Length) GetLineSelection(
        string text,
        int lineNumber)
    {
        if (lineNumber <= 0)
        {
            return (0, 0);
        }

        var currentLine = 1;
        var start = 0;

        for (var index = 0; index < text.Length; index++)
        {
            if (currentLine == lineNumber)
            {
                start = index;
                break;
            }

            if (text[index] == '\n')
            {
                currentLine++;
            }
        }

        if (currentLine != lineNumber)
        {
            return (0, 0);
        }

        var end = text.IndexOf('\n', start);
        if (end < 0)
        {
            end = text.Length;
        }

        while (end > start && text[end - 1] == '\r')
        {
            end--;
        }

        return (start, end - start);
    }

    private static PerspectiveCamera CreateCamera(
        CartesianViewportSnapshot viewport,
        double azimuthDegrees,
        double elevationDegrees,
        double distanceMillimeters)
    {
        var azimuthRadians = DegreesToRadians(azimuthDegrees);
        var elevationRadians = DegreesToRadians(elevationDegrees);
        var horizontalDistance = distanceMillimeters * Math.Cos(elevationRadians);
        var cameraPosition = new VisualVector3(
            viewport.Target.XMillimeters + (horizontalDistance * Math.Cos(azimuthRadians)),
            viewport.Target.YMillimeters + (horizontalDistance * Math.Sin(azimuthRadians)),
            viewport.Target.ZMillimeters + (distanceMillimeters * Math.Sin(elevationRadians)));

        return new PerspectiveCamera
        {
            Position = ToPoint3D(cameraPosition),
            LookDirection = ToVector3D(Subtract(viewport.Target, cameraPosition)),
            UpDirection = ToVector3D(viewport.Up),
            NearPlaneDistance = viewport.NearClipMillimeters,
            FarPlaneDistance = viewport.FarClipMillimeters,
            FieldOfView = 45
        };
    }

    private static Model3D CreateModel(CartesianScenePrimitive primitive) =>
        primitive.Kind == CartesianScenePrimitiveKind.Workspace
            ? CreateWorkspaceBoundsModel(primitive)
            : CreateBoxModel(primitive);

    private bool IsPrimitiveVisible(CartesianScenePrimitive primitive) =>
        primitive.Kind switch
        {
            CartesianScenePrimitiveKind.Workspace => ShowWorkspaceCheckBox.IsChecked == true,
            CartesianScenePrimitiveKind.Rail => ShowRailsCheckBox.IsChecked == true,
            CartesianScenePrimitiveKind.Carriage => ShowCarriagesCheckBox.IsChecked == true,
            CartesianScenePrimitiveKind.Tool => ShowToolCheckBox.IsChecked == true,
            _ => true
        };

    private static Model3DGroup CreateGridModel(CartesianWorkspaceBounds bounds)
    {
        var group = new Model3DGroup();
        var size = bounds.Size;
        var gridZ = bounds.Minimum.ZMillimeters - GridLineThicknessMillimeters;
        var gridColor = Color.FromArgb(90, 71, 85, 105);

        for (var x = bounds.Minimum.XMillimeters; x <= bounds.Maximum.XMillimeters; x += GridSpacingMillimeters)
        {
            group.Children.Add(CreateColoredBoxModel(
                new VisualVector3(x, bounds.Center.YMillimeters, gridZ),
                new VisualVector3(GridLineThicknessMillimeters, size.YMillimeters, GridLineThicknessMillimeters),
                gridColor));
        }

        for (var y = bounds.Minimum.YMillimeters; y <= bounds.Maximum.YMillimeters; y += GridSpacingMillimeters)
        {
            group.Children.Add(CreateColoredBoxModel(
                new VisualVector3(bounds.Center.XMillimeters, y, gridZ),
                new VisualVector3(size.XMillimeters, GridLineThicknessMillimeters, GridLineThicknessMillimeters),
                gridColor));
        }

        return group;
    }

    private static Model3DGroup CreateGlobalAxesModel(CartesianWorkspaceBounds bounds)
    {
        var origin = new VisualVector3(
            Math.Clamp(0, bounds.Minimum.XMillimeters, bounds.Maximum.XMillimeters),
            Math.Clamp(0, bounds.Minimum.YMillimeters, bounds.Maximum.YMillimeters),
            Math.Clamp(0, bounds.Minimum.ZMillimeters, bounds.Maximum.ZMillimeters));
        var group = new Model3DGroup();

        group.Children.Add(CreateColoredBoxModel(
            new VisualVector3(bounds.Center.XMillimeters, origin.YMillimeters, origin.ZMillimeters),
            new VisualVector3(bounds.Size.XMillimeters, AxisLineThicknessMillimeters, AxisLineThicknessMillimeters),
            Color.FromRgb(248, 113, 113)));

        group.Children.Add(CreateColoredBoxModel(
            new VisualVector3(origin.XMillimeters, bounds.Center.YMillimeters, origin.ZMillimeters),
            new VisualVector3(AxisLineThicknessMillimeters, bounds.Size.YMillimeters, AxisLineThicknessMillimeters),
            Color.FromRgb(34, 197, 94)));

        group.Children.Add(CreateColoredBoxModel(
            new VisualVector3(origin.XMillimeters, origin.YMillimeters, bounds.Center.ZMillimeters),
            new VisualVector3(AxisLineThicknessMillimeters, AxisLineThicknessMillimeters, bounds.Size.ZMillimeters),
            Color.FromRgb(96, 165, 250)));

        return group;
    }

    private static Model3DGroup CreatePlannedPathModel(CartesianPlaybackSnapshot snapshot)
    {
        var group = new Model3DGroup();
        var pathPointSize = new VisualVector3(
            PathPointSizeMillimeters,
            PathPointSizeMillimeters,
            PathPointSizeMillimeters);
        var pathPointColor = Color.FromArgb(190, 250, 204, 21);
        var step = Math.Max(1, snapshot.Poses.Count / MaximumPathPointCount);
        VisualVector3? previousPoint = null;

        for (var index = 0; index < snapshot.Poses.Count; index += step)
        {
            var point = snapshot.Poses[index].ToolCenterPoint;
            if (previousPoint is not null && AreNear(previousPoint.Value, point))
            {
                continue;
            }

            group.Children.Add(CreateColoredBoxModel(point, pathPointSize, pathPointColor));
            previousPoint = point;
        }

        var finalPoint = snapshot.Poses[^1].ToolCenterPoint;
        if (previousPoint is null || !AreNear(previousPoint.Value, finalPoint))
        {
            group.Children.Add(CreateColoredBoxModel(finalPoint, pathPointSize, pathPointColor));
        }

        return group;
    }

    private static Model3DGroup CreateStartEndMarkersModel(CartesianPlaybackSnapshot snapshot)
    {
        var markerSize = new VisualVector3(
            StartEndMarkerSizeMillimeters,
            StartEndMarkerSizeMillimeters,
            StartEndMarkerSizeMillimeters);
        var group = new Model3DGroup();

        group.Children.Add(CreateColoredBoxModel(
            snapshot.Poses[0].ToolCenterPoint,
            markerSize,
            Color.FromRgb(74, 222, 128)));
        group.Children.Add(CreateColoredBoxModel(
            snapshot.Poses[^1].ToolCenterPoint,
            markerSize,
            Color.FromRgb(251, 146, 60)));

        return group;
    }

    private static IReadOnlyList<Viewport2DVisual3D> CreateAxisLabelVisuals(
        CartesianWorkspaceBounds bounds,
        PerspectiveCamera camera)
    {
        var origin = new VisualVector3(
            Math.Clamp(0, bounds.Minimum.XMillimeters, bounds.Maximum.XMillimeters),
            Math.Clamp(0, bounds.Minimum.YMillimeters, bounds.Maximum.YMillimeters),
            Math.Clamp(0, bounds.Minimum.ZMillimeters, bounds.Maximum.ZMillimeters));

        return
        [
            CreateAxisLabelVisual(
                "X",
                new VisualVector3(
                    bounds.Maximum.XMillimeters + AxisLabelOffsetMillimeters,
                    origin.YMillimeters,
                    origin.ZMillimeters),
                Color.FromRgb(248, 113, 113),
                camera),
            CreateAxisLabelVisual(
                "Y",
                new VisualVector3(
                    origin.XMillimeters,
                    bounds.Maximum.YMillimeters + AxisLabelOffsetMillimeters,
                    origin.ZMillimeters),
                Color.FromRgb(34, 197, 94),
                camera),
            CreateAxisLabelVisual(
                "Z",
                new VisualVector3(
                    origin.XMillimeters,
                    origin.YMillimeters,
                    bounds.Maximum.ZMillimeters + AxisLabelOffsetMillimeters),
                Color.FromRgb(96, 165, 250),
                camera)
        ];
    }

    private static Viewport2DVisual3D CreateAxisLabelVisual(
        string text,
        VisualVector3 center,
        Color accent,
        PerspectiveCamera camera)
    {
        var material = new DiffuseMaterial(Brushes.White);
        Viewport2DVisual3D.SetIsVisualHostMaterial(material, true);

        return new Viewport2DVisual3D
        {
            Geometry = CreateBillboardMesh(
                center,
                AxisLabelWidthMillimeters,
                AxisLabelHeightMillimeters,
                camera),
            Material = material,
            Visual = CreateAxisLabelElement(text, accent)
        };
    }

    private static Border CreateAxisLabelElement(
        string text,
        Color accent) =>
        new()
        {
            Width = 32,
            Height = 24,
            Background = new SolidColorBrush(Color.FromArgb(210, 15, 23, 42)),
            BorderBrush = new SolidColorBrush(accent),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Child = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(accent),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            }
        };

    private static MeshGeometry3D CreateBillboardMesh(
        VisualVector3 center,
        double widthMillimeters,
        double heightMillimeters,
        PerspectiveCamera camera)
    {
        var look = camera.LookDirection;
        var up = camera.UpDirection;
        look.Normalize();
        up.Normalize();

        var right = Vector3D.CrossProduct(look, up);
        if (right.LengthSquared <= 0.000001)
        {
            right = new Vector3D(1, 0, 0);
        }

        right.Normalize();
        up = Vector3D.CrossProduct(right, look);
        up.Normalize();

        var centerPoint = ToPoint3D(center);
        var halfRight = right * (widthMillimeters / 2);
        var halfUp = up * (heightMillimeters / 2);

        return new MeshGeometry3D
        {
            Positions = new Point3DCollection
            {
                centerPoint - halfRight - halfUp,
                centerPoint + halfRight - halfUp,
                centerPoint + halfRight + halfUp,
                centerPoint - halfRight + halfUp
            },
            TextureCoordinates = new PointCollection
            {
                new(0, 1),
                new(1, 1),
                new(1, 0),
                new(0, 0)
            },
            TriangleIndices = new Int32Collection
            {
                0, 1, 2,
                0, 2, 3
            }
        };
    }

    private static GeometryModel3D CreateBoxModel(CartesianScenePrimitive primitive) =>
        new(
            CreateBoxMesh(primitive.Center, primitive.Size),
            new DiffuseMaterial(new SolidColorBrush(GetColor(primitive.Kind))))
        {
            BackMaterial = new DiffuseMaterial(new SolidColorBrush(GetColor(primitive.Kind)))
        };

    private static GeometryModel3D CreateColoredBoxModel(
        VisualVector3 center,
        VisualVector3 size,
        Color color) =>
        new(
            CreateBoxMesh(center, size),
            new DiffuseMaterial(new SolidColorBrush(color)))
        {
            BackMaterial = new DiffuseMaterial(new SolidColorBrush(color))
        };

    private static Model3DGroup CreateWorkspaceBoundsModel(CartesianScenePrimitive primitive)
    {
        var lineThickness = Math.Max(
            1,
            Math.Min(
                Math.Min(primitive.Size.XMillimeters, primitive.Size.YMillimeters),
                primitive.Size.ZMillimeters) * 0.008);
        var halfX = Math.Max(primitive.Size.XMillimeters, 1) / 2;
        var halfY = Math.Max(primitive.Size.YMillimeters, 1) / 2;
        var halfZ = Math.Max(primitive.Size.ZMillimeters, 1) / 2;
        var center = primitive.Center;
        var group = new Model3DGroup();

        foreach (var yOffset in new[] { -halfY, halfY })
        {
            foreach (var zOffset in new[] { -halfZ, halfZ })
            {
                group.Children.Add(CreateWorkspaceEdge(
                    new VisualVector3(center.XMillimeters, center.YMillimeters + yOffset, center.ZMillimeters + zOffset),
                    new VisualVector3(primitive.Size.XMillimeters, lineThickness, lineThickness)));
            }
        }

        foreach (var xOffset in new[] { -halfX, halfX })
        {
            foreach (var zOffset in new[] { -halfZ, halfZ })
            {
                group.Children.Add(CreateWorkspaceEdge(
                    new VisualVector3(center.XMillimeters + xOffset, center.YMillimeters, center.ZMillimeters + zOffset),
                    new VisualVector3(lineThickness, primitive.Size.YMillimeters, lineThickness)));
            }
        }

        foreach (var xOffset in new[] { -halfX, halfX })
        {
            foreach (var yOffset in new[] { -halfY, halfY })
            {
                group.Children.Add(CreateWorkspaceEdge(
                    new VisualVector3(center.XMillimeters + xOffset, center.YMillimeters + yOffset, center.ZMillimeters),
                    new VisualVector3(lineThickness, lineThickness, primitive.Size.ZMillimeters)));
            }
        }

        return group;
    }

    private static GeometryModel3D CreateWorkspaceEdge(
        VisualVector3 center,
        VisualVector3 size) =>
        new(
            CreateBoxMesh(center, size),
            new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(120, 148, 163, 184))))
        {
            BackMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(120, 148, 163, 184)))
        };

    private static MeshGeometry3D CreateBoxMesh(
        VisualVector3 center,
        VisualVector3 size)
    {
        var halfX = Math.Max(size.XMillimeters, 1) / 2;
        var halfY = Math.Max(size.YMillimeters, 1) / 2;
        var halfZ = Math.Max(size.ZMillimeters, 1) / 2;
        var x = center.XMillimeters;
        var y = center.YMillimeters;
        var z = center.ZMillimeters;
        var mesh = new MeshGeometry3D
        {
            Positions = new Point3DCollection
            {
                new(x - halfX, y - halfY, z - halfZ),
                new(x + halfX, y - halfY, z - halfZ),
                new(x + halfX, y + halfY, z - halfZ),
                new(x - halfX, y + halfY, z - halfZ),
                new(x - halfX, y - halfY, z + halfZ),
                new(x + halfX, y - halfY, z + halfZ),
                new(x + halfX, y + halfY, z + halfZ),
                new(x - halfX, y + halfY, z + halfZ)
            },
            TriangleIndices = new Int32Collection
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                1, 2, 6, 1, 6, 5,
                2, 3, 7, 2, 7, 6,
                3, 0, 4, 3, 4, 7
            }
        };

        return mesh;
    }

    private static Color GetColor(CartesianScenePrimitiveKind kind) => kind switch
    {
        CartesianScenePrimitiveKind.Workspace => Color.FromArgb(28, 148, 163, 184),
        CartesianScenePrimitiveKind.Rail => Color.FromRgb(96, 165, 250),
        CartesianScenePrimitiveKind.Carriage => Color.FromRgb(34, 197, 94),
        CartesianScenePrimitiveKind.Tool => Color.FromRgb(248, 113, 113),
        _ => Colors.White
    };

    private static Point3D ToPoint3D(VisualVector3 vector) =>
        new(vector.XMillimeters, vector.YMillimeters, vector.ZMillimeters);

    private static Vector3D ToVector3D(VisualVector3 vector) =>
        new(vector.XMillimeters, vector.YMillimeters, vector.ZMillimeters);

    private static VisualVector3 Subtract(
        VisualVector3 left,
        VisualVector3 right) =>
        new(
            left.XMillimeters - right.XMillimeters,
            left.YMillimeters - right.YMillimeters,
            left.ZMillimeters - right.ZMillimeters);

    private static double CalculateDistance(
        VisualVector3 left,
        VisualVector3 right)
    {
        var deltaX = left.XMillimeters - right.XMillimeters;
        var deltaY = left.YMillimeters - right.YMillimeters;
        var deltaZ = left.ZMillimeters - right.ZMillimeters;

        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ));
    }

    private static bool AreNear(
        VisualVector3 left,
        VisualVector3 right)
    {
        const double toleranceMillimeters = 0.001;

        return Math.Abs(left.XMillimeters - right.XMillimeters) <= toleranceMillimeters &&
               Math.Abs(left.YMillimeters - right.YMillimeters) <= toleranceMillimeters &&
               Math.Abs(left.ZMillimeters - right.ZMillimeters) <= toleranceMillimeters;
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;

    private static string FormatNumber(double value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);
}
