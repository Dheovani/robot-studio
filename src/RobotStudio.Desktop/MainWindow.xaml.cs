using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using RobotStudio.Desktop.Didactics;
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

public partial class MainWindow : Window
{
    private const double ChartPaddingLeft = 28;
    private const double ChartPaddingTop = 12;
    private const double ChartPaddingRight = 10;
    private const double ChartPaddingBottom = 24;
    private const double StateChartPaddingLeft = 78;
    private const double StateChartRowGap = 4;
    private const double RobotCardGap = 18;
    private const double RobotCardMinimumWidth = 280;
    private const double RobotCardPreferredWidth = 360;
    private const int RobotCardMaximumColumns = 6;
    private const string ScriptFileDialogFilter = "RobotStudio scripts (*.robot;*.gcode;*.txt)|*.robot;*.gcode;*.txt|All files (*.*)|*.*";
    private const string ScriptFileDefaultExtension = ".robot";

    private readonly DispatcherTimer playbackTimer;
    private readonly DispatcherTimer scriptValidationTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(350)
    };
    private readonly TimeSpan renderInterval = TimeSpan.FromMilliseconds(16);
    private readonly Stopwatch playbackStopwatch = new();
    private readonly IRobotScriptDialect simpleDslDialect = new RobotScriptParser();
    private readonly IRobotScriptDialect gCodeDialect = new GCodeParser();
    private readonly CartesianMovementExplanationBuilder movementExplanationBuilder = new();
    private readonly List<FrameworkElement> sessionRecoveryPanels = [];
    private readonly List<Button> playPauseButtons = [];
    private readonly ISchematicViewportPresenter cartesianViewportPresenter;
    private readonly WpfCanvasScenePresenter differentialDriveCanvasPresenter;
    private readonly ISchematicViewportPresenter scaraViewportPresenter;
    private readonly ISchematicViewportPresenter simpleArmViewportPresenter;
    private readonly ISchematicViewportPresenter deltaViewportPresenter;
    private readonly ISchematicViewportPresenter droneViewportPresenter;
    private readonly ISchematicViewportPresenter industrialArmViewportPresenter;

    private IRobotScriptDialect CartesianScriptDialect =>
        ScriptDialectComboBox.SelectedItem is RobotScriptDialectDescriptor
        {
            Id: RobotScriptDialectId.GCode
        }
            ? gCodeDialect
            : simpleDslDialect;

    private IRobotScriptDialect ScaraScriptDialect =>
        ScaraScriptDialectComboBox.SelectedItem is RobotScriptDialectDescriptor
        {
            Id: RobotScriptDialectId.GCode
        }
            ? new GCodeParser(new ScaraGCodeCommandMapper(CreateScaraProfile()))
            : simpleDslDialect;

    private IRobotScriptDialect SimpleArmScriptDialect =>
        SimpleArmScriptDialectComboBox.SelectedItem is RobotScriptDialectDescriptor
        {
            Id: RobotScriptDialectId.GCode
        }
            ? new GCodeParser(new SimpleArmGCodeCommandMapper(CreateSimpleArmProfile()))
            : simpleDslDialect;

    private IRobotScriptDialect DeltaScriptDialect =>
        DeltaScriptDialectComboBox.SelectedItem is RobotScriptDialectDescriptor
        {
            Id: RobotScriptDialectId.GCode
        }
            ? new GCodeParser(new DeltaGCodeCommandMapper(CreateDeltaProfile()))
            : simpleDslDialect;

    private IRobotScriptDialect IndustrialArmScriptDialect =>
        IndustrialArmScriptDialectComboBox.SelectedItem is RobotScriptDialectDescriptor
        {
            Id: RobotScriptDialectId.GCode
        }
            ? new GCodeParser(new IndustrialArmGCodeCommandMapper(CreateIndustrialArmProfile()))
            : simpleDslDialect;
    private CartesianRobotProfile profile = CreateCartesianProfile();
    private XYPlotterProfile? xyPlotterProfile;
    private CartesianPosition initialPosition = new(X: 40, Y: 30, Z: 20);
    private RobotViewerKind activeViewerKind = RobotViewerKind.CartesianThreeDimensional;
    private CartesianPlaybackSnapshot? snapshot;
    private DifferentialDrivePlaybackSnapshot? differentialDriveSnapshot;
    private ScaraPlaybackSnapshot? scaraSnapshot;
    private SimpleArmPlaybackSnapshot? simpleArmSnapshot;
    private DeltaPlaybackSnapshot? deltaSnapshot;
    private DronePlaybackSnapshot? droneSnapshot;
    private IndustrialArmPlaybackSnapshot? industrialArmSnapshot;
    private SimulationContext? cartesianSessionContext;
    private DifferentialDriveSimulationContext? differentialDriveSessionContext;
    private ScaraSimulationContext? scaraSessionContext;
    private SimpleArmSimulationContext? simpleArmSessionContext;
    private DeltaSimulationContext? deltaSessionContext;
    private DroneSimulationContext? droneSessionContext;
    private IndustrialArmSimulationContext? industrialArmSessionContext;
    private RobotViewerKind? pendingScriptValidationKind;
    private int currentFrameIndex;
    private int differentialDriveFrameIndex;
    private int scaraFrameIndex;
    private int simpleArmFrameIndex;
    private int deltaFrameIndex;
    private int droneFrameIndex;
    private int industrialArmFrameIndex;
    private bool isPlaying;
    private TimeSpan playbackStartPosition;
    private PlaybackRenderTimeline? playbackRenderTimeline;
    private double baseCameraDistanceMillimeters;
    private double azimuthDegrees = -45;
    private double elevationDegrees = 35;
    private double zoomMultiplier = 1;
    private readonly ViewportOrbitInteractionState cartesianOrbitInteraction = new();
    private double differentialDriveZoomMultiplier = 1;
    private double scaraAzimuthDegrees = -45;
    private double scaraElevationDegrees = 32;
    private double scaraZoomMultiplier = 1.8;
    private readonly ViewportOrbitInteractionState scaraOrbitInteraction = new();
    private double simpleArmAzimuthDegrees = -45;
    private double simpleArmElevationDegrees = 30;
    private double simpleArmZoomMultiplier = 2.15;
    private readonly ViewportOrbitInteractionState simpleArmOrbitInteraction = new();
    private double deltaAzimuthDegrees = -45;
    private double deltaElevationDegrees = 32;
    private double deltaZoomMultiplier = 1.75;
    private readonly ViewportOrbitInteractionState deltaOrbitInteraction = new();
    private double droneAzimuthDegrees = -45;
    private double droneElevationDegrees = 34;
    private double droneZoomMultiplier = 1.55;
    private readonly ViewportOrbitInteractionState droneOrbitInteraction = new();
    private double industrialArmAzimuthDegrees = -45;
    private double industrialArmElevationDegrees = 28;
    private double industrialArmZoomMultiplier = 1;
    private readonly ViewportOrbitInteractionState industrialArmOrbitInteraction = new();

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

        cartesianViewportPresenter = new WpfSchematicViewportPresenter(RobotViewport);
        differentialDriveCanvasPresenter = new WpfCanvasScenePresenter(DifferentialDriveCanvas);
        scaraViewportPresenter = new WpfSchematicViewportPresenter(ScaraViewport);
        simpleArmViewportPresenter = new WpfSchematicViewportPresenter(SimpleArmViewport);
        deltaViewportPresenter = new WpfSchematicViewportPresenter(DeltaViewport);
        droneViewportPresenter = new WpfSchematicViewportPresenter(DroneViewport);
        industrialArmViewportPresenter = new WpfSchematicViewportPresenter(IndustrialArmViewport);

        playbackTimer = new DispatcherTimer
        {
            Interval = renderInterval
        };
        playbackTimer.Tick += PlaybackTimer_Tick;
        scriptValidationTimer.Tick += ScriptValidationTimer_Tick;

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        InitializeLanguageSelector();
        ScriptDialectComboBox.ItemsSource = RobotScriptDialects.All
            .Where(dialect => dialect.Status == RobotScriptDialectStatus.Available);
        ScriptDialectComboBox.SelectedItem = RobotScriptDialects.SimpleDsl;
        ScaraScriptDialectComboBox.ItemsSource = RobotScriptDialects.All
            .Where(dialect => dialect.Status == RobotScriptDialectStatus.Available);
        ScaraScriptDialectComboBox.SelectedItem = RobotScriptDialects.SimpleDsl;
        SimpleArmScriptDialectComboBox.ItemsSource = RobotScriptDialects.All
            .Where(dialect => dialect.Status == RobotScriptDialectStatus.Available);
        SimpleArmScriptDialectComboBox.SelectedItem = RobotScriptDialects.SimpleDsl;
        DeltaScriptDialectComboBox.ItemsSource = RobotScriptDialects.All
            .Where(dialect => dialect.Status == RobotScriptDialectStatus.Available);
        DeltaScriptDialectComboBox.SelectedItem = RobotScriptDialects.SimpleDsl;
        IndustrialArmScriptDialectComboBox.ItemsSource = RobotScriptDialects.All
            .Where(dialect => dialect.Status == RobotScriptDialectStatus.Available);
        IndustrialArmScriptDialectComboBox.SelectedItem = RobotScriptDialects.SimpleDsl;
        ScriptEditorTextBox.Text = GetCartesianExampleScript(RobotViewerKind.CartesianThreeDimensional);
        GlossaryCategoryComboBox.ItemsSource = new object[] { "All topics" }
            .Concat(Enum.GetValues<GlossaryCategory>().Cast<object>())
            .ToArray();
        GlossaryCategoryComboBox.SelectedIndex = 0;
        RefreshGlossaryEntries();
        RefreshScriptEditorGutter();
        RefreshGCodeExplanations();
        BuildRobotSelectionCards();
    }

    private void MainWindow_PreviewKeyDown(
        object sender,
        KeyEventArgs e)
    {
        var isControlPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

        if (isControlPressed && e.Key == Key.G &&
            RobotSelectionView.Visibility == Visibility.Visible)
        {
            ToggleGlossary();
            e.Handled = true;
            return;
        }

        if (GlossaryOverlay.Visibility == Visibility.Visible)
        {
            if (e.Key == Key.Escape)
            {
                CloseGlossary();
                e.Handled = true;
            }

            return;
        }

        if (RobotSelectionView.Visibility == Visibility.Visible)
        {
            return;
        }

        if (isControlPressed && e.Key == Key.O)
        {
            LoadActiveScript();
            e.Handled = true;
            return;
        }

        if (isControlPressed && e.Key == Key.S)
        {
            SaveActiveScript();
            e.Handled = true;
            return;
        }

        if (isControlPressed && e.Key == Key.Enter)
        {
            SimulateActiveScript();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F5)
        {
            SimulateActiveScript();
            e.Handled = true;
            return;
        }

        if (isControlPressed && e.Key == Key.R)
        {
            ResetActivePlayback();
            e.Handled = true;
            return;
        }

        if (IsTextInputFocused())
        {
            return;
        }

        if (e.Key == Key.Space)
        {
            TogglePlayback();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Left)
        {
            MoveActiveFrame(delta: -1);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Right)
        {
            MoveActiveFrame(delta: 1);
            e.Handled = true;
            return;
        }

        if (isControlPressed && IsZoomInKey(e.Key))
        {
            ZoomActiveCamera(delta: -0.12);
            e.Handled = true;
            return;
        }

        if (isControlPressed && IsZoomOutKey(e.Key))
        {
            ZoomActiveCamera(delta: 0.12);
            e.Handled = true;
            return;
        }

        if (isControlPressed && IsZeroKey(e.Key))
        {
            ResetActiveCamera();
            e.Handled = true;
        }
    }

    private void MainWindow_PreviewMouseWheel(
        object sender,
        MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) ||
            RobotSelectionView.Visibility == Visibility.Visible ||
            !IsPointerOverActiveViewer())
        {
            return;
        }

        e.Handled = true;
        ZoomActiveCamera(e.Delta > 0 ? -0.12 : 0.12);
    }

    private void PlayPauseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        TogglePlayback();
    }

    private void PlayPauseButton_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button button && !playPauseButtons.Contains(button))
        {
            playPauseButtons.Add(button);
        }

        UpdatePlaybackButtonLabels();
    }

    private void ResetActivePlaybackButton_Click(
        object sender,
        RoutedEventArgs e) =>
        ResetActivePlayback();

    private void TogglePlayback()
    {
        if (!isPlaying && !TryPreparePlayback())
        {
            return;
        }

        isPlaying = !isPlaying;
        UpdatePlaybackButtonLabels();

        if (isPlaying)
        {
            playbackRenderTimeline = CreateActiveRenderTimeline();
            playbackStartPosition = GetActiveFrameTime();
            playbackStopwatch.Restart();
            playbackTimer.Start();
        }
        else
        {
            playbackTimer.Stop();
            playbackStopwatch.Reset();
        }
    }

    private bool TryPreparePlayback()
    {
        if (activeViewerKind is not (
            RobotViewerKind.CartesianThreeDimensional or
            RobotViewerKind.XYPlotterTwoDimensional) ||
            snapshot is not null)
        {
            return true;
        }

        if (!TryCreateSnapshotFromScript(
            ScriptEditorTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
        {
            SetScriptStatus(message, Color.FromRgb(248, 113, 113));
            return false;
        }

        snapshot = nextSnapshot;
        InitializeTimelineForSnapshot();
        RenderFrame(index: 0);
        SetScriptStatus(message, Color.FromRgb(74, 222, 128));
        return true;
    }

    private void ResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (snapshot is null)
        {
            return;
        }

        StopPlayback();
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

    private void DeltaResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (deltaSnapshot is null)
        {
            return;
        }

        StopPlayback();
        RenderDeltaFrame(index: 0);
    }

    private void DroneResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (droneSnapshot is null)
        {
            return;
        }

        StopPlayback();
        RenderDroneFrame(index: 0);
    }

    private void IndustrialArmResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (industrialArmSnapshot is null)
        {
            return;
        }

        StopPlayback();
        RenderIndustrialArmFrame(index: 0);
    }


}
