using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public sealed class ScaraPlaybackSampler
{
    private readonly ScaraKinematics kinematics;

    public ScaraPlaybackSampler()
        : this(new ScaraKinematics())
    {
    }

    public ScaraPlaybackSampler(ScaraKinematics kinematics)
    {
        ArgumentNullException.ThrowIfNull(kinematics);

        this.kinematics = kinematics;
    }

    public ScaraPlaybackSnapshot Sample(
        ScaraSimulationResult result,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interval),
                "Playback sample interval must be greater than zero.");
        }

        var frames = new List<ScaraPlaybackFrame>();
        for (var index = 0; index < result.Timeline.Count; index++)
        {
            var current = result.Timeline[index];
            var next = index + 1 < result.Timeline.Count
                ? result.Timeline[index + 1]
                : null;

            if (next is null || next.Time <= current.Time)
            {
                AddFrameIfNeeded(frames, CreateFrame(result.InitialContext.RobotProfile, current));
                continue;
            }

            for (var time = current.Time; time < next.Time; time += interval)
            {
                var progress = MotionProfileTimelineSampler.CalculateProgress(
                    current.MotionProfile,
                    current.Time,
                    next.Time,
                    time);
                var joints = Interpolate(current.Joints, next.Joints, progress);
                frames.Add(new ScaraPlaybackFrame(
                    time,
                    current.State,
                    joints,
                    kinematics.Forward(result.InitialContext.RobotProfile, joints),
                    current.CommandIndex,
                    current.CommandName,
                    current.CommandSource));
            }
        }

        AddFrameIfNeeded(frames, CreateFrame(result.InitialContext.RobotProfile, result.Timeline[^1]));

        return new ScaraPlaybackSnapshot(
            result.InitialContext.RobotProfile,
            frames.AsReadOnly(),
            result.FinalContext.ElapsedTime,
            result.Succeeded,
            result.Failure?.Message);
    }

    private ScaraPlaybackFrame CreateFrame(
        ScaraRobotProfile profile,
        ScaraSimulationStep step) =>
        new(
            step.Time,
            step.State,
            step.Joints,
            kinematics.Forward(profile, step.Joints),
            step.CommandIndex,
            step.CommandName,
            step.CommandSource);

    private static void AddFrameIfNeeded(
        List<ScaraPlaybackFrame> frames,
        ScaraPlaybackFrame frame)
    {
        if (frames.Count == 0 || frames[^1].Time != frame.Time)
        {
            frames.Add(frame);
        }
    }

    private static ScaraJointPosition Interpolate(
        ScaraJointPosition start,
        ScaraJointPosition end,
        double progress)
    {
        var clampedProgress = Math.Clamp(progress, 0, 1);
        return new ScaraJointPosition(
            start.ShoulderDegrees + ((end.ShoulderDegrees - start.ShoulderDegrees) * clampedProgress),
            start.ElbowDegrees + ((end.ElbowDegrees - start.ElbowDegrees) * clampedProgress));
    }
}
