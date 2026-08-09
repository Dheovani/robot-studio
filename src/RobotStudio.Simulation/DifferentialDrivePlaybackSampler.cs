using RobotStudio.Domain;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation;

public sealed class DifferentialDrivePlaybackSampler
{
    public DifferentialDrivePlaybackSnapshot Sample(
        DifferentialDriveSimulationResult result,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interval),
                "Playback sample interval must be greater than zero.");
        }

        var frames = new List<DifferentialDrivePlaybackFrame>();
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
                var progress = MotionProfileTimelineSampler.CalculateProgress(
                    current.MotionProfile,
                    current.Time,
                    next.Time,
                    time);
                var pose = Interpolate(current.Pose, next.Pose, progress);
                frames.Add(new DifferentialDrivePlaybackFrame(
                    time,
                    current.State,
                    pose,
                    DifferentialDriveOdometryCalculator.Advance(
                        current.Odometry,
                        result.InitialContext.RobotProfile,
                        current.Pose,
                        pose),
                    current.CommandIndex,
                    current.CommandName,
                    current.CommandSource));
            }
        }

        AddFrameIfNeeded(frames, CreateFrame(result.Timeline[^1]));

        return new DifferentialDrivePlaybackSnapshot(
            result.InitialContext.RobotProfile,
            frames.AsReadOnly(),
            result.FinalContext.ElapsedTime,
            result.Succeeded,
            result.Failure?.Message);
    }

    private static DifferentialDrivePlaybackFrame CreateFrame(DifferentialDriveSimulationStep step) =>
        new(
            step.Time,
            step.State,
            step.Pose,
            step.Odometry,
            step.CommandIndex,
            step.CommandName,
            step.CommandSource);

    private static void AddFrameIfNeeded(
        List<DifferentialDrivePlaybackFrame> frames,
        DifferentialDrivePlaybackFrame frame)
    {
        if (frames.Count == 0 || frames[^1].Time != frame.Time)
        {
            frames.Add(frame);
        }
    }

    private static DifferentialDrivePose Interpolate(
        DifferentialDrivePose start,
        DifferentialDrivePose end,
        double progress)
    {
        var clampedProgress = Math.Clamp(progress, 0, 1);
        return new DifferentialDrivePose(
            start.X + ((end.X - start.X) * clampedProgress),
            start.Y + ((end.Y - start.Y) * clampedProgress),
            InterpolateHeading(start.HeadingDegrees, end.HeadingDegrees, clampedProgress));
    }

    private static double InterpolateHeading(
        double startDegrees,
        double endDegrees,
        double progress)
    {
        var delta = ((endDegrees - startDegrees + 180) % 360 + 360) % 360 - 180;
        return DifferentialDrivePose.NormalizeHeadingDegrees(startDegrees + (delta * progress));
    }
}
