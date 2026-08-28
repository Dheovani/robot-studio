using RobotStudio.Desktop.Rendering.SceneComposers;
using RobotStudio.Desktop.Viewers;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop;

public partial class MainWindow
{
    private void RenderDifferentialDriveFrame(int index)
    {
        if (differentialDriveSnapshot is null)
        {
            return;
        }

        differentialDriveFrameIndex = Math.Clamp(index, 0, differentialDriveSnapshot.FrameCount - 1);
        DifferentialDriveTimeline.Value = differentialDriveFrameIndex;
        var frame = differentialDriveSnapshot.Frames[differentialDriveFrameIndex];
        differentialDriveCanvasPresenter.Present(
            DifferentialDriveSchematicSceneComposer.Compose(
                differentialDriveSnapshot,
                differentialDriveFrameIndex,
                new System.Windows.Size(
                    DifferentialDriveCanvas.ActualWidth,
                    DifferentialDriveCanvas.ActualHeight),
                differentialDriveZoomMultiplier));

        var status = RobotFramePresenter.Create(
            frame,
            differentialDriveFrameIndex,
            differentialDriveSnapshot.FrameCount,
            differentialDriveSnapshot.TotalDuration);
        DifferentialDriveStateText.Text = status.State;
        DifferentialDrivePoseText.Text = status.PrimaryPose;
        DifferentialDriveCommandText.Text = status.Command;
        DifferentialDriveTimeText.Text = status.Time;
        DifferentialDriveFramesText.Text = status.Frames;
        DifferentialDriveTimeline.State = frame.State;
        DifferentialDriveTimeline.Status = status.Footer;
    }

    private void RenderScaraFrame(int index)
    {
        if (scaraSnapshot is null)
        {
            return;
        }

        scaraFrameIndex = Math.Clamp(index, 0, scaraSnapshot.FrameCount - 1);
        ScaraTimeline.Value = scaraFrameIndex;
        var frame = scaraSnapshot.Frames[scaraFrameIndex];
        scaraViewportPresenter.Present(ScaraSchematicSceneComposer.Compose(
            scaraSnapshot,
            scaraFrameIndex,
            scaraAzimuthDegrees,
            scaraElevationDegrees,
            scaraZoomMultiplier));
        var status = RobotFramePresenter.Create(frame, scaraFrameIndex, scaraSnapshot.FrameCount, scaraSnapshot.TotalDuration);
        ScaraStateText.Text = status.State;
        ScaraJointsText.Text = status.PrimaryPose;
        ScaraToolText.Text = RobotFramePresenter.FormatScaraToolPose(frame);
        ScaraCommandText.Text = status.Command;
        ScaraTimeText.Text = status.Time;
        ScaraTimeline.State = frame.State;
        ScaraTimeline.Status = status.Footer;
        ScaraMovementExplanationText.Text = status.MovementExplanation;
    }

    private void RenderSimpleArmFrame(int index)
    {
        if (simpleArmSnapshot is null)
        {
            return;
        }

        simpleArmFrameIndex = Math.Clamp(index, 0, simpleArmSnapshot.FrameCount - 1);
        SimpleArmTimeline.Value = simpleArmFrameIndex;
        var frame = simpleArmSnapshot.Frames[simpleArmFrameIndex];
        simpleArmViewportPresenter.Present(SimpleArmSchematicSceneComposer.Compose(
            simpleArmSnapshot,
            simpleArmFrameIndex,
            simpleArmAzimuthDegrees,
            simpleArmElevationDegrees,
            simpleArmZoomMultiplier));
        var status = RobotFramePresenter.Create(frame, simpleArmFrameIndex, simpleArmSnapshot.FrameCount, simpleArmSnapshot.TotalDuration);
        SimpleArmStateText.Text = status.State;
        SimpleArmJointsText.Text = status.PrimaryPose;
        SimpleArmToolText.Text = RobotFramePresenter.FormatSimpleArmToolPose(frame);
        SimpleArmCommandText.Text = status.Command;
        SimpleArmTimeText.Text = status.Time;
        SimpleArmTimeline.State = frame.State;
        SimpleArmTimeline.Status = status.Footer;
        SimpleArmMovementExplanationText.Text = status.MovementExplanation;
    }

