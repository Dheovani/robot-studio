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
            collisionRadiusMillimeters: 70,
            maximumLinearVelocityMillimetersPerSecond: 250,
            maximumAngularVelocityDegreesPerSecond: 180,
            maximumLinearAccelerationMillimetersPerSecondSquared: 500,
            maximumAngularAccelerationDegreesPerSecondSquared: 360);

    private static ScaraRobotProfile CreateScaraProfile() =>
        new(
            firstLinkLengthMillimeters: 180,
            secondLinkLengthMillimeters: 120,
            linkCollisionRadiusMillimeters: 12,
            shoulderJoint: new ScaraJoint(
                ScaraJointId.Shoulder,
                minimumDegrees: -180,
                maximumDegrees: 180,
                maximumVelocityDegreesPerSecond: 120,
                maximumAccelerationDegreesPerSecondSquared: 240),
            elbowJoint: new ScaraJoint(
                ScaraJointId.Elbow,
                minimumDegrees: -150,
                maximumDegrees: 150,
                maximumVelocityDegreesPerSecond: 100,
                maximumAccelerationDegreesPerSecondSquared: 200));

    private static SimpleArmRobotProfile CreateSimpleArmProfile() =>
        new(
            firstLinkLengthMillimeters: 120,
            secondLinkLengthMillimeters: 90,
            thirdLinkLengthMillimeters: 60,
            linkCollisionRadiusMillimeters: 10,
            baseJoint: new SimpleArmJoint(
                SimpleArmJointId.Base,
                minimumDegrees: -180,
                maximumDegrees: 180,
                maximumVelocityDegreesPerSecond: 100,
                maximumAccelerationDegreesPerSecondSquared: 200),
            shoulderJoint: new SimpleArmJoint(
                SimpleArmJointId.Shoulder,
                minimumDegrees: -120,
                maximumDegrees: 120,
                maximumVelocityDegreesPerSecond: 90,
                maximumAccelerationDegreesPerSecondSquared: 180),
            elbowJoint: new SimpleArmJoint(
                SimpleArmJointId.Elbow,
                minimumDegrees: -150,
                maximumDegrees: 150,
                maximumVelocityDegreesPerSecond: 80,
                maximumAccelerationDegreesPerSecondSquared: 160));

    private static DeltaRobotProfile CreateDeltaProfile() =>
        DeltaTeachingProfile.Create();

    private static DroneProfile CreateDroneProfile() =>
        new(
            minimumXMillimeters: 0,
            maximumXMillimeters: 500,
            minimumYMillimeters: 0,
            maximumYMillimeters: 350,
            minimumZMillimeters: 0,
            maximumZMillimeters: 240,
            collisionRadiusMillimeters: 24,
            maximumLinearVelocityMillimetersPerSecond: 180,
            maximumYawVelocityDegreesPerSecond: 120,
            maximumLinearAccelerationMillimetersPerSecondSquared: 360,
            maximumYawAccelerationDegreesPerSecondSquared: 240,
            maximumTiltDegrees: 45,
            maximumAttitudeVelocityDegreesPerSecond: 180,
            maximumAttitudeAccelerationDegreesPerSecondSquared: 360);

    private static IndustrialArmRobotProfile CreateIndustrialArmProfile() =>
        IndustrialArmTeachingProfile.Create();

    private void ValidateCommandSequence(
        RobotCommandSequence commands,
        CartesianRobotProfile? robotProfile = null)
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
            RobotCommandValidator.Validate(command, robotProfile ?? profile);
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

    private static void ValidateDeltaCommandSequence(
        RobotCommandSequence commands,
        DeltaRobotProfile profile)
    {
        foreach (var command in commands.Commands)
        {
            RobotCommandValidator.Validate(command, profile);
        }
    }

    private static void ValidateDroneCommandSequence(
        RobotCommandSequence commands,
        DroneProfile profile)
    {
        foreach (var command in commands.Commands)
        {
            RobotCommandValidator.Validate(command, profile);
        }
    }

    private static void ValidateIndustrialArmCommandSequence(
        RobotCommandSequence commands,
        IndustrialArmRobotProfile profile)
    {
        foreach (var command in commands.Commands)
        {
            RobotCommandValidator.Validate(command, profile);
        }
    }

    private bool TryCreateSnapshotFromScript(
        string script,
        out CartesianPlaybackSnapshot? nextSnapshot,
        out string message,
        bool captureSession = false)
    {
        try
        {
            nextSnapshot = CreateSnapshot(script, captureSession);
            message = FormatValidScriptStatus(nextSnapshot.SceneFrameCount);
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = ScriptValidationMessageFormatter.Format(exception);
            return false;
        }
    }

    private bool TryCreateDifferentialDriveSnapshotFromScript(
        string script,
        out DifferentialDrivePlaybackSnapshot? nextSnapshot,
        out string message,
        bool captureSession = false)
    {
        try
        {
            nextSnapshot = CreateDifferentialDriveSnapshot(script, captureSession);
            message = FormatValidScriptStatus(nextSnapshot.FrameCount);
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = ScriptValidationMessageFormatter.Format(exception);
            return false;
        }
    }

    private bool TryCreateScaraSnapshotFromScript(
        string script,
        out ScaraPlaybackSnapshot? nextSnapshot,
        out string message,
        bool captureSession = false)
    {
        try
        {
            nextSnapshot = CreateScaraSnapshot(script, captureSession);
            message = FormatValidScriptStatus(nextSnapshot.FrameCount);
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = ScriptValidationMessageFormatter.Format(exception);
            return false;
        }
    }

    private bool TryCreateSimpleArmSnapshotFromScript(
        string script,
        out SimpleArmPlaybackSnapshot? nextSnapshot,
        out string message,
        bool captureSession = false)
    {
        try
        {
            nextSnapshot = CreateSimpleArmSnapshot(script, captureSession);
            message = FormatValidScriptStatus(nextSnapshot.FrameCount);
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = ScriptValidationMessageFormatter.Format(exception);
            return false;
        }
    }

    private bool TryCreateDeltaSnapshotFromScript(
        string script,
        out DeltaPlaybackSnapshot? nextSnapshot,
        out string message,
        bool captureSession = false)
    {
        try
        {
            nextSnapshot = CreateDeltaSnapshot(script, captureSession);
            message = FormatValidScriptStatus(nextSnapshot.FrameCount);
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = ScriptValidationMessageFormatter.Format(exception);
            return false;
        }
    }

    private bool TryCreateDroneSnapshotFromScript(
        string script,
        out DronePlaybackSnapshot? nextSnapshot,
        out string message,
        bool captureSession = false)
    {
        try
        {
            nextSnapshot = CreateDroneSnapshot(script, captureSession);
            message = FormatValidScriptStatus(nextSnapshot.FrameCount);
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = ScriptValidationMessageFormatter.Format(exception);
            return false;
        }
    }

    private bool TryCreateIndustrialArmSnapshotFromScript(
        string script,
        out IndustrialArmPlaybackSnapshot? nextSnapshot,
        out string message,
        bool captureSession = false)
    {
        try
        {
            nextSnapshot = CreateIndustrialArmSnapshot(script, captureSession);
            message = FormatValidScriptStatus(nextSnapshot.FrameCount);
            return true;
        }
        catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
        {
            nextSnapshot = null;
            message = ScriptValidationMessageFormatter.Format(exception);
            return false;
        }
    }

    private void LoadScriptInto(
        TextBox target,
        Action<string, Color> setStatus,
        Action resetSnapshot,
        Action<string>? beforeLoad = null)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Load RobotStudio script",
            DefaultExt = ScriptFileDefaultExtension,
            Filter = ScriptFileDialogFilter,
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            beforeLoad?.Invoke(dialog.FileName);
            target.Text = File.ReadAllText(dialog.FileName, Encoding.UTF8);
            resetSnapshot();
            setStatus("Script loaded. Automatic validation is running.", Color.FromRgb(74, 222, 128));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            setStatus($"Could not load script: {exception.Message}", Color.FromRgb(248, 113, 113));
        }
    }

    private string FormatValidScriptStatus(int frameCount) => string.Format(
        CultureInfo.CurrentCulture,
        languageService.GetText("Script.ValidStatus"),
        frameCount);

    private void SelectCartesianDialectForFile(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName);
        if (extension.Equals(".gcode", StringComparison.OrdinalIgnoreCase))
        {
            ScriptDialectComboBox.SelectedItem = RobotScriptDialects.GCode;
        }
        else if (extension.Equals(".robot", StringComparison.OrdinalIgnoreCase))
        {
            ScriptDialectComboBox.SelectedItem = RobotScriptDialects.SimpleDsl;
        }
    }

    private void SelectScaraDialectForFile(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName);
        if (extension.Equals(".gcode", StringComparison.OrdinalIgnoreCase))
        {
            ScaraScriptDialectComboBox.SelectedItem = RobotScriptDialects.GCode;
        }
        else if (extension.Equals(".robot", StringComparison.OrdinalIgnoreCase))
        {
            ScaraScriptDialectComboBox.SelectedItem = RobotScriptDialects.SimpleDsl;
        }
    }

    private void SelectSimpleArmDialectForFile(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName);
        if (extension.Equals(".gcode", StringComparison.OrdinalIgnoreCase))
        {
            SimpleArmScriptDialectComboBox.SelectedItem = RobotScriptDialects.GCode;
        }
        else if (extension.Equals(".robot", StringComparison.OrdinalIgnoreCase))
        {
            SimpleArmScriptDialectComboBox.SelectedItem = RobotScriptDialects.SimpleDsl;
        }
    }

    private void SelectDeltaDialectForFile(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName);
        if (extension.Equals(".gcode", StringComparison.OrdinalIgnoreCase))
        {
            DeltaScriptDialectComboBox.SelectedItem = RobotScriptDialects.GCode;
        }
        else if (extension.Equals(".robot", StringComparison.OrdinalIgnoreCase))
        {
            DeltaScriptDialectComboBox.SelectedItem = RobotScriptDialects.SimpleDsl;
        }
    }

    private void SelectIndustrialArmDialectForFile(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName);
        if (extension.Equals(".gcode", StringComparison.OrdinalIgnoreCase))
        {
            IndustrialArmScriptDialectComboBox.SelectedItem = RobotScriptDialects.GCode;
        }
        else if (extension.Equals(".robot", StringComparison.OrdinalIgnoreCase))
        {
            IndustrialArmScriptDialectComboBox.SelectedItem = RobotScriptDialects.SimpleDsl;
        }
    }

    private void SaveScriptFrom(
        TextBox source,
        Action<string, Color> setStatus,
        string defaultExtension = ScriptFileDefaultExtension)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save RobotStudio script",
            DefaultExt = defaultExtension,
            Filter = ScriptFileDialogFilter,
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            File.WriteAllText(dialog.FileName, source.Text, Encoding.UTF8);
            setStatus("Script saved.", Color.FromRgb(74, 222, 128));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            setStatus($"Could not save script: {exception.Message}", Color.FromRgb(248, 113, 113));
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

    private void SetDeltaScriptStatus(
        string message,
        Color color)
    {
        DeltaScriptStatusText.Text = message;
        DeltaScriptStatusText.Foreground = new SolidColorBrush(color);
    }

    private void SetDroneScriptStatus(
        string message,
        Color color)
    {
        DroneScriptStatusText.Text = message;
        DroneScriptStatusText.Foreground = new SolidColorBrush(color);
    }

    private void SetIndustrialArmScriptStatus(
        string message,
        Color color)
    {
        IndustrialArmScriptStatusText.Text = message;
        IndustrialArmScriptStatusText.Foreground = new SolidColorBrush(color);
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
        ScriptEditorLineKind.UnitMode => Color.FromRgb(17, 94, 89),
        ScriptEditorLineKind.PositioningMode => Color.FromRgb(88, 28, 135),
        ScriptEditorLineKind.Wait => Color.FromRgb(133, 77, 14),
        ScriptEditorLineKind.Other => Color.FromRgb(127, 29, 29),
        _ => Color.FromRgb(30, 41, 59)
    };

    private static Color GetScriptCommandForeground(ScriptEditorLineKind kind) => kind switch
    {
        ScriptEditorLineKind.Home => Color.FromRgb(191, 219, 254),
        ScriptEditorLineKind.Move => Color.FromRgb(187, 247, 208),
        ScriptEditorLineKind.UnitMode => Color.FromRgb(153, 246, 228),
        ScriptEditorLineKind.PositioningMode => Color.FromRgb(233, 213, 255),
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
        RobotCapability.GCode => "G-code",
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
        RobotCapability.MixedJointMotion => "mixed joint motion",
        RobotCapability.SteeringKinematics => "steering kinematics",
        RobotCapability.HolonomicMotion => "holonomic motion",
        RobotCapability.FeedbackControl => "feedback control",
        RobotCapability.SensorSimulation => "sensor simulation",
        RobotCapability.PoseControl => "pose control",
        RobotCapability.SubsystemCoordination => "subsystem coordination",
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
}
