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
    private const double InitialAzimuthDegrees = -48;
    private const double InitialElevationDegrees = 28;
    private const double InitialCameraDistance = 18;
    private const double CameraFieldOfViewDegrees = 45;

    private static readonly Point3D InitialCameraTarget = new(0, 0, 3.4);

    private readonly MechanicalShowcasePresentation presentation;
    private readonly MechanicalShowcaseDefinition showcase;
    private readonly Dictionary<RobotPartId, List<MeshGeometryModel3D>> modelsByPart = [];
    private readonly Dictionary<MeshGeometryModel3D, RobotPartId?> motionAxisModels = [];
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
    private readonly PhongMaterialCore importedTransparentSelectionMaterial = CoreMaterial(
        new Color4(1f, 0.72f, 0.12f, 0.32f),
        new Color4(1f, 0.9f, 0.55f, 0.32f),
        90);
    private readonly PhongMaterialCore importedGhostMaterial = CoreMaterial(
        new Color4(0.38f, 0.45f, 0.56f, 0.14f),
        new Color4(0.75f, 0.84f, 0.96f, 0.14f),
        70);
    private readonly PhongMaterialCore importedDriveMotorMaterial = CoreMaterial(
        new Color4(0.95f, 0.38f, 0.08f, 1f),
        new Color4(1f, 0.82f, 0.55f, 1f),
        95);
    private readonly PhongMaterialCore importedDriveTransmissionMaterial = CoreMaterial(
        new Color4(0.95f, 0.72f, 0.08f, 1f),
        new Color4(1f, 0.92f, 0.55f, 1f),
        80);
    private readonly PhongMaterialCore importedDriveRailMaterial = CoreMaterial(
        new Color4(0.18f, 0.72f, 0.9f, 1f),
        new Color4(0.75f, 0.95f, 1f, 1f),
        110);

    private TimeSpan playbackOffset;
    private ImportedRobotVisualScene? importedScene;
    private SceneNodeGroupModel3D? importedSceneHost;
    private RobotPartId? selectedPartId;
    private double cameraAzimuthDegrees = InitialAzimuthDegrees;
    private double cameraElevationDegrees = InitialElevationDegrees;
    private double cameraDistance = InitialCameraDistance;
    private Point3D cameraTarget = InitialCameraTarget;
    private Point3D fittedCameraTarget = InitialCameraTarget;
    private double fittedCameraDistance = InitialCameraDistance;

    internal MechanicalShowcaseView(MechanicalShowcasePresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        this.presentation = presentation;
        showcase = presentation.Showcase;

        InitializeComponent();

        ShowcaseTitleText.Text = presentation.Title;
        ShowcaseSubtitleText.Text = presentation.Subtitle;
        ShowcaseViewport.EffectsManager = effectsManager;
        ShowcaseViewport.Camera = camera;
        ShowcaseViewport.MouseDown3D += ShowcaseViewport_MouseDown3D;
        ApplyCamera();

        BuildScene();
        TeachingViewComboBox.ItemsSource = presentation.ViewOptions;
        TeachingViewComboBox.SelectedIndex = 0;
        SelectPart(presentation.InitiallySelectedPartId);
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
        AddMotionAxisOverlays();
        TryLoadImportedScene();
    }

    private void AddMotionAxisOverlays()
    {
        foreach (var guide in presentation.MotionAxes)
        {
            var color = guide.Axis switch
            {
                MechanicalMotionAxis.X => new Color4(0.95f, 0.18f, 0.22f, 1f),
                MechanicalMotionAxis.Y => new Color4(0.1f, 0.8f, 0.36f, 1f),
                MechanicalMotionAxis.Z => new Color4(0.16f, 0.48f, 1f, 1f),
                _ => throw new ArgumentOutOfRangeException(nameof(guide))
            };
            var builder = new MeshBuilder();
            builder.AddArrow(guide.Start, guide.End, 0.16f, 3.2f, 24);
            var model = new MeshGeometryModel3D
            {
                Geometry = builder.ToMeshGeometry3D(),
                Material = AxisMaterial(color),
                IsHitTestVisible = false,
                Visibility = Visibility.Collapsed
            };

            motionAxisModels.Add(model, guide.AttachedPartId);
            ShowcaseViewport.Items.Add(model);
        }
    }

    private void BuildProceduralScene()
    {
        var materials = CreateFallbackMaterials();
        foreach (var primitive in presentation.FallbackPrimitives)
        {
            var material = materials[primitive.MaterialRole];
            switch (primitive)
            {
                case MechanicalBoxPrimitive box:
                    AddBox(box.PartId, box.Center, box.Size, material);
                    break;
                case MechanicalCylinderPrimitive cylinder:
                    AddCylinder(
                        cylinder.PartId,
                        cylinder.Start,
                        cylinder.End,
                        cylinder.Radius,
                        material);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported fallback primitive '{primitive.GetType().Name}'.");
            }
        }
    }

    private void TryLoadImportedScene()
    {
        try
        {
            var manifestPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Robots",
                presentation.AssetDirectoryName,
                "robot.json");
            var package = new RobotVisualAssetPackageLoader().Load(manifestPath, showcase.Model);
            importedScene = new HelixRobotVisualAssetImporter().Import(package);
            FitCameraToImportedScene();

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

    private void FitCameraToImportedScene()
    {
        if (importedScene is null)
        {
            return;
        }

        TreeTraverser.ForceUpdateTransformsAndBounds(importedScene.Root);
        var bounds = importedScene.Root.BoundsWithTransform;
        var center = (bounds.Minimum + bounds.Maximum) / 2;
        var radius = (bounds.Maximum - bounds.Minimum).Length() / 2;
        if (!float.IsFinite(radius) || radius <= 0)
        {
            return;
        }

        fittedCameraTarget = new Point3D(center.X, center.Y, center.Z);
        fittedCameraDistance = Math.Clamp(
            OrbitCameraInteractionMath.FitDistance(radius, CameraFieldOfViewDegrees),
            8,
            32);
        cameraTarget = fittedCameraTarget;
        cameraDistance = fittedCameraDistance;
        ApplyCamera();
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

    private void AddBox(RobotPartId partId, Vector3 center, Vector3 size, PhongMaterial material)
    {
        var builder = new MeshBuilder();
        builder.AddBox(center, size.X, size.Y, size.Z);
        AddModel(partId, builder.ToMeshGeometry3D(), material);
    }

    private void AddCylinder(
        RobotPartId partId,
        Vector3 start,
        Vector3 end,
        float radius,
        PhongMaterial material)
    {
        var builder = new MeshBuilder();
        builder.AddCylinder(start, end, radius, 32, true, true);
        AddModel(partId, builder.ToMeshGeometry3D(), material);
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
        ApplyImportedAppearance();
    }

    private void TeachingViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TeachingViewComboBox.SelectedItem is not MechanicalTeachingViewOption option)
        {
            return;
        }

        PauseDemonstration();
        ApplyDemonstrationOptions(option.Mode);
        TeachingViewDescriptionText.Text = option.Description;
        ApplyTeachingView();
        ApplyDemonstrationTime(CurrentDemonstrationTime());
    }

    private void ApplyDemonstrationOptions(MechanicalTeachingViewMode mode)
    {
        var previousId = SelectedDemonstration?.Id;
        var selectedView = presentation.ViewOptions.Single(option => option.Mode == mode);
        var allowedIds = selectedView.DemonstrationIds;
        var demonstrations = showcase.Demonstrations
            .Where(demonstration => allowedIds.Contains(demonstration.Id, StringComparer.Ordinal))
            .ToArray();
        DemonstrationComboBox.ItemsSource = demonstrations;
        DemonstrationComboBox.SelectedItem = demonstrations.FirstOrDefault(
            demonstration => demonstration.Id == previousId) ?? demonstrations[0];
    }

    private void ApplyTeachingView()
    {
        var useImportedScene = importedScene is not null;
        var showMotionAxes = TeachingViewComboBox.SelectedItem is MechanicalTeachingViewOption
        {
            Mode: MechanicalTeachingViewMode.MotionAxes
        };
        if (importedScene is not null)
        {
            importedScene.Root.Visible = true;
        }

        foreach (var model in motionAxisModels.Keys)
        {
            model.Visibility = showMotionAxes ? Visibility.Visible : Visibility.Collapsed;
        }

        foreach (var (partId, models) in modelsByPart)
        {
            foreach (var model in models)
            {
                model.Visibility = useImportedScene ? Visibility.Collapsed : Visibility.Visible;
            }

            ApplyPartAppearance(partId, models);
        }

        ApplyImportedAppearance();
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

        var demonstrationPoses = MechanicalDemonstrationSampler.Sample(demonstration, time);
        var viewMode = TeachingViewComboBox.SelectedItem is MechanicalTeachingViewOption option
            ? option.Mode
            : MechanicalTeachingViewMode.Assembled;
        var poses = MechanicalTeachingPoseComposer.Compose(
            showcase.Model,
            demonstrationPoses,
            viewMode,
            presentation.ExplodedOffsets);
        var transforms = RobotComponentPoseResolver.ResolveWorldTransforms(showcase.Model, poses);
        foreach (var (partId, models) in modelsByPart)
        {
            var transform = ToWpfTransform(transforms[partId]);
            foreach (var model in models)
            {
                model.Transform = transform;
            }
        }

        foreach (var (model, attachedPartId) in motionAxisModels)
        {
            if (attachedPartId is RobotPartId partId)
            {
                model.Transform = ToWpfTransform(transforms[partId]);
            }
        }

        ApplyImportedPoses(poses);

        DemonstrationProgressBar.Value = Math.Clamp(time.TotalSeconds / demonstration.Duration.TotalSeconds, 0, 1);
        DemonstrationTimeText.Text = $"{time.TotalSeconds:0.0} / {demonstration.Duration.TotalSeconds:0.0} s";
    }

    private TimeSpan CurrentDemonstrationTime()
    {
        var time = playbackOffset + (stopwatch.IsRunning ? stopwatch.Elapsed : TimeSpan.Zero);
        return SelectedDemonstration is { } demonstration && time > demonstration.Duration
            ? demonstration.Duration
            : time;
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

    private void ApplyImportedAppearance()
    {
        foreach (var (node, material) in importedMaterials)
        {
            node.Material = material;
            node.IsTransparent = false;
        }

        if (importedScene is null)
        {
            return;
        }

        var isDriveView = TeachingViewComboBox.SelectedItem is MechanicalTeachingViewOption
        {
            Mode: MechanicalTeachingViewMode.DriveSystem
        };
        if (isDriveView)
        {
            foreach (var (partId, nodes) in importedScene.NodesByPart)
            {
                var part = showcase.Model.GetPart(partId);
                var isGhosted = MechanicalTeachingViewCatalog.ShouldGhost(part.Kind);
                foreach (var materialNode in nodes.OfType<MaterialGeometryNode>())
                {
                    materialNode.IsTransparent = isGhosted;
                    materialNode.Material = isGhosted
                        ? importedGhostMaterial
                        : ImportedDriveMaterial(part.Kind, materialNode);
                }
            }
        }

        if (selectedPartId is not RobotPartId selectedId ||
            !importedScene.NodesByPart.TryGetValue(selectedId, out var selectedNodes))
        {
            return;
        }

        var selectedPart = showcase.Model.GetPart(selectedId);
        var selectedIsGhosted = isDriveView && MechanicalTeachingViewCatalog.ShouldGhost(selectedPart.Kind);
        foreach (var materialNode in selectedNodes.OfType<MaterialGeometryNode>())
        {
            materialNode.IsTransparent = selectedIsGhosted;
            materialNode.Material = selectedIsGhosted
                ? importedTransparentSelectionMaterial
                : importedSelectionMaterial;
        }
    }

    private MaterialCore? ImportedDriveMaterial(
        RobotPartKind kind,
        MaterialGeometryNode node) =>
        kind switch
        {
            RobotPartKind.Motor => importedDriveMotorMaterial,
            RobotPartKind.Transmission => importedDriveTransmissionMaterial,
            RobotPartKind.Rail => importedDriveRailMaterial,
            _ => importedMaterials[node]
        };

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

    private static IReadOnlyDictionary<MechanicalMaterialRole, PhongMaterial> CreateFallbackMaterials() =>
        new Dictionary<MechanicalMaterialRole, PhongMaterial>
        {
            [MechanicalMaterialRole.Frame] = Material(
                new Color4(0.28f, 0.32f, 0.38f, 1),
                new Color4(0.85f, 0.9f, 0.98f, 1),
                115),
            [MechanicalMaterialRole.DarkMetal] = Material(
                new Color4(0.08f, 0.1f, 0.14f, 1),
                new Color4(0.5f, 0.56f, 0.65f, 1),
                100),
            [MechanicalMaterialRole.Steel] = Material(
                new Color4(0.48f, 0.54f, 0.62f, 1),
                new Color4(0.95f, 0.98f, 1f, 1),
                125),
            [MechanicalMaterialRole.Accent] = Material(
                new Color4(0.06f, 0.34f, 0.72f, 1),
                new Color4(0.5f, 0.78f, 1f, 1),
                85),
            [MechanicalMaterialRole.Platform] = Material(
                new Color4(0.16f, 0.19f, 0.24f, 1),
                new Color4(0.65f, 0.72f, 0.8f, 1),
                95),
            [MechanicalMaterialRole.Motor] = Material(
                new Color4(0.12f, 0.14f, 0.18f, 1),
                new Color4(0.75f, 0.8f, 0.9f, 1),
                110),
            [MechanicalMaterialRole.Transmission] = Material(
                new Color4(0.035f, 0.04f, 0.05f, 1),
                new Color4(0.2f, 0.22f, 0.25f, 1),
                45),
            [MechanicalMaterialRole.Power] = Material(
                new Color4(0.12f, 0.34f, 0.2f, 1),
                new Color4(0.55f, 0.78f, 0.62f, 1),
                70),
            [MechanicalMaterialRole.Sensor] = Material(
                new Color4(0.04f, 0.48f, 0.7f, 1),
                new Color4(0.52f, 0.88f, 1f, 1),
                95),
            [MechanicalMaterialRole.Tool] = Material(
                new Color4(0.9f, 0.32f, 0.06f, 1),
                new Color4(1f, 0.8f, 0.5f, 1),
                90)
        };

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

    private static PhongMaterial AxisMaterial(Color4 color) =>
        new()
        {
            DiffuseColor = color,
            AmbientColor = new Color4(color.Red * 0.35f, color.Green * 0.35f, color.Blue * 0.35f, 1f),
            EmissiveColor = new Color4(color.Red * 0.2f, color.Green * 0.2f, color.Blue * 0.2f, 1f),
            SpecularColor = new Color4(0.9f, 0.9f, 0.9f, 1f),
            SpecularShininess = 80
        };

    private static PhongMaterialCore CoreMaterial(Color4 diffuse, Color4 specular, float shininess) =>
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

    private void DemonstrationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DemonstrationDescriptionText.Text = SelectedDemonstration?.Description ?? string.Empty;
        ResetDemonstration();
    }

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

    private void ShowcaseViewportHost_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var mode = e.ChangedButton switch
        {
            MouseButton.Middle => ViewportDragMode.Pan,
            MouseButton.Left when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) => ViewportDragMode.Pan,
            MouseButton.Left => ViewportDragMode.Orbit,
            _ => (ViewportDragMode?)null
        };
        if (mode is ViewportDragMode dragMode)
        {
            orbitInteraction.BeginDrag(ShowcaseViewportHost, ShowcaseViewport, e, dragMode);
        }
    }

    private void ShowcaseViewportHost_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton is MouseButton.Left or MouseButton.Middle)
        {
            orbitInteraction.EndDrag(ShowcaseViewportHost, e);
        }
    }

    private void ShowcaseViewportHost_MouseMove(object sender, MouseEventArgs e)
    {
        if (!orbitInteraction.TryGetDragDelta(ShowcaseViewport, e, out var deltaX, out var deltaY))
        {
            return;
        }

        if (orbitInteraction.Mode == ViewportDragMode.Pan)
        {
            cameraTarget = OrbitCameraInteractionMath.PanTarget(
                cameraTarget,
                camera.LookDirection,
                camera.UpDirection,
                cameraDistance,
                camera.FieldOfView,
                ShowcaseViewport.ActualHeight,
                deltaX,
                deltaY);
        }
        else
        {
            cameraAzimuthDegrees = OrbitCameraFactory.NormalizeDegrees(
                cameraAzimuthDegrees - (deltaX * 0.35));
            cameraElevationDegrees = Math.Clamp(
                cameraElevationDegrees + (deltaY * 0.25),
                5,
                85);
        }

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
            cameraTarget,
            cameraAzimuthDegrees,
            cameraElevationDegrees,
            cameraDistance,
            FieldOfView: CameraFieldOfViewDegrees,
            NearPlaneDistance: 0.05,
            FarPlaneDistance: 500));
        camera.Position = reference.Position;
        camera.LookDirection = reference.LookDirection;
        camera.UpDirection = reference.UpDirection;
        camera.FieldOfView = reference.FieldOfView;
        camera.NearPlaneDistance = reference.NearPlaneDistance;
        camera.FarPlaneDistance = reference.FarPlaneDistance;

        var keyLightDirection = reference.LookDirection;
        keyLightDirection.Normalize();
        CameraKeyLight.Direction = keyLightDirection;
    }

    private void ResetCamera()
    {
        cameraAzimuthDegrees = InitialAzimuthDegrees;
        cameraElevationDegrees = InitialElevationDegrees;
        cameraTarget = fittedCameraTarget;
        cameraDistance = fittedCameraDistance;
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
