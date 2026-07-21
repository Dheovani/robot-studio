using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
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
    private CartesianPlaybackSnapshot snapshot = null!;
    private int currentFrameIndex;
    private bool isPlaying;

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
        snapshot = CreateSnapshot();
        TimelineSlider.Maximum = snapshot.SceneFrameCount - 1;
        TimelineSlider.TickFrequency = 1;

        RenderFrame(index: 0);
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
        var nextFrame = currentFrameIndex + 1;
        if (nextFrame >= snapshot.SceneFrameCount)
        {
            nextFrame = 0;
        }

        RenderFrame(nextFrame);
    }

    private void RenderFrame(int index)
    {
        currentFrameIndex = Math.Clamp(index, 0, snapshot.SceneFrameCount - 1);
        TimelineSlider.Value = currentFrameIndex;

        var sceneFrame = snapshot.SceneFrames[currentFrameIndex];

        RobotViewport.Children.Clear();
        RobotViewport.Camera = CreateCamera(snapshot.Viewport);

        var sceneRoot = new Model3DGroup();
        sceneRoot.Children.Add(new AmbientLight(Color.FromRgb(92, 105, 130)));
        sceneRoot.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -2)));

        foreach (var primitive in sceneFrame.Primitives)
        {
            sceneRoot.Children.Add(CreateBoxModel(primitive));
        }

        RobotViewport.Children.Add(new ModelVisual3D { Content = sceneRoot });

        StatusText.Text =
            $"Frame {currentFrameIndex + 1}/{snapshot.SceneFrameCount} | " +
            $"t={sceneFrame.Time.TotalSeconds:0.###}s | {sceneFrame.State}";
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

    private static PerspectiveCamera CreateCamera(CartesianViewportSnapshot viewport) =>
        new()
        {
            Position = ToPoint3D(viewport.CameraPosition),
            LookDirection = ToVector3D(Subtract(viewport.Target, viewport.CameraPosition)),
            UpDirection = ToVector3D(viewport.Up),
            NearPlaneDistance = viewport.NearClipMillimeters,
            FarPlaneDistance = viewport.FarClipMillimeters,
            FieldOfView = 45
        };

    private static GeometryModel3D CreateBoxModel(CartesianScenePrimitive primitive) =>
        new(
            CreateBoxMesh(primitive.Center, primitive.Size),
            new DiffuseMaterial(new SolidColorBrush(GetColor(primitive.Kind))))
        {
            BackMaterial = new DiffuseMaterial(new SolidColorBrush(GetColor(primitive.Kind)))
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
}
