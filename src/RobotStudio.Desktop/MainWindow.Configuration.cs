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
    private void ApplyCartesianProfileButton_Click(
        object sender,
        RoutedEventArgs e) =>
        ApplyCartesianProfile(ReadCartesianProfileInput(), isRestore: false);

    private void RestoreCartesianProfileButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var defaultProfile = CreateCartesianProfile();
        var input = CartesianProfileInput.FromProfile(defaultProfile);
        PopulateCartesianProfileEditor(defaultProfile);
        ApplyCartesianProfile(input, isRestore: true);
    }

    private void ApplyCartesianProfile(
        CartesianProfileInput input,
        bool isRestore)
    {
        CartesianRobotProfile nextProfile;
        try
        {
            nextProfile = input.CreateProfile();
            nextProfile.ValidatePosition(new CartesianPosition(0, 0, 0));
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException)
        {
            SetCartesianProfileStatus(
                $"Profile was not changed: {exception.Message}",
                Color.FromRgb(248, 113, 113));
            return;
        }

        StopPlayback();
        profile = nextProfile;
        initialPosition = new CartesianPosition(0, 0, 0);
        cartesianSessionContext = null;
        UpdateSessionRecoveryControls();

        if (TryCreateSnapshotFromScript(
            ScriptEditorTextBox.Text,
            out var nextSnapshot,
            out var scriptMessage,
            captureSession: true))
        {
            snapshot = nextSnapshot;
            InitializeTimelineForSnapshot();
            RenderFrame(index: 0);
            SetCartesianProfileStatus(
                isRestore
                    ? "Default profile restored. Simulation reset to HOME."
                    : "Profile applied. Simulation reset to HOME.",
                Color.FromRgb(74, 222, 128));
            SetScriptStatus(scriptMessage, Color.FromRgb(74, 222, 128));
            return;
        }

        snapshot = CreateCartesianProfilePreview(nextProfile);
        InitializeTimelineForSnapshot();
        RenderFrame(index: 0);
        SetCartesianProfileStatus(
            "Profile applied. The current script is outside the new profile, so the viewport shows a HOME preview.",
            Color.FromRgb(251, 191, 36));
        SetScriptStatus(scriptMessage, Color.FromRgb(248, 113, 113));
    }

    private CartesianProfileInput ReadCartesianProfileInput() =>
        new(
            ProfileXMinimumTextBox.Text,
            ProfileXMaximumTextBox.Text,
            ProfileXVelocityTextBox.Text,
            ProfileXAccelerationTextBox.Text,
            ProfileYMinimumTextBox.Text,
            ProfileYMaximumTextBox.Text,
            ProfileYVelocityTextBox.Text,
            ProfileYAccelerationTextBox.Text,
            ProfileZMinimumTextBox.Text,
            ProfileZMaximumTextBox.Text,
            ProfileZVelocityTextBox.Text,
            ProfileZAccelerationTextBox.Text);

    private void PopulateCartesianProfileEditor(CartesianRobotProfile robotProfile)
    {
        var input = CartesianProfileInput.FromProfile(robotProfile);
        ProfileXMinimumTextBox.Text = input.XMinimum;
        ProfileXMaximumTextBox.Text = input.XMaximum;
        ProfileXVelocityTextBox.Text = input.XMaximumVelocity;
        ProfileXAccelerationTextBox.Text = input.XMaximumAcceleration;
        ProfileYMinimumTextBox.Text = input.YMinimum;
        ProfileYMaximumTextBox.Text = input.YMaximum;
        ProfileYVelocityTextBox.Text = input.YMaximumVelocity;
        ProfileYAccelerationTextBox.Text = input.YMaximumAcceleration;
        ProfileZMinimumTextBox.Text = input.ZMinimum;
        ProfileZMaximumTextBox.Text = input.ZMaximum;
        ProfileZVelocityTextBox.Text = input.ZMaximumVelocity;
        ProfileZAccelerationTextBox.Text = input.ZMaximumAcceleration;
        SetCartesianProfileStatus("Default Cartesian profile.", Color.FromRgb(148, 163, 184));
    }

    private void SetCartesianProfileStatus(
        string message,
        Color color)
    {
        CartesianProfileStatusText.Text = message;
        CartesianProfileStatusText.Foreground = new SolidColorBrush(color);
    }

    private void LoadCartesianScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        LoadScriptInto(
            ScriptEditorTextBox,
            SetScriptStatus,
            () => snapshot = null,
            SelectCartesianDialectForFile);

    private void SaveCartesianScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SaveScriptFrom(
            ScriptEditorTextBox,
            SetScriptStatus,
            CartesianScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode ? ".gcode" : ScriptFileDefaultExtension);

    private void LoadDifferentialDriveScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        LoadScriptInto(
            DifferentialDriveScriptTextBox,
            SetDifferentialDriveScriptStatus,
            () => differentialDriveSnapshot = null);

    private void SaveDifferentialDriveScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SaveScriptFrom(DifferentialDriveScriptTextBox, SetDifferentialDriveScriptStatus);

    private void LoadScaraScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        LoadScriptInto(
            ScaraScriptTextBox,
            SetScaraScriptStatus,
            () => scaraSnapshot = null,
            SelectScaraDialectForFile);

    private void SaveScaraScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SaveScriptFrom(
            ScaraScriptTextBox,
            SetScaraScriptStatus,
            ScaraScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
                ? ".gcode"
                : ScriptFileDefaultExtension);

    private void LoadSimpleArmScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        LoadScriptInto(
            SimpleArmScriptTextBox,
            SetSimpleArmScriptStatus,
            () => simpleArmSnapshot = null,
            SelectSimpleArmDialectForFile);

    private void SaveSimpleArmScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SaveScriptFrom(
            SimpleArmScriptTextBox,
            SetSimpleArmScriptStatus,
            SimpleArmScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
                ? ".gcode"
                : ScriptFileDefaultExtension);

    private void LoadDeltaScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        LoadScriptInto(
            DeltaScriptTextBox,
            SetDeltaScriptStatus,
            () => deltaSnapshot = null,
            SelectDeltaDialectForFile);

    private void SaveDeltaScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SaveScriptFrom(
            DeltaScriptTextBox,
            SetDeltaScriptStatus,
            DeltaScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
                ? ".gcode"
                : ScriptFileDefaultExtension);

    private void LoadDroneScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        LoadScriptInto(
            DroneScriptTextBox,
            SetDroneScriptStatus,
            () => droneSnapshot = null);

    private void SaveDroneScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SaveScriptFrom(DroneScriptTextBox, SetDroneScriptStatus);

    private void LoadIndustrialArmScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        LoadScriptInto(
            IndustrialArmScriptTextBox,
            SetIndustrialArmScriptStatus,
            () => industrialArmSnapshot = null,
            SelectIndustrialArmDialectForFile);

    private void SaveIndustrialArmScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SaveScriptFrom(
            IndustrialArmScriptTextBox,
            SetIndustrialArmScriptStatus,
            IndustrialArmScriptDialect.Descriptor.Id == RobotScriptDialectId.GCode
                ? ".gcode"
                : ScriptFileDefaultExtension);
}
