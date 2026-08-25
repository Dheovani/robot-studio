using System.Diagnostics;
using System.IO;
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
using HelixToolkit.SharpDX.Model;
using HelixToolkit.SharpDX.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using RobotStudio.Desktop.Rendering;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;
using RobotStudio.Visualization.Assets;
using MediaColor = System.Windows.Media.Color;
using MeshGeometry3D = HelixToolkit.SharpDX.MeshGeometry3D;

namespace RobotStudio.Desktop.Showcases;

public partial class MechanicalShowcaseView : UserControl
{
    private const float MillimetersPerSceneUnit = 100;
    private const double InitialAzimuthDegrees = 48;
    private const double InitialElevationDegrees = 28;
    private const double InitialCameraDistance = 18;

    private static readonly Point3D CameraTarget = new(0, 0, 3.4);

    private readonly MechanicalShowcaseDefinition showcase = CartesianMechanicalShowcaseDefinition.Create();
    private readonly Dictionary<RobotPartId, List<MeshGeometryModel3D>> modelsByPart = [];
    private readonly Dictionary<MeshGeometryModel3D, PhongMaterial> normalMaterials = [];
    private readonly Dictionary<MeshGeometryModel3D, PhongMaterial> transparentMaterials = [];
    private readonly Dictionary<MaterialGeometryNode, MaterialCore?> importedMaterials = [];
    private readonly DefaultEffectsManager effectsManager = new();
    private readonly HelixToolkit.Wpf.SharpDX.PerspectiveCamera camera = new();
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private readonly Stopwatch stopwatch = new();
    private readonly ViewportOrbitInteractionState orbitInteraction = new();
    private readonly PhongMaterial selectionMaterial = Material(
        diffuse: new Color4(1f, 0.72f, 0.12f, 1f),
        specular: new Color4(1f, 0.9f, 0.55f, 1f),
        shininess: 90);
    private readonly PhongMaterial transparentSelectionMaterial = Material(
        diffuse: new Color4(1f, 0.72f, 0.12f, 0.32f),
        specular: new Color4(1f, 0.9f, 0.55f, 0.32f),
        shininess: 90);
    private readonly PhongMaterial driveMotorMaterial = Material(
        diffuse: new Color4(0.95f, 0.38f, 0.08f, 1f),
        specular: new Color4(1f, 0.82f, 0.55f, 1f),
        shininess: 95);
    private readonly PhongMaterial driveTransmissionMaterial = Material(
        diffuse: new Color4(0.95f, 0.72f, 0.08f, 1f),
        specular: new Color4(1f, 0.92f, 0.55f, 1f),
        shininess: 80);
    private readonly PhongMaterial driveRailMaterial = Material(
        diffuse: new Color4(0.18f, 0.72f, 0.9f, 1f),
        specular: new Color4(0.75f, 0.95f, 1f, 1f),
        shininess: 110);
    private readonly PhongMaterialCore importedSelectionMaterial = new()
    {
        DiffuseColor = new Color4(1f, 0.72f, 0.12f, 1f),
        AmbientColor = new Color4(0.25f, 0.18f, 0.03f, 1f),
        SpecularColor = new Color4(1f, 0.9f, 0.55f, 1f),
        SpecularShininess = 90
    };

    private TimeSpan playbackOffset;
    private ImportedRobotVisualScene? importedScene;
    private SceneNodeGroupModel3D? importedSceneHost;
    private RobotPartId? selectedPartId;
    private double cameraAzimuthDegrees = InitialAzimuthDegrees;
    private double cameraElevationDegrees = InitialElevationDegrees;
    private double cameraDistance = InitialCameraDistance;

    public MechanicalShowcaseView()
    {
        InitializeComponent();

        ShowcaseViewport.EffectsManager = effectsManager;
        ShowcaseViewport.Camera = camera;
        ShowcaseViewport.MouseDown3D += ShowcaseViewport_MouseDown3D;
        ApplyCamera();

        BuildScene();
        TeachingViewComboBox.ItemsSource = MechanicalTeachingViewCatalog.Options;
        TeachingViewComboBox.SelectedIndex = 0;
        SelectPart(showcase.Model.Parts.Single(part => part.Kind == RobotPartKind.Tool).Id);
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
        BuildProceduralScene();
        TryLoadImportedScene();
    }

