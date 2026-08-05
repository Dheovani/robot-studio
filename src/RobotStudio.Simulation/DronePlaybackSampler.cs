using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;

namespace RobotStudio.Simulation;

public sealed class DronePlaybackSampler
{
    public DronePlaybackSnapshot Sample(
        DroneSimulationResult result,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interval),
                "Playback sample interval must be greater than zero.");
        }

        var frames = new List<DronePlaybackFrame>();
        for (var index = 0; index < result.Timeline.Count; index++)
        {
            var current = result.Timeline[index];
            var next = index + 1 < result.Timeline.Count
                ? result.Timeline[index + 1]
                : null;

            if (next is null || next.Time <= current.Time)
            {
                AddFrameIfNeeded(frames, CreateFrame(current));
                continue;
            }

            for (var time = current.Time; time < next.Time; time += interval)
            {
                var progress = (time - current.Time).TotalSeconds / (next.Time - current.Time).TotalSeconds;
                frames.Add(new DronePlaybackFrame(
                    time,
                    current.State,
                    Interpolate(current.Pose, next.Pose, progress),
                    current.CommandIndex,
                    current.CommandName,
                    current.CommandSource));
            }
        }

        AddFrameIfNeeded(frames, CreateFrame(result.Timeline[^1]));

        return new DronePlaybackSnapshot(
            result.InitialContext.RobotProfile,
            frames.AsReadOnly(),
            result.FinalContext.ElapsedTime,
            result.Succeeded,
            result.Failure?.Message);
    }

    private static DronePlaybackFrame CreateFrame(DroneSimulationStep step) =>
        new(
            step.Time,
            step.State,
            step.Pose,
            step.CommandIndex,
            step.CommandName,
            step.CommandSource);

    private static void AddFrameIfNeeded(
        List<DronePlaybackFrame> frames,
        DronePlaybackFrame frame)
    {
        if (frames.Count == 0 || frames[^1].Time != frame.Time)
        {
            frames.Add(frame);
        }
    }

    private static DronePose Interpolate(
        DronePose start,
        DronePose end,
        double progress)
    {
        var clampedProgress = Math.Clamp(progress, 0, 1);
        var yawDelta = DronePose.NormalizeSignedDegrees(end.YawDegrees - start.YawDegrees);

        return new DronePose(
            start.XMillimeters + ((end.XMillimeters - start.XMillimeters) * clampedProgress),
            start.YMillimeters + ((end.YMillimeters - start.YMillimeters) * clampedProgress),
            start.ZMillimeters + ((end.ZMillimeters - start.ZMillimeters) * clampedProgress),
            DronePose.NormalizeYawDegrees(start.YawDegrees + (yawDelta * clampedProgress)));
    }
}
