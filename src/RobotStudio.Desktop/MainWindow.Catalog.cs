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
using RobotStudio.Desktop.Showcases;
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

public partial class MainWindow
{
    private MechanicalShowcaseView? activeMechanicalShowcaseView;

    private void UpdateStatePanel(CartesianSceneFrame sceneFrame)
    {
        if (snapshot is null)
        {
            return;
        }

        var pose = snapshot.Poses[currentFrameIndex];
        var frame = snapshot.Frames[currentFrameIndex];
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
        ProfilePhaseValueText.Text = frame.MotionProfilePhase?.ToString() ?? "-";
        VelocityValueText.Text = $"{frame.VelocityMillimetersPerSecond:0.###} mm/s";
        AccelerationValueText.Text = $"{frame.AccelerationMillimetersPerSecondSquared:0.###} mm/s^2";
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

    private CartesianPlaybackSnapshot CreateSnapshot(
        string script,
        bool captureSession = false) =>
        CreateSnapshot(script, profile, captureSession);

    private CartesianPlaybackSnapshot CreateSnapshot(
        string script,
        CartesianRobotProfile robotProfile,
        bool captureSession)
    {
        var commands = CartesianScriptDialect.Parse(
            script,
            new RobotScriptParseContext(initialPosition));
        ValidateCommandSequence(commands, robotProfile);

        var context = SimulationContext.Create(robotProfile, initialPosition);
        var result = new RobotSimulator().Execute(context, commands);
        if (captureSession)
        {
            cartesianSessionContext = result.FinalContext;
            UpdateSessionRecoveryControls();
        }

        return new CartesianPlaybackSnapshotBuilder()
            .Build(robotProfile, result, TimeSpan.FromMilliseconds(100));
    }

    private static CartesianPlaybackSnapshot CreateCartesianProfilePreview(
        CartesianRobotProfile robotProfile)
    {
        var home = new CartesianPosition(0, 0, 0);
        var context = SimulationContext.Create(robotProfile, home);
        var commands = new RobotCommandSequence([new HomeCommand()]);
        var result = new RobotSimulator().Execute(context, commands);

        return new CartesianPlaybackSnapshotBuilder()
            .Build(robotProfile, result, TimeSpan.FromMilliseconds(100));
    }

    private DifferentialDrivePlaybackSnapshot CreateDifferentialDriveSnapshot(
        string script,
        bool captureSession = false)
    {
        var profile = CreateDifferentialDriveProfile();
        var commands = simpleDslDialect.Parse(script);
        ValidateDifferentialDriveCommandSequence(commands, profile);

        var context = DifferentialDriveSimulationContext.Create(
            profile,
            new DifferentialDrivePose(X: 60, Y: 50, HeadingDegrees: 0));
        var result = new DifferentialDriveSimulator().Execute(context, commands);
        if (captureSession)
        {
            differentialDriveSessionContext = result.FinalContext;
            UpdateSessionRecoveryControls();
        }

        return new DifferentialDrivePlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private ScaraPlaybackSnapshot CreateScaraSnapshot(
        string script,
        bool captureSession = false)
    {
        var profile = CreateScaraProfile();
        var initialJoints = new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0);
        var commands = ScaraScriptDialect.Parse(
            script,
            new RobotScriptParseContext(initialJoints));
        ValidateScaraCommandSequence(commands, profile);

        var context = ScaraSimulationContext.Create(
            profile,
            initialJoints);
        var result = new ScaraSimulator().Execute(context, commands);
        if (captureSession)
        {
            scaraSessionContext = result.FinalContext;
            UpdateSessionRecoveryControls();
        }

        return new ScaraPlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private SimpleArmPlaybackSnapshot CreateSimpleArmSnapshot(
        string script,
        bool captureSession = false)
    {
        var profile = CreateSimpleArmProfile();
        var initialJoints = new SimpleArmJointPosition(0, 0, 0);
        var commands = SimpleArmScriptDialect.Parse(
            script,
            new RobotScriptParseContext(initialJoints));
        ValidateSimpleArmCommandSequence(commands, profile);

        var context = SimpleArmSimulationContext.Create(
            profile,
            initialJoints);
        var result = new SimpleArmSimulator().Execute(context, commands);
        if (captureSession)
        {
            simpleArmSessionContext = result.FinalContext;
            UpdateSessionRecoveryControls();
        }

        return new SimpleArmPlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private DeltaPlaybackSnapshot CreateDeltaSnapshot(
        string script,
        bool captureSession = false)
    {
        var profile = CreateDeltaProfile();
        var initialActuators = new DeltaActuatorPosition(0, 0, 0);
        var commands = DeltaScriptDialect.Parse(
            script,
            new RobotScriptParseContext(initialActuators));
        ValidateDeltaCommandSequence(commands, profile);

        var context = DeltaSimulationContext.Create(
            profile,
            initialActuators);
        var result = new DeltaSimulator().Execute(context, commands);
        if (captureSession)
        {
            deltaSessionContext = result.FinalContext;
            UpdateSessionRecoveryControls();
        }

        return new DeltaPlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private DronePlaybackSnapshot CreateDroneSnapshot(
        string script,
        bool captureSession = false)
    {
        var profile = CreateDroneProfile();
        var commands = simpleDslDialect.Parse(script);
        ValidateDroneCommandSequence(commands, profile);

        var context = DroneSimulationContext.Create(
            profile,
            new DronePose(
                XMillimeters: 0,
                YMillimeters: 0,
                ZMillimeters: 0,
                YawDegrees: 0));
        var result = new DroneSimulator().Execute(context, commands);
        if (captureSession)
        {
            droneSessionContext = result.FinalContext;
            UpdateSessionRecoveryControls();
        }

        return new DronePlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private IndustrialArmPlaybackSnapshot CreateIndustrialArmSnapshot(
        string script,
        bool captureSession = false)
    {
        var profile = CreateIndustrialArmProfile();
        var commands = IndustrialArmScriptDialect.Parse(
            script,
            new RobotScriptParseContext(IndustrialArmJointPosition.Home));
        ValidateIndustrialArmCommandSequence(commands, profile);
        var context = IndustrialArmSimulationContext.Create(profile, IndustrialArmJointPosition.Home);
        var result = new IndustrialArmSimulator().Execute(context, commands);
        if (captureSession)
        {
            industrialArmSessionContext = result.FinalContext;
            UpdateSessionRecoveryControls();
        }

        return new IndustrialArmPlaybackSampler()
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
            Focusable = canOpen,
            Cursor = canOpen ? Cursors.Hand : Cursors.Arrow,
            BorderBrush = canOpen
                ? RobotCardAvailableBorderBrush
                : RobotCardPlannedBorderBrush,
            BorderThickness = new Thickness(1),
            Background = RobotCardBackgroundBrush,
            CornerRadius = new CornerRadius(8)
        };

        if (canOpen)
        {
            card.MouseEnter += (_, _) => ApplyRobotCardVisualState(card, template, isHighlighted: true);
            card.MouseLeave += (_, _) => ApplyRobotCardVisualState(
                card,
                template,
                isHighlighted: card.IsKeyboardFocusWithin);
            card.GotKeyboardFocus += (_, _) => ApplyRobotCardVisualState(card, template, isHighlighted: true);
            card.LostKeyboardFocus += (_, _) => ApplyRobotCardVisualState(card, template, isHighlighted: false);
            card.PreviewMouseLeftButtonUp += (_, e) =>
            {
                if (e.OriginalSource is DependencyObject source && IsInsideButton(source))
                {
                    return;
                }

                e.Handled = true;
                OpenRobot(template);
            };
            card.KeyDown += (_, e) =>
            {
                if (e.OriginalSource is Button || e.Key is not (Key.Enter or Key.Space))
                {
                    return;
                }

                e.Handled = true;
                OpenRobot(template);
            };
        }

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

        var metadataTags = new WrapPanel
        {
            Margin = new Thickness(0, 14, 0, 0)
        };
        metadataTags.Children.Add(CreateStatusBadge(template.Status));
        metadataTags.Children.Add(CreateComplexityBadge(template.Complexity));
        topContent.Children.Add(metadataTags);

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

        var footer = CreateRobotCardFooter(template, canOpen);
        Grid.SetRow(footer, 2);
        content.Children.Add(footer);

        return card;
    }

    private FrameworkElement CreateRobotCardFooter(RobotTemplate template, bool canOpen)
    {
        if (canOpen)
        {
            var actions = new Grid
            {
                Height = 36,
                Margin = new Thickness(0, 12, 0, 0)
            };
            actions.ColumnDefinitions.Add(new ColumnDefinition());

            var simulatorButton = new Button
            {
                Content = "Open Simulator",
                Tag = template
            };
            simulatorButton.Click += OpenRobotButton_Click;
            actions.Children.Add(simulatorButton);

            if (RobotCatalog.CanExploreMechanics(template))
            {
                actions.ColumnDefinitions.Add(new ColumnDefinition());
                simulatorButton.Margin = new Thickness(0, 0, 5, 0);

                var showcaseButton = new Button
                {
                    Margin = new Thickness(5, 0, 0, 0),
                    Style = (Style)FindResource("SecondaryButtonStyle"),
                    Content = "Explore Mechanics",
                    Tag = template
                };
                showcaseButton.Click += ExploreMechanicsButton_Click;
                Grid.SetColumn(showcaseButton, 1);
                actions.Children.Add(showcaseButton);
            }

            return actions;
        }

        var status = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Text = "Availability: Planned for a future release"
        };

        return new Border
        {
            Margin = new Thickness(0, 12, 0, 0),
            Padding = new Thickness(0, 12, 0, 0),
            BorderBrush = RobotCardPlannedBorderBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Child = status
        };
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

        RobotCardsPanel.Columns = ResponsiveGridLayout.CalculateColumnCount(
            availableWidth,
            RobotCardPreferredWidth,
            RobotCardGap,
            RobotCardMaximumColumns);
    }

    private static Border CreateStatusBadge(RobotAvailabilityStatus status) =>
        new()
        {
            Margin = new Thickness(0, 0, 8, 0),
            Padding = new Thickness(8, 3, 8, 3),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = GetStatusBackgroundBrush(status),
            BorderBrush = GetStatusBorderBrush(status),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
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
            Margin = new Thickness(0, 0, 8, 0),
            Padding = new Thickness(8, 3, 8, 3),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
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
                CornerRadius = new CornerRadius(4),
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

        if (template.Viewer.Kind == RobotViewerKind.DeltaThreeDimensional)
        {
            DeltaViewerView.Visibility = Visibility.Visible;
            EnsureDeltaSnapshot();
            RenderDeltaFrame(index: 0);
            return;
        }

        if (template.Viewer.Kind == RobotViewerKind.DroneThreeDimensional)
        {
            DroneViewerView.Visibility = Visibility.Visible;
            EnsureDroneSnapshot();
            RenderDroneFrame(index: 0);
            return;
        }

        if (template.Viewer.Kind == RobotViewerKind.IndustrialArmThreeDimensional)
        {
            IndustrialArmViewerView.Visibility = Visibility.Visible;
            EnsureIndustrialArmSnapshot();
            RenderIndustrialArmFrame(index: 0);
            return;
        }

        CartesianViewerView.Visibility = Visibility.Visible;
        EnsureCartesianSnapshot();
        RenderFrame(index: 0);
    }

    private void OpenMechanicalShowcase(RobotTemplate template)
    {
        if (!RobotCatalog.CanExploreMechanics(template))
        {
            return;
        }

        var descriptor = template.MechanicalShowcase!;
        var presentation = MechanicalShowcaseCatalog.Create(descriptor.ModelId);
        CloseMechanicalShowcase();

        var view = new MechanicalShowcaseView(presentation);
        view.BackRequested += MechanicalShowcase_BackRequested;
        activeMechanicalShowcaseView = view;
        MechanicalShowcaseHost.Content = view;

        StopPlayback();
        RobotSelectionView.Visibility = Visibility.Collapsed;
        MechanicalShowcaseHost.Visibility = Visibility.Visible;
    }

    private void CloseMechanicalShowcase()
    {
        if (activeMechanicalShowcaseView is not null)
        {
            activeMechanicalShowcaseView.BackRequested -= MechanicalShowcase_BackRequested;
            activeMechanicalShowcaseView = null;
        }

        MechanicalShowcaseHost.Content = null;
        MechanicalShowcaseHost.Visibility = Visibility.Collapsed;
    }

    private static bool IsInsideButton(DependencyObject source)
    {
        for (var current = source; current is not null; current = GetElementParent(current))
        {
            if (current is Button)
            {
                return true;
            }
        }

        return false;
    }

    private static DependencyObject? GetElementParent(DependencyObject element) =>
        element is Visual or Visual3D
            ? VisualTreeHelper.GetParent(element)
            : LogicalTreeHelper.GetParent(element);

    private void ConfigureActiveViewer(RobotViewerKind viewerKind)
    {
        activeViewerKind = viewerKind;
        snapshot = null;
        differentialDriveSnapshot = null;
        scaraSnapshot = null;
        simpleArmSnapshot = null;
        deltaSnapshot = null;
        droneSnapshot = null;
        industrialArmSnapshot = null;
        cartesianSessionContext = null;
        differentialDriveSessionContext = null;
        scaraSessionContext = null;
        simpleArmSessionContext = null;
        deltaSessionContext = null;
        droneSessionContext = null;
        industrialArmSessionContext = null;
        currentFrameIndex = 0;
        differentialDriveFrameIndex = 0;
        scaraFrameIndex = 0;
        simpleArmFrameIndex = 0;
        deltaFrameIndex = 0;
        droneFrameIndex = 0;
        industrialArmFrameIndex = 0;
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

            case RobotViewerKind.DeltaThreeDimensional:
                ConfigureDeltaViewer();
                break;

            case RobotViewerKind.DroneThreeDimensional:
                ConfigureDroneViewer();
                break;

            case RobotViewerKind.IndustrialArmThreeDimensional:
                ConfigureIndustrialArmViewer();
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
        RefreshGCodeExplanations();
        UpdateSessionRecoveryControls();
    }

    private void RefreshGCodeExplanations()
    {
        if (CartesianGCodeExplanationPanel is null ||
            ScaraGCodeExplanationPanel is null ||
            SimpleArmGCodeExplanationPanel is null ||
            DeltaGCodeExplanationPanel is null ||
            IndustrialArmGCodeExplanationPanel is null)
        {
            return;
        }

        var cartesianTarget = activeViewerKind == RobotViewerKind.XYPlotterTwoDimensional
            ? GCodeRobotTarget.XYPlotter
            : GCodeRobotTarget.CartesianRobot;
        CartesianGCodeExplanationPanel.SetContext(
            CartesianScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode,
            ScriptEditorTextBox.Text,
            cartesianTarget);
        ScaraGCodeExplanationPanel.SetContext(
            ScaraScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode,
            ScaraScriptTextBox.Text,
            GCodeRobotTarget.ScaraRobot);
        SimpleArmGCodeExplanationPanel.SetContext(
            SimpleArmScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode,
            SimpleArmScriptTextBox.Text,
            GCodeRobotTarget.SimpleArticulatedArm);
        DeltaGCodeExplanationPanel.SetContext(
            DeltaScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode,
            DeltaScriptTextBox.Text,
            GCodeRobotTarget.DeltaRobot);
        IndustrialArmGCodeExplanationPanel.SetContext(
            IndustrialArmScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode,
            IndustrialArmScriptTextBox.Text,
            GCodeRobotTarget.IndustrialArm6Dof);
    }

    private void ConfigureCartesianViewer()
    {
        profile = CreateCartesianProfile();
        xyPlotterProfile = null;
        initialPosition = new CartesianPosition(X: 40, Y: 30, Z: 20);
        ViewerSubtitleText.Text = "Cartesian robot simulation";
        ConfigureExampleSelector(
            CartesianExampleComboBox,
            RobotViewerKind.CartesianThreeDimensional);
        ScriptEditorTextBox.Text = GetCartesianExampleScript(RobotViewerKind.CartesianThreeDimensional);
        CommandConsoleTextBox.Text = GetCartesianMoveCommandText(new CartesianPosition(100, 50, 20), 80);
        JogNegativeZButton.IsEnabled = true;
        JogPositiveZButton.IsEnabled = true;
        ManualControlStatusText.Text = "Manual actions append commands in the selected dialect and resimulate the robot.";
        CartesianProfileExpander.Visibility = Visibility.Visible;
        PopulateCartesianProfileEditor(profile);
    }

    private void ConfigureXYPlotterViewer()
    {
        xyPlotterProfile = CreateXYPlotterProfile();
        profile = xyPlotterProfile.ToCartesianProfile();
        initialPosition = new CartesianPosition(X: 40, Y: 30, Z: 0);
        ViewerSubtitleText.Text = "XY plotter simulation";
        ConfigureExampleSelector(
            CartesianExampleComboBox,
            RobotViewerKind.XYPlotterTwoDimensional);
        ScriptEditorTextBox.Text = GetCartesianExampleScript(RobotViewerKind.XYPlotterTwoDimensional);
        CommandConsoleTextBox.Text = GetCartesianMoveCommandText(new CartesianPosition(100, 50, 0), 80);
        JogNegativeZButton.IsEnabled = false;
        JogPositiveZButton.IsEnabled = false;
        ManualControlStatusText.Text = "XY Plotter uses X/Y jog commands. Z remains fixed at 0 mm.";
        CartesianProfileExpander.Visibility = Visibility.Collapsed;
    }

    private void ConfigureDifferentialDriveViewer()
    {
        ConfigureExampleSelector(
            DifferentialDriveExampleComboBox,
            RobotViewerKind.DifferentialDriveTwoDimensional);
        DifferentialDriveScriptTextBox.Text = GetDefaultExampleScript(RobotViewerKind.DifferentialDriveTwoDimensional);
        SetDifferentialDriveScriptStatus(
            "Edit DRIVE commands and simulate the mobile robot.",
            Color.FromRgb(148, 163, 184));
    }

    private void ConfigureScaraViewer()
    {
        ConfigureExampleSelector(
            ScaraExampleComboBox,
            RobotViewerKind.ScaraThreeDimensional);
        ScaraScriptTextBox.Text = GetScaraExampleScript(
            RobotExampleCatalog.GetDefaultFor(RobotViewerKind.ScaraThreeDimensional));
        SetScaraScriptStatus(
            "Choose joint-space Simple DSL or tool-space G-code and simulate the articulated robot.",
            Color.FromRgb(148, 163, 184));
    }

    private void ConfigureSimpleArmViewer()
    {
        ConfigureExampleSelector(
        SimpleArmExampleComboBox,
            RobotViewerKind.SimpleArmThreeDimensional);
        SimpleArmScriptTextBox.Text = GetSimpleArmExampleScript(
            RobotExampleCatalog.GetDefaultFor(RobotViewerKind.SimpleArmThreeDimensional));
        SetSimpleArmScriptStatus(
            "Choose joint-space Simple DSL or tool-pose G-code and simulate the articulated arm.",
            Color.FromRgb(148, 163, 184));
    }

    private void ConfigureDeltaViewer()
    {
        ConfigureExampleSelector(
            DeltaExampleComboBox,
            RobotViewerKind.DeltaThreeDimensional);
        DeltaScriptTextBox.Text = GetDeltaExampleScript(
            RobotExampleCatalog.GetDefaultFor(RobotViewerKind.DeltaThreeDimensional));
        SetDeltaScriptStatus(
            "Choose actuator-space Simple DSL or tool-space G-code and simulate the parallel robot.",
            Color.FromRgb(148, 163, 184));
    }

    private void ConfigureDroneViewer()
    {
        ConfigureExampleSelector(
            DroneExampleComboBox,
            RobotViewerKind.DroneThreeDimensional);
        DroneScriptTextBox.Text = GetDefaultExampleScript(RobotViewerKind.DroneThreeDimensional);
        SetDroneScriptStatus(
            "Edit DRONE pose commands and simulate the aerial robot.",
            Color.FromRgb(148, 163, 184));
    }

    private void ConfigureIndustrialArmViewer()
    {
        ConfigureExampleSelector(
            IndustrialArmExampleComboBox,
            RobotViewerKind.IndustrialArmThreeDimensional);
        IndustrialArmScriptTextBox.Text = GetIndustrialArmExampleScript(
            RobotExampleCatalog.GetDefaultFor(RobotViewerKind.IndustrialArmThreeDimensional));
        SetIndustrialArmScriptStatus(
            "Choose joint-space Simple DSL or tool-pose G-code and simulate the industrial arm.",
            Color.FromRgb(148, 163, 184));
    }

    private static string GetDefaultExampleScript(RobotViewerKind viewerKind) =>
        RobotExampleCatalog.GetDefaultFor(viewerKind).Script;

    private string GetCartesianExampleScript(RobotViewerKind viewerKind)
    {
        var example = RobotExampleCatalog.GetDefaultFor(viewerKind);
        return ConvertCartesianExampleForSelectedDialect(example);
    }

    private string GetSelectedCartesianExampleScript()
    {
        var example = CartesianExampleComboBox.SelectedItem as RobotExample ??
            RobotExampleCatalog.GetDefaultFor(activeViewerKind);
        return ConvertCartesianExampleForSelectedDialect(example);
    }

    private string ConvertCartesianExampleForSelectedDialect(RobotExample example) =>
        CartesianScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
            ? example.GCodeScript ?? GCodeWriter.Write(simpleDslDialect.Parse(example.Script))
            : example.Script;

    private string GetSelectedScaraExampleScript()
    {
        var example = ScaraExampleComboBox.SelectedItem as RobotExample ??
            RobotExampleCatalog.GetDefaultFor(RobotViewerKind.ScaraThreeDimensional);
        return GetScaraExampleScript(example);
    }

    private string GetScaraExampleScript(RobotExample example) =>
        ScaraScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
            ? example.GCodeScript ?? throw new InvalidOperationException(
                $"SCARA example '{example.Name}' does not define a G-code program.")
            : example.Script;

    private string GetSelectedSimpleArmExampleScript()
    {
        var example = SimpleArmExampleComboBox.SelectedItem as RobotExample ??
            RobotExampleCatalog.GetDefaultFor(RobotViewerKind.SimpleArmThreeDimensional);
        return GetSimpleArmExampleScript(example);
    }

    private string GetSimpleArmExampleScript(RobotExample example) =>
        SimpleArmScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
            ? example.GCodeScript ?? throw new InvalidOperationException(
                $"Simple Arm example '{example.Name}' does not define a G-code program.")
            : example.Script;

    private string GetSelectedDeltaExampleScript()
    {
        var example = DeltaExampleComboBox.SelectedItem as RobotExample ??
            RobotExampleCatalog.GetDefaultFor(RobotViewerKind.DeltaThreeDimensional);
        return GetDeltaExampleScript(example);
    }

    private string GetDeltaExampleScript(RobotExample example) =>
        DeltaScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
            ? example.GCodeScript ?? throw new InvalidOperationException(
                $"Delta example '{example.Name}' does not define a G-code program.")
            : example.Script;

    private string GetSelectedIndustrialArmExampleScript()
    {
        var example = IndustrialArmExampleComboBox.SelectedItem as RobotExample ??
            RobotExampleCatalog.GetDefaultFor(RobotViewerKind.IndustrialArmThreeDimensional);
        return GetIndustrialArmExampleScript(example);
    }

    private string GetIndustrialArmExampleScript(RobotExample example) =>
        IndustrialArmScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
            ? example.GCodeScript ?? throw new InvalidOperationException(
                $"Industrial Arm example '{example.Name}' does not define a G-code program.")
            : example.Script;

    private static void ConfigureExampleSelector(
        ComboBox comboBox,
        RobotViewerKind viewerKind)
    {
        comboBox.ItemsSource = RobotExampleCatalog.GetFor(viewerKind);
        comboBox.SelectedIndex = 0;
    }

    private static void UpdateSelectedExampleDescription(
        ComboBox comboBox,
        TextBlock descriptionTextBlock)
    {
        descriptionTextBlock.Text = comboBox.SelectedItem is RobotExample example
            ? example.Description
            : "Select an example to load a starter script.";
    }

    private static string GetSelectedExampleScript(
        ComboBox comboBox,
        RobotViewerKind fallbackViewerKind) =>
        comboBox.SelectedItem is RobotExample example
            ? example.Script
            : GetDefaultExampleScript(fallbackViewerKind);

    private static bool IsTextInputFocused() =>
        Keyboard.FocusedElement is TextBox or ComboBox;

    private static bool IsZoomInKey(Key key) =>
        key is Key.OemPlus or Key.Add;

    private static bool IsZoomOutKey(Key key) =>
        key is Key.OemMinus or Key.Subtract;

    private static bool IsZeroKey(Key key) =>
        key is Key.D0 or Key.NumPad0;

    private bool IsPointerOverActiveViewer() =>
        activeViewerKind switch
        {
            RobotViewerKind.DifferentialDriveTwoDimensional => DifferentialDriveCanvas.IsMouseOver,
            RobotViewerKind.ScaraThreeDimensional => ScaraViewportHost.IsMouseOver,
            RobotViewerKind.SimpleArmThreeDimensional => SimpleArmViewportHost.IsMouseOver,
            RobotViewerKind.DeltaThreeDimensional => DeltaViewportHost.IsMouseOver,
            RobotViewerKind.DroneThreeDimensional => DroneViewportHost.IsMouseOver,
            RobotViewerKind.IndustrialArmThreeDimensional => IndustrialArmViewportHost.IsMouseOver,
            RobotViewerKind.CartesianThreeDimensional or RobotViewerKind.XYPlotterTwoDimensional => RobotViewportHost.IsMouseOver,
            _ => false
        };
}