    private void BuildProceduralScene()
    {
        var frame = Material(new Color4(0.28f, 0.32f, 0.38f, 1), new Color4(0.85f, 0.9f, 0.98f, 1), 115);
        var darkMetal = Material(new Color4(0.08f, 0.1f, 0.14f, 1), new Color4(0.5f, 0.56f, 0.65f, 1), 100);
        var steel = Material(new Color4(0.48f, 0.54f, 0.62f, 1), new Color4(0.95f, 0.98f, 1f, 1), 125);
        var accent = Material(new Color4(0.06f, 0.34f, 0.72f, 1), new Color4(0.5f, 0.78f, 1f, 1), 85);
        var bed = Material(new Color4(0.16f, 0.19f, 0.24f, 1), new Color4(0.65f, 0.72f, 0.8f, 1), 95);
        var motor = Material(new Color4(0.12f, 0.14f, 0.18f, 1), new Color4(0.75f, 0.8f, 0.9f, 1), 110);
        var belt = Material(new Color4(0.035f, 0.04f, 0.05f, 1), new Color4(0.2f, 0.22f, 0.25f, 1), 45);
        var tool = Material(new Color4(0.9f, 0.32f, 0.06f, 1), new Color4(1f, 0.8f, 0.5f, 1), 90);

        AddBox("base", new Vector3(0, 0, 0.25f), new Vector3(9.5f, 8, 0.5f), darkMetal);
        AddBox("base", new Vector3(-4.25f, -3.5f, 0.05f), new Vector3(0.65f, 0.65f, 0.3f), darkMetal);
        AddBox("base", new Vector3(4.25f, -3.5f, 0.05f), new Vector3(0.65f, 0.65f, 0.3f), darkMetal);
        AddBox("controller", new Vector3(3.55f, -3.2f, 0.95f), new Vector3(1.7f, 1.1f, 1.25f), accent);

        AddBox("left-y-rail", new Vector3(-2.45f, -0.45f, 0.78f), new Vector3(0.22f, 5.8f, 0.22f), steel);
        AddBox("right-y-rail", new Vector3(2.45f, -0.45f, 0.78f), new Vector3(0.22f, 5.8f, 0.22f), steel);
        AddCylinder("y-motor", new Vector3(0, -3.65f, 0.78f), new Vector3(0, -3.05f, 0.78f), 0.42f, motor);
        AddBox("y-belt", new Vector3(0, -0.45f, 0.82f), new Vector3(0.12f, 5.7f, 0.1f), belt);
        AddBox("y-bed-carriage", new Vector3(0, -0.8f, 1.02f), new Vector3(6.7f, 5.5f, 0.35f), frame);
        AddBox("build-plate", new Vector3(0, -0.8f, 1.27f), new Vector3(6.3f, 5.1f, 0.16f), bed);

        AddBox("left-frame-column", new Vector3(-4.05f, 2.65f, 4.35f), new Vector3(0.5f, 0.55f, 7.2f), frame);
        AddBox("right-frame-column", new Vector3(4.05f, 2.65f, 4.35f), new Vector3(0.5f, 0.55f, 7.2f), frame);
        AddBox("top-frame-beam", new Vector3(0, 2.65f, 7.95f), new Vector3(8.6f, 0.55f, 0.5f), frame);
        AddCylinder("left-z-guide", new Vector3(-3.7f, 2.4f, 1.05f), new Vector3(-3.7f, 2.4f, 7.55f), 0.11f, steel);
        AddCylinder("right-z-guide", new Vector3(3.7f, 2.4f, 1.05f), new Vector3(3.7f, 2.4f, 7.55f), 0.11f, steel);
        AddCylinder("left-z-screw", new Vector3(-3.45f, 2.8f, 1.1f), new Vector3(-3.45f, 2.8f, 7.55f), 0.09f, steel);
        AddCylinder("right-z-screw", new Vector3(3.45f, 2.8f, 1.1f), new Vector3(3.45f, 2.8f, 7.55f), 0.09f, steel);
        AddCylinder("left-z-motor", new Vector3(-3.45f, 2.8f, 0.65f), new Vector3(-3.45f, 2.8f, 1.15f), 0.38f, motor);
        AddCylinder("right-z-motor", new Vector3(3.45f, 2.8f, 0.65f), new Vector3(3.45f, 2.8f, 1.15f), 0.38f, motor);

        AddBox("z-gantry", new Vector3(0, 2.5f, 5.4f), new Vector3(8.2f, 0.62f, 0.62f), accent);
        AddBox("z-gantry", new Vector3(-3.7f, 2.5f, 5.4f), new Vector3(0.75f, 0.9f, 0.9f), frame);
        AddBox("z-gantry", new Vector3(3.7f, 2.5f, 5.4f), new Vector3(0.75f, 0.9f, 0.9f), frame);
        AddBox("x-rail", new Vector3(0, 2.15f, 5.4f), new Vector3(7.25f, 0.18f, 0.22f), steel);
        AddBox("x-belt", new Vector3(0, 2.02f, 5.65f), new Vector3(7.1f, 0.1f, 0.1f), belt);
        AddCylinder("x-motor", new Vector3(-4.2f, 2.5f, 5.4f), new Vector3(-3.7f, 2.5f, 5.4f), 0.4f, motor);
        AddBox("x-tool-carriage", new Vector3(-1.6f, 1.92f, 5.35f), new Vector3(0.9f, 0.75f, 1.05f), accent);
        AddBox("tool", new Vector3(-1.6f, 1.65f, 4.65f), new Vector3(0.62f, 0.62f, 0.5f), darkMetal);
        AddCylinder("tool", new Vector3(-1.6f, 1.65f, 4.45f), new Vector3(-1.6f, 1.65f, 3.9f), 0.16f, tool);
    }

