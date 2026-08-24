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
    private void EnsureCartesianSnapshot()
    {
        if (snapshot is not null)
        {
            return;
        }

        snapshot = CreateSnapshot(ScriptEditorTextBox.Text, captureSession: true);
        InitializeTimelineForSnapshot();
    }

    private void EnsureDifferentialDriveSnapshot()
    {
        if (differentialDriveSnapshot is not null)
        {
            return;
        }

        differentialDriveSnapshot = CreateDifferentialDriveSnapshot(
            DifferentialDriveScriptTextBox.Text,
            captureSession: true);
        DifferentialDriveTimeline.Maximum = differentialDriveSnapshot.FrameCount - 1;
        DifferentialDriveTimeline.TickFrequency = 1;
    }

    private void EnsureScaraSnapshot()
    {
        if (scaraSnapshot is not null)
        {
            return;
        }

        scaraSnapshot = CreateScaraSnapshot(ScaraScriptTextBox.Text, captureSession: true);
        ScaraTimeline.Maximum = scaraSnapshot.FrameCount - 1;
        ScaraTimeline.TickFrequency = 1;
    }

    private void EnsureSimpleArmSnapshot()
    {
        if (simpleArmSnapshot is not null)
        {
            return;
        }

        simpleArmSnapshot = CreateSimpleArmSnapshot(SimpleArmScriptTextBox.Text, captureSession: true);
        SimpleArmTimeline.Maximum = simpleArmSnapshot.FrameCount - 1;
        SimpleArmTimeline.TickFrequency = 1;
    }

    private void EnsureDeltaSnapshot()
    {
        if (deltaSnapshot is not null)
        {
            return;
        }

        deltaSnapshot = CreateDeltaSnapshot(DeltaScriptTextBox.Text, captureSession: true);
        DeltaTimeline.Maximum = deltaSnapshot.FrameCount - 1;
        DeltaTimeline.TickFrequency = 1;
    }

    private void EnsureDroneSnapshot()
    {
        if (droneSnapshot is not null)
        {
            return;
        }

        droneSnapshot = CreateDroneSnapshot(DroneScriptTextBox.Text, captureSession: true);
        DroneTimeline.Maximum = droneSnapshot.FrameCount - 1;
        DroneTimeline.TickFrequency = 1;
    }

    private void EnsureIndustrialArmSnapshot()
    {
        if (industrialArmSnapshot is not null)
        {
            return;
        }

        industrialArmSnapshot = CreateIndustrialArmSnapshot(
            IndustrialArmScriptTextBox.Text,
            captureSession: true);
        IndustrialArmTimeline.Maximum = industrialArmSnapshot.FrameCount - 1;
        IndustrialArmTimeline.TickFrequency = 1;
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
        UpdatePlaybackButtonLabels();
    }

    private void UpdatePlaybackButtonLabels()
    {
        var label = isPlaying ? "Pause" : "Play";
        foreach (var button in playPauseButtons)
        {
            button.Content = label;
        }
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
        var command = GetCartesianMoveCommandText(
            targetPosition,
            speedMillimetersPerSecond,
            ensureAbsoluteMode: true);

        AppendManualCommandAndSimulate(command);
    }

    private string GetCartesianHomeCommandText() =>
        CartesianScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
            ? "G28"
            : "HOME";

    private string GetCartesianMoveCommandText(
        CartesianPosition targetPosition,
        double speedMillimetersPerSecond,
        bool ensureAbsoluteMode = false) =>
        CartesianScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
            ? (ensureAbsoluteMode ? $"G90{Environment.NewLine}" : string.Empty) +
              $"G1 X{FormatNumber(targetPosition.X)} " +
              $"Y{FormatNumber(targetPosition.Y)} " +
              $"Z{FormatNumber(targetPosition.Z)} " +
              $"F{FormatNumber(speedMillimetersPerSecond * 60d)}"
            : $"MOVE X={FormatNumber(targetPosition.X)} " +
              $"Y={FormatNumber(targetPosition.Y)} " +
              $"Z={FormatNumber(targetPosition.Z)} " +
              $"SPEED={FormatNumber(speedMillimetersPerSecond)}";

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
}
