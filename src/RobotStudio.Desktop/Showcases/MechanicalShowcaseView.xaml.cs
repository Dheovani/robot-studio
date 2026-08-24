using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Geometry;
using HelixToolkit.Maths;
using HelixToolkit.SharpDX;
using HelixToolkit.Wpf.SharpDX;
using RobotStudio.Desktop.Rendering;
using RobotStudio.Visualization;
using MediaColor = System.Windows.Media.Color;
using MeshGeometry3D = HelixToolkit.SharpDX.MeshGeometry3D;

namespace RobotStudio.Desktop.Showcases;

public partial class MechanicalShowcaseView : UserControl
{
    private const float MillimetersPerSceneUnit = 100;
    private const double InitialAzimuthDegrees = 48;
    private const double InitialElevationDegrees = 28;
    private const double InitialCameraDistance = 17;

    private static readonly Point3D CameraTarget = new(0, 0, 2.3);

    private readonly MechanicalShowcaseDefinition showcase = CartesianMechanicalShowcaseDefinition.Create();
    private readonly Dictionary<RobotPartId, List<MeshGeometryModel3D>> modelsByPart = [];
    private readonly Dictionary<MeshGeometryModel3D, PhongMaterial> normalMaterials = [];
    private readonly DefaultEffectsManager effectsManager = new();
    private readonly HelixToolkit.Wpf.SharpDX.PerspectiveCamera camera = new();
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private readonly Stopwatch stopwatch = new();
    private readonly ViewportOrbitInteractionState orbitInteraction = new();
    private readonly PhongMaterial selectionMaterial = Material(
        diffuse: new Color4(1f, 0.72f, 0.12f, 1f),
        specular: new Color4(1f, 0.9f, 0.55f, 1f),
        shininess: 90);

    private TimeSpan playbackOffset;
    private RobotPartId? selectedPartId;
    private double cameraAzimuthDegrees = InitialAzimuthDegrees;
    private double cameraElevationDegrees = InitialElevationDegrees;
    private double cameraDistance = InitialCameraDistance;

    public MechanicalShowcaseView()
    {
        InitializeComponent();

        ShowcaseViewport.EffectsManager = effectsManager;
        ShowcaseViewport.Camera = camera;
        ApplyCamera();

        BuildScene();
        DemonstrationComboBox.ItemsSource = showcase.Demonstrations;
        DemonstrationComboBox.SelectedIndex = 0;
        timer.Tick += Timer_Tick;
        Unloaded += MechanicalShowcaseView_Unloaded;
        ResetDemonstration();
    }

    public event EventHandler? BackRequested;

    private MechanicalDemonstrationDefinition? SelectedDemonstration =>
        DemonstrationComboBox.SelectedItem as MechanicalDemonstrationDefinition;

    private void BuildScene()
    {
        AddGrid();

        var darkMetal = Material(new Color4(0.12f, 0.15f, 0.2f, 1), new Color4(0.55f, 0.6f, 0.7f, 1), 100);
        var steel = Material(new Color4(0.34f, 0.4f, 0.48f, 1), new Color4(0.85f, 0.9f, 1f, 1), 120);
        var paintedBlue = Material(new Color4(0.08f, 0.32f, 0.72f, 1), new Color4(0.5f, 0.75f, 1f, 1), 80);
        var carriage = Material(new Color4(0.08f, 0.65f, 0.4f, 1), new Color4(0.65f, 1f, 0.8f, 1), 75);
        var motor = Material(new Color4(0.16f, 0.18f, 0.23f, 1), new Color4(0.75f, 0.8f, 0.9f, 1), 110);
        var tool = Material(new Color4(0.88f, 0.35f, 0.08f, 1), new Color4(1f, 0.8f, 0.55f, 1), 90);

        AddBox("base", new Vector3(0, 0, 0.2f), new Vector3(9, 6.5f, 0.4f), darkMetal);
        AddBox("controller", new Vector3(-3.6f, 2.5f, 1.15f), new Vector3(1.25f, 0.9f, 1.5f), paintedBlue);

        AddBox("x-rail", new Vector3(0, -2.2f, 0.75f), new Vector3(7.5f, 0.22f, 0.28f), steel);
        AddBox("x-rail", new Vector3(0, 2.2f, 0.75f), new Vector3(7.5f, 0.22f, 0.28f), steel);
        AddCylinder("x-motor", new Vector3(-4.2f, -2.2f, 0.75f), new Vector3(-3.65f, -2.2f, 0.75f), 0.42f, motor);
        AddBox("x-carriage", new Vector3(-1.1f, 0, 0.95f), new Vector3(1.1f, 5, 0.45f), carriage);

        AddBox("y-rail", new Vector3(-1.1f, 0, 1.35f), new Vector3(0.35f, 4.2f, 0.32f), paintedBlue);
        AddCylinder("y-motor", new Vector3(-1.1f, -2.75f, 1.35f), new Vector3(-1.1f, -2.15f, 1.35f), 0.38f, motor);
        AddBox("y-carriage", new Vector3(-1.1f, -0.7f, 1.55f), new Vector3(0.9f, 0.9f, 0.5f), carriage);

        AddBox("z-column", new Vector3(-1.1f, -0.7f, 3.5f), new Vector3(0.42f, 0.42f, 3.5f), paintedBlue);
        AddCylinder("z-motor", new Vector3(-1.1f, -0.7f, 5.65f), new Vector3(-1.1f, -0.7f, 5.05f), 0.4f, motor);
        AddBox("z-carriage", new Vector3(-1.1f, -0.7f, 3.15f), new Vector3(0.95f, 0.95f, 0.7f), carriage);
        AddCylinder("tool", new Vector3(-1.1f, -0.7f, 2.8f), new Vector3(-1.1f, -0.7f, 1.85f), 0.22f, tool);
        AddBox("tool", new Vector3(-1.1f, -0.7f, 1.7f), new Vector3(0.65f, 0.65f, 0.3f), tool);
    }