    private void TryLoadImportedScene()
    {
        try
        {
            var manifestPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Robots",
                "CartesianMechanical",
                "robot.json");
            var package = new RobotVisualAssetPackageLoader().Load(manifestPath, showcase.Model);
            importedScene = new HelixRobotVisualAssetImporter().Import(package);

            foreach (var materialNode in importedScene.NodesByPart.Values
                         .SelectMany(nodes => nodes)
                         .OfType<MaterialGeometryNode>()
                         .Distinct())
            {
                importedMaterials.Add(materialNode, materialNode.Material);
            }

            importedSceneHost = new SceneNodeGroupModel3D();
            importedSceneHost.AddNode(importedScene.Root);
            ShowcaseViewport.Items.Add(importedSceneHost);
        }
        catch (RobotVisualAssetException exception)
        {
            Debug.WriteLine($"Mechanical showcase asset fallback: {exception.Message}");
            importedScene = null;
        }
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

    private void ShowcaseViewport_MouseDown3D(object? sender, RoutedEventArgs eventArgs)
    {
        if (eventArgs is Mouse3DEventArgs
            {
                HitTestResult.ModelHit: SceneNode { Tag: RobotPartId partId }
            })
        {
            SelectPart(partId);
        }
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
        transparentMaterials.Add(model, TransparentMaterial(material));
        ShowcaseViewport.Items.Add(model);
    }

    private void SelectPart(RobotPartId partId)
    {
        if (selectedPartId is RobotPartId previousId && modelsByPart.TryGetValue(previousId, out var previousModels))
        {
            selectedPartId = null;
            ApplyPartAppearance(previousId, previousModels);
        }

        selectedPartId = partId;
        ApplyPartAppearance(partId, modelsByPart[partId]);

        var part = showcase.Model.GetPart(partId);
        PartNameText.Text = part.Name;
        PartKindText.Text = part.Kind.ToString();
        PartRelationshipText.Text = part.ParentId is RobotPartId parentId
            ? $"Mounted to: {showcase.Model.GetPart(parentId).Name}"
            : "Root component";
        PartFunctionText.Text = part.Function;
        PartMovementText.Text = part.Movement;
        ApplyImportedSelection();
    }

