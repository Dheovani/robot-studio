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
    private void SimulateDifferentialDriveScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryCreateDifferentialDriveSnapshotFromScript(
            DifferentialDriveScriptTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
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
        DifferentialDriveTimeline.Maximum = differentialDriveSnapshot.FrameCount - 1;
        DifferentialDriveTimeline.TickFrequency = 1;
        RenderDifferentialDriveFrame(index: 0);
        SetDifferentialDriveScriptStatus(message, Color.FromRgb(74, 222, 128));
    }

    private void SimulateScaraScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryCreateScaraSnapshotFromScript(
            ScaraScriptTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
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
        ScaraTimeline.Maximum = scaraSnapshot.FrameCount - 1;
        ScaraTimeline.TickFrequency = 1;
        RenderScaraFrame(index: 0);
        SetScaraScriptStatus(message, Color.FromRgb(74, 222, 128));
    }

    private void LoadScaraExampleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        ScaraScriptTextBox.Text = GetSelectedScaraExampleScript();
        scaraSnapshot = CreateScaraSnapshot(ScaraScriptTextBox.Text, captureSession: true);
        ScaraTimeline.Maximum = scaraSnapshot.FrameCount - 1;
        ScaraTimeline.TickFrequency = 1;
        RenderScaraFrame(index: 0);
        SetScaraScriptStatus("Loaded the selected SCARA example.", Color.FromRgb(74, 222, 128));
    }

    private void LoadDifferentialDriveExampleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        DifferentialDriveScriptTextBox.Text = GetSelectedExampleScript(
            DifferentialDriveExampleComboBox,
            RobotViewerKind.DifferentialDriveTwoDimensional);
        differentialDriveSnapshot = CreateDifferentialDriveSnapshot(
            DifferentialDriveScriptTextBox.Text,
            captureSession: true);
        DifferentialDriveTimeline.Maximum = differentialDriveSnapshot.FrameCount - 1;
        DifferentialDriveTimeline.TickFrequency = 1;
        RenderDifferentialDriveFrame(index: 0);
        SetDifferentialDriveScriptStatus("Loaded the selected mobile robot example.", Color.FromRgb(74, 222, 128));
    }

    private void SimulateSimpleArmScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryCreateSimpleArmSnapshotFromScript(
            SimpleArmScriptTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
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
        SimpleArmTimeline.Maximum = simpleArmSnapshot.FrameCount - 1;
        SimpleArmTimeline.TickFrequency = 1;
        RenderSimpleArmFrame(index: 0);
        SetSimpleArmScriptStatus(message, Color.FromRgb(74, 222, 128));
    }

    private void LoadSimpleArmExampleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        SimpleArmScriptTextBox.Text = GetSelectedSimpleArmExampleScript();
        simpleArmSnapshot = CreateSimpleArmSnapshot(SimpleArmScriptTextBox.Text, captureSession: true);
        SimpleArmTimeline.Maximum = simpleArmSnapshot.FrameCount - 1;
        SimpleArmTimeline.TickFrequency = 1;
        RenderSimpleArmFrame(index: 0);
        SetSimpleArmScriptStatus("Loaded the selected articulated arm example.", Color.FromRgb(74, 222, 128));
    }

    private void SimulateDeltaScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryCreateDeltaSnapshotFromScript(
            DeltaScriptTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
        {
            SetDeltaScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        if (nextSnapshot is null)
        {
            SetDeltaScriptStatus("Script did not produce a Delta playback snapshot.", Color.FromRgb(248, 113, 113));
            return;
        }

        StopPlayback();
        deltaSnapshot = nextSnapshot;
        DeltaTimeline.Maximum = deltaSnapshot.FrameCount - 1;
        DeltaTimeline.TickFrequency = 1;
        RenderDeltaFrame(index: 0);
        SetDeltaScriptStatus(message, Color.FromRgb(74, 222, 128));
    }

    private void LoadDeltaExampleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        DeltaScriptTextBox.Text = GetSelectedDeltaExampleScript();
        deltaSnapshot = CreateDeltaSnapshot(DeltaScriptTextBox.Text, captureSession: true);
        DeltaTimeline.Maximum = deltaSnapshot.FrameCount - 1;
        DeltaTimeline.TickFrequency = 1;
        RenderDeltaFrame(index: 0);
        SetDeltaScriptStatus("Loaded the selected Delta example.", Color.FromRgb(74, 222, 128));
    }

    private void SimulateDroneScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryCreateDroneSnapshotFromScript(
            DroneScriptTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
        {
            SetDroneScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        if (nextSnapshot is null)
        {
            SetDroneScriptStatus("Script did not produce a Drone playback snapshot.", Color.FromRgb(248, 113, 113));
            return;
        }

        StopPlayback();
        droneSnapshot = nextSnapshot;
        DroneTimeline.Maximum = droneSnapshot.FrameCount - 1;
        DroneTimeline.TickFrequency = 1;
        RenderDroneFrame(index: 0);
        SetDroneScriptStatus(message, Color.FromRgb(74, 222, 128));
    }

    private void LoadDroneExampleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        DroneScriptTextBox.Text = GetSelectedExampleScript(
            DroneExampleComboBox,
            RobotViewerKind.DroneThreeDimensional);
        droneSnapshot = CreateDroneSnapshot(DroneScriptTextBox.Text, captureSession: true);
        DroneTimeline.Maximum = droneSnapshot.FrameCount - 1;
        DroneTimeline.TickFrequency = 1;
        RenderDroneFrame(index: 0);
        SetDroneScriptStatus("Loaded the selected Drone example.", Color.FromRgb(74, 222, 128));
    }

    private void SimulateIndustrialArmScriptButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryCreateIndustrialArmSnapshotFromScript(
                IndustrialArmScriptTextBox.Text,
                out var nextSnapshot,
                out var message,
                captureSession: true) ||
            nextSnapshot is null)
        {
            SetIndustrialArmScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        StopPlayback();
        industrialArmSnapshot = nextSnapshot;
        IndustrialArmTimeline.Maximum = industrialArmSnapshot.FrameCount - 1;
        IndustrialArmTimeline.TickFrequency = 1;
        RenderIndustrialArmFrame(index: 0);
        SetIndustrialArmScriptStatus(message, Color.FromRgb(74, 222, 128));
    }

    private void LoadIndustrialArmExampleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        IndustrialArmScriptTextBox.Text = GetSelectedIndustrialArmExampleScript();
        industrialArmSnapshot = CreateIndustrialArmSnapshot(
            IndustrialArmScriptTextBox.Text,
            captureSession: true);
        IndustrialArmTimeline.Maximum = industrialArmSnapshot.FrameCount - 1;
        IndustrialArmTimeline.TickFrequency = 1;
        RenderIndustrialArmFrame(index: 0);
        SetIndustrialArmScriptStatus("Loaded the selected industrial arm example.", Color.FromRgb(74, 222, 128));
    }

    private void IndustrialArmScriptDialectComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded ||
            activeViewerKind != RobotViewerKind.IndustrialArmThreeDimensional ||
            IndustrialArmScriptDialectComboBox.SelectedItem is not RobotScriptDialectDescriptor descriptor)
        {
            return;
        }

        StopPlayback();
        IndustrialArmScriptTextBox.Text = GetSelectedIndustrialArmExampleScript();
        if (!TryCreateIndustrialArmSnapshotFromScript(
            IndustrialArmScriptTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
        {
            industrialArmSnapshot = null;
            industrialArmSessionContext = null;
            SetIndustrialArmScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        industrialArmSnapshot = nextSnapshot;
        IndustrialArmTimeline.Maximum = industrialArmSnapshot!.FrameCount - 1;
        IndustrialArmTimeline.TickFrequency = 1;
        RenderIndustrialArmFrame(index: 0);
        var dialectMessage = descriptor.Id == RobotScriptDialectId.GCode
            ? "G-code tool-pose mode ready. G1 follows X/Y/Z/A/B/C with deterministic inverse kinematics."
            : "Simple DSL joint-space mode ready.";
        SetIndustrialArmScriptStatus($"{dialectMessage} {message}", Color.FromRgb(74, 222, 128));
    }

    private void LoadCartesianExampleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StopPlayback();
        ScriptEditorTextBox.Text = GetSelectedCartesianExampleScript();
        snapshot = CreateSnapshot(ScriptEditorTextBox.Text, captureSession: true);
        TimelineSlider.Maximum = snapshot.SceneFrameCount - 1;
        TimelineSlider.TickFrequency = 1;
        RenderFrame(index: 0);
        SetScriptStatus("Loaded the selected teaching example.", Color.FromRgb(74, 222, 128));
    }

    private void CartesianExampleComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e) =>
        UpdateSelectedExampleDescription(
            CartesianExampleComboBox,
            CartesianExampleDescriptionText);

    private void ScriptDialectComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded || ScriptDialectComboBox.SelectedItem is not RobotScriptDialectDescriptor descriptor)
        {
            return;
        }

        StopPlayback();
        ScriptEditorTextBox.Text = GetSelectedCartesianExampleScript();
        CommandConsoleTextBox.Text = descriptor.Id == RobotScriptDialectId.GCode
            ? "G1 X100 Y50 Z20 F4800"
            : "MOVE X=100 Y=50 Z=20 SPEED=80";
        RefreshScriptEditorGutter();

        if (!TryCreateSnapshotFromScript(
            ScriptEditorTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
        {
            snapshot = null;
            cartesianSessionContext = null;
            SetScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        snapshot = nextSnapshot;
        InitializeTimelineForSnapshot();
        RenderFrame(index: 0);
        var dialectMessage = descriptor.Id == RobotScriptDialectId.GCode
            ? "G-code mode ready. F is measured in mm/min."
            : "Simple DSL mode ready.";
        SetScriptStatus($"{dialectMessage} {message}", Color.FromRgb(74, 222, 128));
    }

    private void ScaraScriptDialectComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded ||
            activeViewerKind != RobotViewerKind.ScaraThreeDimensional ||
            ScaraScriptDialectComboBox.SelectedItem is not RobotScriptDialectDescriptor descriptor)
        {
            return;
        }

        StopPlayback();
        ScaraScriptTextBox.Text = GetSelectedScaraExampleScript();

        if (!TryCreateScaraSnapshotFromScript(
            ScaraScriptTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
        {
            scaraSnapshot = null;
            scaraSessionContext = null;
            SetScaraScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        if (nextSnapshot is null)
        {
            SetScaraScriptStatus(
                "Script did not produce a SCARA playback snapshot.",
                Color.FromRgb(248, 113, 113));
            return;
        }

        scaraSnapshot = nextSnapshot;
        ScaraTimeline.Maximum = scaraSnapshot.FrameCount - 1;
        ScaraTimeline.TickFrequency = 1;
        RenderScaraFrame(index: 0);
        var dialectMessage = descriptor.Id == RobotScriptDialectId.GCode
            ? "G-code tool-space mode ready. G1 follows X/Y with elbow-down IK."
            : "Simple DSL joint-space mode ready.";
        SetScaraScriptStatus($"{dialectMessage} {message}", Color.FromRgb(74, 222, 128));
    }

    private void SimpleArmScriptDialectComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded ||
            activeViewerKind != RobotViewerKind.SimpleArmThreeDimensional ||
            SimpleArmScriptDialectComboBox.SelectedItem is not RobotScriptDialectDescriptor descriptor)
        {
            return;
        }

        StopPlayback();
        SimpleArmScriptTextBox.Text = GetSelectedSimpleArmExampleScript();

        if (!TryCreateSimpleArmSnapshotFromScript(
            SimpleArmScriptTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
        {
            simpleArmSnapshot = null;
            simpleArmSessionContext = null;
            SetSimpleArmScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        if (nextSnapshot is null)
        {
            SetSimpleArmScriptStatus(
                "Script did not produce a Simple Arm playback snapshot.",
                Color.FromRgb(248, 113, 113));
            return;
        }

        simpleArmSnapshot = nextSnapshot;
        SimpleArmTimeline.Maximum = simpleArmSnapshot.FrameCount - 1;
        SimpleArmTimeline.TickFrequency = 1;
        RenderSimpleArmFrame(index: 0);
        var dialectMessage = descriptor.Id == RobotScriptDialectId.GCode
            ? "G-code tool-pose mode ready. G1 follows X/Y/A with positive-bend IK."
            : "Simple DSL joint-space mode ready.";
        SetSimpleArmScriptStatus($"{dialectMessage} {message}", Color.FromRgb(74, 222, 128));
    }

    private void DeltaScriptDialectComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded ||
            activeViewerKind != RobotViewerKind.DeltaThreeDimensional ||
            DeltaScriptDialectComboBox.SelectedItem is not RobotScriptDialectDescriptor descriptor)
        {
            return;
        }

        StopPlayback();
        DeltaScriptTextBox.Text = GetSelectedDeltaExampleScript();

        if (!TryCreateDeltaSnapshotFromScript(
            DeltaScriptTextBox.Text,
            out var nextSnapshot,
            out var message,
            captureSession: true))
        {
            deltaSnapshot = null;
            deltaSessionContext = null;
            SetDeltaScriptStatus(message, Color.FromRgb(248, 113, 113));
            return;
        }

        if (nextSnapshot is null)
        {
            SetDeltaScriptStatus(
                "Script did not produce a Delta playback snapshot.",
                Color.FromRgb(248, 113, 113));
            return;
        }

        deltaSnapshot = nextSnapshot;
        DeltaTimeline.Maximum = deltaSnapshot.FrameCount - 1;
        DeltaTimeline.TickFrequency = 1;
        RenderDeltaFrame(index: 0);
        var dialectMessage = descriptor.Id == RobotScriptDialectId.GCode
            ? "G-code tool-space mode ready. G1 follows X/Y/Z through parallel inverse kinematics."
            : "Simple DSL actuator-space mode ready.";
        SetDeltaScriptStatus($"{dialectMessage} {message}", Color.FromRgb(74, 222, 128));
    }
}
