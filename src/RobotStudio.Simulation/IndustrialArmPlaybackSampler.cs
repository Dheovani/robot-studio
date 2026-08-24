using RobotStudio.Domain.Articulated;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class IndustrialArmPlaybackSampler
{
    private readonly IndustrialArmKinematics kinematics = new();

    public IndustrialArmPlaybackSnapshot Sample(
        IndustrialArmSimulationResult result,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Playback sample interval must be greater than zero.");
        }

        var frames = new List<IndustrialArmPlaybackFrame>();
        for (var index = 0; index < result.Timeline.Count; index++)
        {
            var current = result.Timeline[index];
            var next = index + 1 < result.Timeline.Count ? result.Timeline[index + 1] : null;

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
                var joints = current.CartesianMotionPlan is { } cartesianPlan
                    ? kinematics.Inverse(
                        result.InitialContext.RobotProfile,
                        IndustrialArmCartesianMotionPlanner.Interpolate(
                            cartesianPlan.StartToolPose,
                            cartesianPlan.EndToolPose,
                            progress),
                        cartesianPlan.Configuration)
                    : Interpolate(current.Joints, next.Joints, progress);
                frames.Add(new IndustrialArmPlaybackFrame(
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

        return new IndustrialArmPlaybackSnapshot(
            result.InitialContext.RobotProfile,
            frames.AsReadOnly(),
            result.FinalContext.ElapsedTime,
            result.Succeeded,
            result.Failure?.Message);
    }

    private IndustrialArmPlaybackFrame CreateFrame(
        IndustrialArmRobotProfile profile,
        IndustrialArmSimulationStep step) =>
        new(
            step.Time,
            step.State,
            step.Joints,
            kinematics.Forward(profile, step.Joints),
            step.CommandIndex,
            step.CommandName,
            step.CommandSource);

    private static void AddFrameIfNeeded(
        List<IndustrialArmPlaybackFrame> frames,
        IndustrialArmPlaybackFrame frame)
    {
        if (frames.Count == 0 || frames[^1].Time != frame.Time)
        {
            frames.Add(frame);
        }
    }

    private static IndustrialArmJointPosition Interpolate(
        IndustrialArmJointPosition start,
        IndustrialArmJointPosition end,
        double progress)
    {
        var t = Math.Clamp(progress, 0, 1);
        return new IndustrialArmJointPosition(
            Lerp(start.J1Degrees, end.J1Degrees, t),
            Lerp(start.J2Degrees, end.J2Degrees, t),
            Lerp(start.J3Degrees, end.J3Degrees, t),
            Lerp(start.J4Degrees, end.J4Degrees, t),
            Lerp(start.J5Degrees, end.J5Degrees, t),
            Lerp(start.J6Degrees, end.J6Degrees, t));
    }

    private static double Lerp(double start, double end, double progress) =>
        start + ((end - start) * progress);
}
