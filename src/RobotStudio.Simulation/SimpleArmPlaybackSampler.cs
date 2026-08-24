using RobotStudio.Domain.Articulated;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class SimpleArmPlaybackSampler
{
    private readonly SimpleArmKinematics kinematics;

    public SimpleArmPlaybackSampler()
        : this(new SimpleArmKinematics())
    {
    }

    public SimpleArmPlaybackSampler(SimpleArmKinematics kinematics)
    {
        ArgumentNullException.ThrowIfNull(kinematics);

        this.kinematics = kinematics;
    }

    public SimpleArmPlaybackSnapshot Sample(
        SimpleArmSimulationResult result,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interval),
                "Playback sample interval must be greater than zero.");
        }

        var frames = new List<SimpleArmPlaybackFrame>();
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
                var (joints, toolPose) = SampleMovement(
                    result.InitialContext.RobotProfile,
                    current,
                    next,
                    time);
                frames.Add(new SimpleArmPlaybackFrame(
                    time,
                    current.State,
                    joints,
                    toolPose,
                    current.CommandIndex,
                    current.CommandName,
                    current.CommandSource));
            }
        }

        AddFrameIfNeeded(frames, CreateFrame(result.InitialContext.RobotProfile, result.Timeline[^1]));

        return new SimpleArmPlaybackSnapshot(
            result.InitialContext.RobotProfile,
            frames.AsReadOnly(),
            result.FinalContext.ElapsedTime,
            result.Succeeded,
            result.Failure?.Message);
    }

    private (SimpleArmJointPosition Joints, SimpleArmToolPose ToolPose) SampleMovement(
        SimpleArmRobotProfile profile,
        SimpleArmSimulationStep current,
        SimpleArmSimulationStep next,
        TimeSpan time)
    {
        if (current.CartesianMotionPlan is
            {
                ProgressMotionProfile: not null
            } cartesianPlan)
        {
            var sample = cartesianPlan.ProgressMotionProfile.SampleAt(time - current.Time);
            var toolPose = SimpleArmCartesianMotionPlanner.Interpolate(
                cartesianPlan.StartToolPose,
                cartesianPlan.EndToolPose,
                sample.Progress);
            return (kinematics.InversePositiveBend(profile, toolPose), toolPose);
        }

        var progress = MotionProfileTimelineSampler.CalculateProgress(
            current.MotionProfile,
            current.Time,
            next.Time,
            time);
        var joints = Interpolate(current.Joints, next.Joints, progress);
        return (joints, kinematics.Forward(profile, joints));
    }

    private SimpleArmPlaybackFrame CreateFrame(
        SimpleArmRobotProfile profile,
        SimpleArmSimulationStep step) =>
        new(
            step.Time,
            step.State,
            step.Joints,
            kinematics.Forward(profile, step.Joints),
            step.CommandIndex,
            step.CommandName,
            step.CommandSource);

    private static void AddFrameIfNeeded(
        List<SimpleArmPlaybackFrame> frames,
        SimpleArmPlaybackFrame frame)
    {
        if (frames.Count == 0 || frames[^1].Time != frame.Time)
        {
            frames.Add(frame);
        }
    }

    private static SimpleArmJointPosition Interpolate(
        SimpleArmJointPosition start,
        SimpleArmJointPosition end,
        double progress)
    {
        var clampedProgress = Math.Clamp(progress, 0, 1);
        return new SimpleArmJointPosition(
            start.BaseDegrees + ((end.BaseDegrees - start.BaseDegrees) * clampedProgress),
            start.ShoulderDegrees + ((end.ShoulderDegrees - start.ShoulderDegrees) * clampedProgress),
            start.ElbowDegrees + ((end.ElbowDegrees - start.ElbowDegrees) * clampedProgress));
    }
}
