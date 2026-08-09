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
                var translationProgress = MotionProfileTimelineSampler.CalculateSynchronizedProgress(
                    current.TranslationProfile,
                    current.Time,
                    next.Time,
                    time);
                var yawProgress = MotionProfileTimelineSampler.CalculateSynchronizedProgress(
                    current.YawProfile,
                    current.Time,
                    next.Time,
                    time);
                frames.Add(new DronePlaybackFrame(
                    time,
                    current.State,
                    Interpolate(current.Pose, next.Pose, translationProgress, yawProgress),
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
        double translationProgress,
        double yawProgress)
    {
        var clampedTranslationProgress = Math.Clamp(translationProgress, 0, 1);
        var clampedYawProgress = Math.Clamp(yawProgress, 0, 1);
        var yawDelta = DronePose.NormalizeSignedDegrees(end.YawDegrees - start.YawDegrees);

        return new DronePose(
            start.XMillimeters + ((end.XMillimeters - start.XMillimeters) * clampedTranslationProgress),
            start.YMillimeters + ((end.YMillimeters - start.YMillimeters) * clampedTranslationProgress),
            start.ZMillimeters + ((end.ZMillimeters - start.ZMillimeters) * clampedTranslationProgress),
            DronePose.NormalizeYawDegrees(start.YawDegrees + (yawDelta * clampedYawProgress)));
    }
}