    private void AddGrid()
    {
        var geometry = LineBuilder.GenerateGrid(Vector3.UnitZ, -6, 6, -5, 5);
        ShowcaseViewport.Items.Add(new LineGeometryModel3D
        {
            Geometry = geometry,
            Color = MediaColor.FromRgb(38, 52, 74),
            Thickness = 0.8,
            IsHitTestVisible = false
        });
    }

    private void AddBox(string partId, Vector3 center, Vector3 size, PhongMaterial material)
    {
        var builder = new MeshBuilder();
        builder.AddBox(center, size.X, size.Y, size.Z);
        AddModel(new RobotPartId(partId), builder.ToMeshGeometry3D(), material);
    }

    private void AddCylinder(
        string partId,
        Vector3 start,
        Vector3 end,
        float radius,
        PhongMaterial material)
    {
        var builder = new MeshBuilder();
        builder.AddCylinder(start, end, radius, 32, true, true);
        AddModel(new RobotPartId(partId), builder.ToMeshGeometry3D(), material);
    }

    private void AddModel(RobotPartId partId, MeshGeometry3D geometry, PhongMaterial material)
    {
        var model = new MeshGeometryModel3D
        {
            Geometry = geometry,
            Material = material,
            IsHitTestVisible = showcase.Model.GetPart(partId).IsSelectable,
            Tag = partId
        };
        model.MouseDown3D += (_, _) => SelectPart(partId);

        if (!modelsByPart.TryGetValue(partId, out var models))
        {
            models = [];
            modelsByPart.Add(partId, models);
        }

        models.Add(model);
        normalMaterials.Add(model, material);
        ShowcaseViewport.Items.Add(model);
    }

    private void SelectPart(RobotPartId partId)
    {
        if (selectedPartId is RobotPartId previousId && modelsByPart.TryGetValue(previousId, out var previousModels))
        {
            foreach (var model in previousModels)
            {
                model.Material = normalMaterials[model];
            }
        }

        selectedPartId = partId;
        foreach (var model in modelsByPart[partId])
        {
            model.Material = selectionMaterial;
        }

        var part = showcase.Model.GetPart(partId);
        PartNameText.Text = part.Name;
        PartKindText.Text = part.Kind.ToString();
        PartRelationshipText.Text = part.ParentId is RobotPartId parentId
            ? $"Mounted to: {showcase.Model.GetPart(parentId).Name}"
            : "Root component";
        PartFunctionText.Text = part.Function;
        PartMovementText.Text = part.Movement;
    }

    private void ApplyDemonstrationTime(TimeSpan time)
    {
        var demonstration = SelectedDemonstration;
        if (demonstration is null)
        {
            return;
        }

        var poses = MechanicalDemonstrationSampler.Sample(demonstration, time);
        var transforms = RobotComponentPoseResolver.ResolveWorldTransforms(showcase.Model, poses);
        foreach (var (partId, models) in modelsByPart)
        {
            var transform = ToWpfTransform(transforms[partId]);
            foreach (var model in models)
            {
                model.Transform = transform;
            }
        }

        DemonstrationProgressBar.Value = Math.Clamp(time.TotalSeconds / demonstration.Duration.TotalSeconds, 0, 1);
        DemonstrationTimeText.Text = $"{time.TotalSeconds:0.0} / {demonstration.Duration.TotalSeconds:0.0} s";
    }

