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
    private void LoadActiveScript()
    {
        switch (activeViewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                LoadDifferentialDriveScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.ScaraThreeDimensional:
                LoadScaraScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.SimpleArmThreeDimensional:
                LoadSimpleArmScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.DeltaThreeDimensional:
                LoadDeltaScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.DroneThreeDimensional:
                LoadDroneScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.IndustrialArmThreeDimensional:
                LoadIndustrialArmScriptButton_Click(this, new RoutedEventArgs());
                break;

            default:
                LoadCartesianScriptButton_Click(this, new RoutedEventArgs());
                break;
        }
    }

    private void SaveActiveScript()
    {
        switch (activeViewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                SaveDifferentialDriveScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.ScaraThreeDimensional:
                SaveScaraScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.SimpleArmThreeDimensional:
                SaveSimpleArmScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.DeltaThreeDimensional:
                SaveDeltaScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.DroneThreeDimensional:
                SaveDroneScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.IndustrialArmThreeDimensional:
                SaveIndustrialArmScriptButton_Click(this, new RoutedEventArgs());
                break;

            default:
                SaveCartesianScriptButton_Click(this, new RoutedEventArgs());
                break;
        }
    }

    private void SimulateActiveScript()
    {
        switch (activeViewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                SimulateDifferentialDriveScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.ScaraThreeDimensional:
                SimulateScaraScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.SimpleArmThreeDimensional:
                SimulateSimpleArmScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.DeltaThreeDimensional:
                SimulateDeltaScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.DroneThreeDimensional:
                SimulateDroneScriptButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.IndustrialArmThreeDimensional:
                SimulateIndustrialArmScriptButton_Click(this, new RoutedEventArgs());
                break;

            default:
                SimulateScriptButton_Click(this, new RoutedEventArgs());
                break;
        }
    }

    private void SimulateActiveScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SimulateActiveScript();

    private void LoadActiveExampleButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        switch (activeViewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                LoadDifferentialDriveExampleButton_Click(sender, e);
                break;
            case RobotViewerKind.ScaraThreeDimensional:
                LoadScaraExampleButton_Click(sender, e);
                break;
            case RobotViewerKind.SimpleArmThreeDimensional:
                LoadSimpleArmExampleButton_Click(sender, e);
                break;
            case RobotViewerKind.DeltaThreeDimensional:
                LoadDeltaExampleButton_Click(sender, e);
                break;
            case RobotViewerKind.DroneThreeDimensional:
                LoadDroneExampleButton_Click(sender, e);
                break;
            case RobotViewerKind.IndustrialArmThreeDimensional:
                LoadIndustrialArmExampleButton_Click(sender, e);
                break;
            case RobotViewerKind.CartesianThreeDimensional:
            case RobotViewerKind.XYPlotterTwoDimensional:
                LoadCartesianExampleButton_Click(sender, e);
                break;
        }
    }

    private void LoadActiveScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        LoadActiveScript();

    private void SaveActiveScriptButton_Click(
        object sender,
        RoutedEventArgs e) =>
        SaveActiveScript();

    private void ResetActivePlayback()
    {
        switch (activeViewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                DifferentialDriveResetButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.ScaraThreeDimensional:
                ScaraResetButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.SimpleArmThreeDimensional:
                SimpleArmResetButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.DeltaThreeDimensional:
                DeltaResetButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.DroneThreeDimensional:
                DroneResetButton_Click(this, new RoutedEventArgs());
                break;

            case RobotViewerKind.IndustrialArmThreeDimensional:
                IndustrialArmResetButton_Click(this, new RoutedEventArgs());
                break;

            default:
                ResetButton_Click(this, new RoutedEventArgs());
                break;
        }
    }

    private void MoveActiveFrame(int delta)
    {
        StopPlayback();

        switch (activeViewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                RenderDifferentialDriveFrame(differentialDriveFrameIndex + delta);
                break;

            case RobotViewerKind.ScaraThreeDimensional:
                RenderScaraFrame(scaraFrameIndex + delta);
                break;

            case RobotViewerKind.SimpleArmThreeDimensional:
                RenderSimpleArmFrame(simpleArmFrameIndex + delta);
                break;

            case RobotViewerKind.DeltaThreeDimensional:
                RenderDeltaFrame(deltaFrameIndex + delta);
                break;

            case RobotViewerKind.DroneThreeDimensional:
                RenderDroneFrame(droneFrameIndex + delta);
                break;

            case RobotViewerKind.IndustrialArmThreeDimensional:
                RenderIndustrialArmFrame(industrialArmFrameIndex + delta);
                break;

            default:
                RenderFrame(currentFrameIndex + delta);
                break;
        }
    }

    private void ZoomActiveCamera(double delta)
    {
        switch (activeViewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                differentialDriveZoomMultiplier = Math.Clamp(differentialDriveZoomMultiplier + delta, 0.55, 4);
                RenderDifferentialDriveFrame(differentialDriveFrameIndex);
                break;

            case RobotViewerKind.ScaraThreeDimensional:
                scaraZoomMultiplier = Math.Clamp(scaraZoomMultiplier + delta, 0.55, 4);
                RenderScaraFrame(scaraFrameIndex);
                break;

            case RobotViewerKind.SimpleArmThreeDimensional:
                simpleArmZoomMultiplier = Math.Clamp(simpleArmZoomMultiplier + delta, 0.55, 4);
                RenderSimpleArmFrame(simpleArmFrameIndex);
                break;

            case RobotViewerKind.DeltaThreeDimensional:
                deltaZoomMultiplier = Math.Clamp(deltaZoomMultiplier + delta, 0.55, 4);
                RenderDeltaFrame(deltaFrameIndex);
                break;

            case RobotViewerKind.DroneThreeDimensional:
                droneZoomMultiplier = Math.Clamp(droneZoomMultiplier + delta, 0.55, 4);
                RenderDroneFrame(droneFrameIndex);
                break;

            case RobotViewerKind.IndustrialArmThreeDimensional:
                industrialArmZoomMultiplier = Math.Clamp(industrialArmZoomMultiplier + delta, 0.55, 4);
                RenderIndustrialArmFrame(industrialArmFrameIndex);
                break;

            case RobotViewerKind.CartesianThreeDimensional:
            case RobotViewerKind.XYPlotterTwoDimensional:
                SetCameraControls(
                    azimuth: azimuthDegrees,
                    elevation: elevationDegrees,
                    zoom: Math.Clamp(zoomMultiplier + delta, ZoomSlider.Minimum, ZoomSlider.Maximum));
                break;
        }
    }

    private void ResetActiveCamera()
    {
        switch (activeViewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                differentialDriveZoomMultiplier = 1;
                RenderDifferentialDriveFrame(differentialDriveFrameIndex);
                break;

            case RobotViewerKind.ScaraThreeDimensional:
                scaraAzimuthDegrees = -45;
                scaraElevationDegrees = 32;
                scaraZoomMultiplier = 1.8;
                RenderScaraFrame(scaraFrameIndex);
                break;

            case RobotViewerKind.SimpleArmThreeDimensional:
                simpleArmAzimuthDegrees = -45;
                simpleArmElevationDegrees = 30;
                simpleArmZoomMultiplier = 2.15;
                RenderSimpleArmFrame(simpleArmFrameIndex);
                break;

            case RobotViewerKind.DeltaThreeDimensional:
                deltaAzimuthDegrees = -45;
                deltaElevationDegrees = 32;
                deltaZoomMultiplier = 1.75;
                RenderDeltaFrame(deltaFrameIndex);
                break;

            case RobotViewerKind.DroneThreeDimensional:
                droneAzimuthDegrees = -45;
                droneElevationDegrees = 34;
                droneZoomMultiplier = 1.55;
                RenderDroneFrame(droneFrameIndex);
                break;

            case RobotViewerKind.IndustrialArmThreeDimensional:
                industrialArmAzimuthDegrees = -45;
                industrialArmElevationDegrees = 28;
                industrialArmZoomMultiplier = 1;
                RenderIndustrialArmFrame(industrialArmFrameIndex);
                break;

            case RobotViewerKind.CartesianThreeDimensional:
            case RobotViewerKind.XYPlotterTwoDimensional:
                SetCameraControls(azimuth: -45, elevation: 35, zoom: 1);
                break;
        }
    }

    private void UpdateSessionRecoveryControls()
    {
        var visibility = GetActiveSessionState() == RobotState.Faulted
            ? Visibility.Visible
            : Visibility.Collapsed;
        foreach (var panel in sessionRecoveryPanels)
        {
            panel.Visibility = visibility;
        }
    }

    private RobotState? GetActiveSessionState() => activeViewerKind switch
    {
        RobotViewerKind.DifferentialDriveTwoDimensional => differentialDriveSessionContext?.State,
        RobotViewerKind.ScaraThreeDimensional => scaraSessionContext?.State,
        RobotViewerKind.SimpleArmThreeDimensional => simpleArmSessionContext?.State,
        RobotViewerKind.DeltaThreeDimensional => deltaSessionContext?.State,
        RobotViewerKind.DroneThreeDimensional => droneSessionContext?.State,
        RobotViewerKind.IndustrialArmThreeDimensional => industrialArmSessionContext?.State,
        RobotViewerKind.CartesianThreeDimensional or RobotViewerKind.XYPlotterTwoDimensional => cartesianSessionContext?.State,
        _ => null
    };

    private void ExecuteSessionRecoveryCommand(string commandText)
    {
        if (commandText == "RESET" && GetActiveSessionState() != RobotState.Faulted)
        {
            SetActiveScriptStatus(
                "RESET is available only after the active simulation enters Faulted.",
                Color.FromRgb(248, 113, 113));
            return;
        }

        StopPlayback();
        var commands = simpleDslDialect.Parse(commandText);

        switch (activeViewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                ExecuteDifferentialDriveRecovery(commands, commandText);
                break;
            case RobotViewerKind.ScaraThreeDimensional:
                ExecuteScaraRecovery(commands, commandText);
                break;
            case RobotViewerKind.SimpleArmThreeDimensional:
                ExecuteSimpleArmRecovery(commands, commandText);
                break;
            case RobotViewerKind.DeltaThreeDimensional:
                ExecuteDeltaRecovery(commands, commandText);
                break;
            case RobotViewerKind.DroneThreeDimensional:
                ExecuteDroneRecovery(commands, commandText);
                break;
            case RobotViewerKind.IndustrialArmThreeDimensional:
                ExecuteIndustrialArmRecovery(commands, commandText);
                break;
            case RobotViewerKind.CartesianThreeDimensional:
            case RobotViewerKind.XYPlotterTwoDimensional:
                ExecuteCartesianRecovery(commands, commandText);
                break;
        }

        UpdateSessionRecoveryControls();
    }

    private void ExecuteCartesianRecovery(
        RobotCommandSequence commands,
        string commandText)
    {
        var context = cartesianSessionContext ?? SimulationContext.Create(profile, initialPosition);
        var result = new RobotSimulator().Execute(context, commands);
        cartesianSessionContext = result.FinalContext;
        snapshot = new CartesianPlaybackSnapshotBuilder()
            .Build(profile, result, TimeSpan.FromMilliseconds(100));
        InitializeTimelineForSnapshot();
        RenderFrame(snapshot.SceneFrameCount - 1);
        SetRecoveryStatus(commandText, result.Succeeded, result.Failure);
    }

    private void ExecuteDifferentialDriveRecovery(
        RobotCommandSequence commands,
        string commandText)
    {
        var profile = CreateDifferentialDriveProfile();
        var context = differentialDriveSessionContext ?? DifferentialDriveSimulationContext.Create(
            profile,
            new DifferentialDrivePose(X: 60, Y: 50, HeadingDegrees: 0));
        var result = new DifferentialDriveSimulator().Execute(context, commands);
        differentialDriveSessionContext = result.FinalContext;
        differentialDriveSnapshot = new DifferentialDrivePlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
        DifferentialDriveTimeline.Maximum = differentialDriveSnapshot.FrameCount - 1;
        RenderDifferentialDriveFrame(differentialDriveSnapshot.FrameCount - 1);
        SetRecoveryStatus(commandText, result.Succeeded, result.Failure);
    }

    private void ExecuteScaraRecovery(
        RobotCommandSequence commands,
        string commandText)
    {
        var profile = CreateScaraProfile();
        var context = scaraSessionContext ?? ScaraSimulationContext.Create(
            profile,
            new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0));
        var result = new ScaraSimulator().Execute(context, commands);
        scaraSessionContext = result.FinalContext;
        scaraSnapshot = new ScaraPlaybackSampler().Sample(result, TimeSpan.FromMilliseconds(100));
        ScaraTimeline.Maximum = scaraSnapshot.FrameCount - 1;
        RenderScaraFrame(scaraSnapshot.FrameCount - 1);
        SetRecoveryStatus(commandText, result.Succeeded, result.Failure);
    }

    private void ExecuteSimpleArmRecovery(
        RobotCommandSequence commands,
        string commandText)
    {
        var profile = CreateSimpleArmProfile();
        var context = simpleArmSessionContext ?? SimpleArmSimulationContext.Create(
            profile,
            new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0));
        var result = new SimpleArmSimulator().Execute(context, commands);
        simpleArmSessionContext = result.FinalContext;
        simpleArmSnapshot = new SimpleArmPlaybackSampler().Sample(result, TimeSpan.FromMilliseconds(100));
        SimpleArmTimeline.Maximum = simpleArmSnapshot.FrameCount - 1;
        RenderSimpleArmFrame(simpleArmSnapshot.FrameCount - 1);
        SetRecoveryStatus(commandText, result.Succeeded, result.Failure);
    }

    private void ExecuteDeltaRecovery(
        RobotCommandSequence commands,
        string commandText)
    {
        var profile = CreateDeltaProfile();
        var context = deltaSessionContext ?? DeltaSimulationContext.Create(
            profile,
            new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 0, CMillimeters: 0));
        var result = new DeltaSimulator().Execute(context, commands);
        deltaSessionContext = result.FinalContext;
        deltaSnapshot = new DeltaPlaybackSampler().Sample(result, TimeSpan.FromMilliseconds(100));
        DeltaTimeline.Maximum = deltaSnapshot.FrameCount - 1;
        RenderDeltaFrame(deltaSnapshot.FrameCount - 1);
        SetRecoveryStatus(commandText, result.Succeeded, result.Failure);
    }

    private void ExecuteDroneRecovery(
        RobotCommandSequence commands,
        string commandText)
    {
        var profile = CreateDroneProfile();
        var context = droneSessionContext ?? DroneSimulationContext.Create(
            profile,
            new DronePose(
                XMillimeters: 0,
                YMillimeters: 0,
                ZMillimeters: 0,
                YawDegrees: 0));
        var result = new DroneSimulator().Execute(context, commands);
        droneSessionContext = result.FinalContext;
        droneSnapshot = new DronePlaybackSampler().Sample(result, TimeSpan.FromMilliseconds(100));
        DroneTimeline.Maximum = droneSnapshot.FrameCount - 1;
        RenderDroneFrame(droneSnapshot.FrameCount - 1);
        SetRecoveryStatus(commandText, result.Succeeded, result.Failure);
    }

    private void ExecuteIndustrialArmRecovery(
        RobotCommandSequence commands,
        string commandText)
    {
        var profile = CreateIndustrialArmProfile();
        var context = industrialArmSessionContext ?? IndustrialArmSimulationContext.Create(
            profile,
            IndustrialArmJointPosition.Home);
        var result = new IndustrialArmSimulator().Execute(context, commands);
        industrialArmSessionContext = result.FinalContext;
        industrialArmSnapshot = new IndustrialArmPlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
        IndustrialArmTimeline.Maximum = industrialArmSnapshot.FrameCount - 1;
        RenderIndustrialArmFrame(industrialArmSnapshot.FrameCount - 1);
        SetRecoveryStatus(commandText, result.Succeeded, result.Failure);
    }

    private void SetRecoveryStatus(
        string commandText,
        bool succeeded,
        Exception? failure)
    {
        var message = succeeded
            ? $"{commandText} executed from the preserved desktop session context."
            : ScriptValidationMessageFormatter.Format(failure!);
        SetActiveScriptStatus(
            message,
            succeeded ? Color.FromRgb(74, 222, 128) : Color.FromRgb(248, 113, 113));
    }

    private void SetActiveScriptStatus(string message, Color color) =>
        SetScriptStatus(activeViewerKind, message, color);

    private void SetScriptStatus(
        RobotViewerKind viewerKind,
        string message,
        Color color)
    {
        switch (viewerKind)
        {
            case RobotViewerKind.DifferentialDriveTwoDimensional:
                SetDifferentialDriveScriptStatus(message, color);
                break;
            case RobotViewerKind.ScaraThreeDimensional:
                SetScaraScriptStatus(message, color);
                break;
            case RobotViewerKind.SimpleArmThreeDimensional:
                SetSimpleArmScriptStatus(message, color);
                break;
            case RobotViewerKind.DeltaThreeDimensional:
                SetDeltaScriptStatus(message, color);
                break;
            case RobotViewerKind.DroneThreeDimensional:
                SetDroneScriptStatus(message, color);
                break;
            case RobotViewerKind.IndustrialArmThreeDimensional:
                SetIndustrialArmScriptStatus(message, color);
                break;
            case RobotViewerKind.CartesianThreeDimensional:
            case RobotViewerKind.XYPlotterTwoDimensional:
                SetScriptStatus(message, color);
                break;
        }
    }
}