    private void TeachingViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TeachingViewComboBox.SelectedItem is not MechanicalTeachingViewOption option)
        {
            return;
        }

        TeachingViewDescriptionText.Text = option.Description;
        ApplyTeachingView();
    }

    private void ApplyTeachingView()
    {
        var useImportedScene = importedScene is not null &&
                               TeachingViewComboBox.SelectedItem is MechanicalTeachingViewOption
                               {
                                   Mode: MechanicalTeachingViewMode.Assembled
                               };
        if (importedScene is not null)
        {
            importedScene.Root.Visible = useImportedScene;
        }

        foreach (var (partId, models) in modelsByPart)
        {
            foreach (var model in models)
            {
                model.Visibility = useImportedScene ? Visibility.Collapsed : Visibility.Visible;
            }

            ApplyPartAppearance(partId, models);
        }

        ApplyImportedSelection();
    }

    private void ApplyPartAppearance(
        RobotPartId partId,
        IReadOnlyList<MeshGeometryModel3D> models)
    {
        var part = showcase.Model.GetPart(partId);
        var isDriveView = TeachingViewComboBox.SelectedItem is MechanicalTeachingViewOption
        {
            Mode: MechanicalTeachingViewMode.DriveSystem
        };
        var isGhosted = isDriveView && MechanicalTeachingViewCatalog.ShouldGhost(part.Kind);
        var isSelected = selectedPartId == partId;

        foreach (var model in models)
        {
            model.IsTransparent = isGhosted;
            model.Material = SelectMaterial(
                model,
                part.Kind,
                isDriveView,
                isGhosted,
                isSelected);
        }
    }

    private PhongMaterial SelectMaterial(
        MeshGeometryModel3D model,
        RobotPartKind kind,
        bool isDriveView,
        bool isGhosted,
        bool isSelected)
    {
        if (isSelected)
        {
            return isGhosted ? transparentSelectionMaterial : selectionMaterial;
        }

        return isGhosted
            ? transparentMaterials[model]
            : GetOpaqueMaterial(model, kind, isDriveView);
    }

    private PhongMaterial GetOpaqueMaterial(
        MeshGeometryModel3D model,
        RobotPartKind kind,
        bool isDriveView) =>
        isDriveView
            ? kind switch
            {
                RobotPartKind.Motor => driveMotorMaterial,
                RobotPartKind.Transmission => driveTransmissionMaterial,
                RobotPartKind.Rail => driveRailMaterial,
                _ => normalMaterials[model]
            }
            : normalMaterials[model];

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

        ApplyImportedPoses(poses);

        DemonstrationProgressBar.Value = Math.Clamp(time.TotalSeconds / demonstration.Duration.TotalSeconds, 0, 1);
        DemonstrationTimeText.Text = $"{time.TotalSeconds:0.0} / {demonstration.Duration.TotalSeconds:0.0} s";
    }

    private void ApplyImportedPoses(IReadOnlyList<RobotComponentPose> poses)
    {
        if (importedScene is null)
        {
            return;
        }

        var posesByPart = poses.ToDictionary(pose => pose.PartId);
        foreach (var (partId, rootNodes) in importedScene.RootNodesByPart)
        {
            var pose = posesByPart.GetValueOrDefault(partId, RobotComponentPose.Identity(partId));
            var transform = Matrix4x4.CreateScale(pose.Scale) *
                            Matrix4x4.CreateFromQuaternion(pose.Rotation) *
                            Matrix4x4.CreateTranslation(pose.TranslationMillimeters / MillimetersPerSceneUnit);
            foreach (var rootNode in rootNodes)
            {
                rootNode.ModelMatrix = transform;
            }
        }
    }

    private void ApplyImportedSelection()
    {
        foreach (var (node, material) in importedMaterials)
        {
            node.Material = material;
        }

        if (importedScene is null || selectedPartId is not RobotPartId partId ||
            !importedScene.NodesByPart.TryGetValue(partId, out var selectedNodes))
        {
            return;
        }

        foreach (var materialNode in selectedNodes.OfType<MaterialGeometryNode>())
        {
            materialNode.Material = importedSelectionMaterial;
        }
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
            AmbientColor = new Color4(
                diffuse.Red * 0.25f,
                diffuse.Green * 0.25f,
                diffuse.Blue * 0.25f,
                diffuse.Alpha)
        };

    private static PhongMaterial TransparentMaterial(PhongMaterial source)
    {
        const float opacity = 0.14f;
        var diffuse = source.DiffuseColor;
        var specular = source.SpecularColor;

        return Material(
            new Color4(diffuse.Red, diffuse.Green, diffuse.Blue, opacity),
            new Color4(specular.Red, specular.Green, specular.Blue, opacity),
            source.SpecularShininess);
    }

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
        importedSceneHost?.Clear(detachChildren: true);
        importedScene?.Dispose();
        effectsManager.Dispose();
    }
}