    private void RenderDeltaFrame(int index)
    {
        if (deltaSnapshot is null)
        {
            return;
        }

        deltaFrameIndex = Math.Clamp(index, 0, deltaSnapshot.FrameCount - 1);
        DeltaTimeline.Value = deltaFrameIndex;
        var frame = deltaSnapshot.Frames[deltaFrameIndex];
        deltaViewportPresenter.Present(DeltaSchematicSceneComposer.Compose(
            deltaSnapshot,
            deltaFrameIndex,
            deltaAzimuthDegrees,
            deltaElevationDegrees,
            deltaZoomMultiplier));
        var status = RobotFramePresenter.Create(frame, deltaFrameIndex, deltaSnapshot.FrameCount, deltaSnapshot.TotalDuration);
        DeltaStateText.Text = status.State;
        DeltaActuatorsText.Text = status.PrimaryPose;
        DeltaToolText.Text = RobotFramePresenter.FormatDeltaToolPose(frame);
        DeltaCommandText.Text = status.Command;
        DeltaTimeText.Text = status.Time;
        DeltaTimeline.State = frame.State;
        DeltaTimeline.Status = status.Footer;
        DeltaMovementExplanationText.Text = status.MovementExplanation;
    }

    private void RenderDroneFrame(int index)
    {
        if (droneSnapshot is null)
        {
            return;
        }

        droneFrameIndex = Math.Clamp(index, 0, droneSnapshot.FrameCount - 1);
        DroneTimeline.Value = droneFrameIndex;
        var frame = droneSnapshot.Frames[droneFrameIndex];
        droneViewportPresenter.Present(DroneSchematicSceneComposer.Compose(
            droneSnapshot,
            droneFrameIndex,
            droneAzimuthDegrees,
            droneElevationDegrees,
            droneZoomMultiplier));
        var status = RobotFramePresenter.Create(frame, droneFrameIndex, droneSnapshot.FrameCount, droneSnapshot.TotalDuration);
        DroneStateText.Text = status.State;
        DronePoseText.Text = status.PrimaryPose;
        DroneYawText.Text = RobotFramePresenter.FormatDroneAttitude(frame);
        DroneCommandText.Text = status.Command;
        DroneTimeText.Text = status.Time;
        DroneTimeline.State = frame.State;
        DroneTimeline.Status = status.Footer;
        DroneMovementExplanationText.Text = status.MovementExplanation;
    }

    private void RenderIndustrialArmFrame(int index)
    {
        if (industrialArmSnapshot is null)
        {
            return;
        }

        industrialArmFrameIndex = Math.Clamp(index, 0, industrialArmSnapshot.FrameCount - 1);
        IndustrialArmTimeline.Value = industrialArmFrameIndex;
        var frame = industrialArmSnapshot.Frames[industrialArmFrameIndex];
        industrialArmViewportPresenter.Present(IndustrialArmSchematicSceneComposer.Compose(
            industrialArmSnapshot,
            industrialArmFrameIndex,
            industrialArmAzimuthDegrees,
            industrialArmElevationDegrees,
            industrialArmZoomMultiplier));
        var status = RobotFramePresenter.Create(frame, industrialArmFrameIndex, industrialArmSnapshot.FrameCount, industrialArmSnapshot.TotalDuration);
        IndustrialArmStateText.Text = status.State;
        IndustrialArmJointsText.Text = status.PrimaryPose;
        IndustrialArmToolText.Text = RobotFramePresenter.FormatIndustrialArmToolPose(frame);
        IndustrialArmCommandText.Text = status.Command;
        IndustrialArmTimeText.Text = status.Time;
        IndustrialArmTimeline.State = frame.State;
        IndustrialArmTimeline.Status = status.Footer;
        IndustrialArmMovementExplanationText.Text = status.MovementExplanation;
    }
}
