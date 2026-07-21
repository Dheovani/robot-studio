using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using RobotStudio.Desktop.Robots;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Scripting;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop;

public partial class MainWindow : Window
{
    private const string ExampleScript =
        """
        HOME
        MOVE X=120 Y=80 Z=40 SPEED=90
        WAIT 500
        """;

    private readonly DispatcherTimer playbackTimer;
    private CartesianPlaybackSnapshot? snapshot;
    private int currentFrameIndex;
    private bool isPlaying;
    private double baseCameraDistanceMillimeters;
    private double azimuthDegrees = -45;
    private double elevationDegrees = 35;
    private double zoomMultiplier = 1;
    private bool isRotatingCamera;
    private Point lastMousePosition;

    public MainWindow()
    {
        InitializeComponent();

        playbackTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(120)
        };
        playbackTimer.Tick += PlaybackTimer_Tick;

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        BuildRobotSelectionCards();
    }

    private void PlayPauseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        isPlaying = !isPlaying;
        PlayPauseButton.Content = isPlaying ? "Pause" : "Play";

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

        foreach (var primitive in sceneFrame.Primitives)
        {
            sceneRoot.Children.Add(CreateModel(primitive));
        }

        RobotViewport.Children.Add(new ModelVisual3D { Content = sceneRoot });

        StatusText.Text =
            $"Frame {currentFrameIndex + 1}/{snapshot.SceneFrameCount} | " +
            $"t={sceneFrame.Time.TotalSeconds:0.###}s | {sceneFrame.State}";
        UpdateStatePanel(sceneFrame);
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
            azimuth: NormalizeDegrees(azimuthDegrees - (deltaX * 0.35)),
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

    private static CartesianPlaybackSnapshot CreateSnapshot()
    {
        var profile = CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));
        var initialPosition = new CartesianPosition(X: 40, Y: 30, Z: 20);
        var commands = new RobotScriptParser().Parse(ExampleScript);
        var context = SimulationContext.Create(profile, initialPosition);
        var result = new RobotSimulator().Execute(context, commands);

        return new CartesianPlaybackSnapshotBuilder()
            .Build(profile, result, TimeSpan.FromMilliseconds(100));
    }

    private void BuildRobotSelectionCards()
    {
        RobotCardsPanel.Children.Clear();

        foreach (var template in RobotCatalog.Templates)
        {
            RobotCardsPanel.Children.Add(CreateRobotCard(template));
        }
    }

    private UIElement CreateRobotCard(RobotTemplate template)
    {
        var card = new Border
        {
            Width = 350,
            Height = 306,
            Margin = new Thickness(0, 0, 18, 18),
            Padding = new Thickness(20),
            BorderBrush = template.Status == RobotAvailabilityStatus.Available
                ? new SolidColorBrush(Color.FromRgb(37, 99, 235))
                : new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            CornerRadius = new CornerRadius(8)
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
            Content = template.Status == RobotAvailabilityStatus.Available
                ? "Open Robot"
                : "Planned",
            IsEnabled = template.Status == RobotAvailabilityStatus.Available,
            Tag = template
        };
        button.Click += OpenRobotButton_Click;
        Grid.SetRow(button, 2);
        content.Children.Add(button);

        return card;
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
        if (template.Viewer.Kind != RobotViewerKind.CartesianThreeDimensional)
        {
            return;
        }

        RobotSelectionView.Visibility = Visibility.Collapsed;
        CartesianViewerView.Visibility = Visibility.Visible;
        EnsureCartesianSnapshot();
        RenderFrame(index: 0);
    }

    private void EnsureCartesianSnapshot()
    {
        if (snapshot is not null)
        {
            return;
        }

        snapshot = CreateSnapshot();
        baseCameraDistanceMillimeters = CalculateDistance(
            snapshot.Viewport.Target,
            snapshot.Viewport.CameraPosition);
        TimelineSlider.Maximum = snapshot.SceneFrameCount - 1;
        TimelineSlider.TickFrequency = 1;
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
    }

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
        RobotCapability.ScriptExecution => "DSL",
        RobotCapability.ThreeDimensionalView => "3D view",
        RobotCapability.ManualControl => "manual control",
        RobotCapability.HardwareCommunication => "hardware",
        RobotCapability.GCode => "G-code",
        _ => capability.ToString()
    };

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

    private static GeometryModel3D CreateBoxModel(CartesianScenePrimitive primitive) =>
        new(
            CreateBoxMesh(primitive.Center, primitive.Size),
            new DiffuseMaterial(new SolidColorBrush(GetColor(primitive.Kind))))
        {
            BackMaterial = new DiffuseMaterial(new SolidColorBrush(GetColor(primitive.Kind)))
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

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;

    private static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360;
        if (normalized > 180)
        {
            normalized -= 360;
        }

        if (normalized < -180)
        {
            normalized += 360;
        }

        return normalized;
    }
}