    private static MatrixTransform3D ToWpfTransform(Matrix4x4 source)
    {
        var matrix = new Matrix3D(
            source.M11, source.M12, source.M13, source.M14,
            source.M21, source.M22, source.M23, source.M24,
            source.M31, source.M32, source.M33, source.M34,
            source.M41 / MillimetersPerSceneUnit,
            source.M42 / MillimetersPerSceneUnit,
            source.M43 / MillimetersPerSceneUnit,
            source.M44);
        return new MatrixTransform3D(matrix);
    }

    private static PhongMaterial Material(Color4 diffuse, Color4 specular, float shininess) =>
        new()
        {
            DiffuseColor = diffuse,
            SpecularColor = specular,
            SpecularShininess = shininess,
            AmbientColor = new Color4(diffuse.Red * 0.25f, diffuse.Green * 0.25f, diffuse.Blue * 0.25f, 1)
        };

    private void Timer_Tick(object? sender, EventArgs e)
    {
        var demonstration = SelectedDemonstration;
        if (demonstration is null)
        {
            return;
        }

        var time = playbackOffset + stopwatch.Elapsed;
        if (time >= demonstration.Duration)
        {
            time = demonstration.Duration;
            PauseDemonstration();
            playbackOffset = time;
        }

        ApplyDemonstrationTime(time);
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        var demonstration = SelectedDemonstration;
        if (demonstration is null)
        {
            return;
        }

        if (stopwatch.IsRunning)
        {
            PauseDemonstration();
            return;
        }

        if (playbackOffset >= demonstration.Duration)
        {
            playbackOffset = TimeSpan.Zero;
        }

        if (!stopwatch.IsRunning)
        {
            stopwatch.Restart();
            timer.Start();
            HeaderPlayButton.Content = "Pause";
        }
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        ResetDemonstration();
        ResetCamera();
    }

    private void DemonstrationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        ResetDemonstration();

    private void PauseDemonstration()
    {
        if (stopwatch.IsRunning)
        {
            playbackOffset += stopwatch.Elapsed;
        }

        stopwatch.Reset();
        timer.Stop();
        HeaderPlayButton.Content = "Play";
    }

    private void ResetDemonstration()
    {
        stopwatch.Reset();
        timer.Stop();
        playbackOffset = TimeSpan.Zero;
        HeaderPlayButton.Content = "Play";
        ApplyDemonstrationTime(TimeSpan.Zero);
    }

    private void ShowcaseViewportHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        orbitInteraction.BeginDrag(ShowcaseViewportHost, ShowcaseViewport, e);

    private void ShowcaseViewportHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
        orbitInteraction.EndDrag(ShowcaseViewportHost, e);

    private void ShowcaseViewportHost_MouseMove(object sender, MouseEventArgs e)
    {
        if (!orbitInteraction.TryGetDragDelta(ShowcaseViewport, e, out var deltaX, out var deltaY))
        {
            return;
        }

        cameraAzimuthDegrees = OrbitCameraFactory.NormalizeDegrees(
            cameraAzimuthDegrees - (deltaX * 0.35));
        cameraElevationDegrees = Math.Clamp(
            cameraElevationDegrees + (deltaY * 0.25),
            5,
            85);
        ApplyCamera();
        e.Handled = true;
    }

    private void ShowcaseViewportHost_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        cameraDistance = Math.Clamp(
            cameraDistance * (e.Delta > 0 ? 0.9 : 1.1),
            8,
            32);
        ApplyCamera();
        e.Handled = true;
    }

    private void ApplyCamera()
    {
        var reference = OrbitCameraFactory.Create(new OrbitCameraSettings(
            CameraTarget,
            cameraAzimuthDegrees,
            cameraElevationDegrees,
            cameraDistance,
            FieldOfView: 45,
            NearPlaneDistance: 0.05,
            FarPlaneDistance: 500));
        camera.Position = reference.Position;
        camera.LookDirection = reference.LookDirection;
        camera.UpDirection = reference.UpDirection;
        camera.FieldOfView = reference.FieldOfView;
        camera.NearPlaneDistance = reference.NearPlaneDistance;
        camera.FarPlaneDistance = reference.FarPlaneDistance;
    }

    private void ResetCamera()
    {
        cameraAzimuthDegrees = InitialAzimuthDegrees;
        cameraElevationDegrees = InitialElevationDegrees;
        cameraDistance = InitialCameraDistance;
        ApplyCamera();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        PauseDemonstration();
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MechanicalShowcaseView_Unloaded(object sender, RoutedEventArgs e)
    {
        timer.Stop();
        stopwatch.Stop();
        effectsManager.Dispose();
    }
}
