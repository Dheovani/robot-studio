using RobotStudio.Domain.Parallel;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class DeltaPlaybackSampler
{
    private readonly DeltaKinematics kinematics;

    public DeltaPlaybackSampler()
        : this(new DeltaKinematics())
    {
    }

    public DeltaPlaybackSampler(DeltaKinematics kinematics)
    {
        ArgumentNullException.ThrowIfNull(kinematics);

        this.kinematics = kinematics;
    }

    public DeltaPlaybackSnapshot Sample(
        DeltaSimulationResult result,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interval),
                "Playback sample interval must be greater than zero.");
        }

        var frames = new List<DeltaPlaybackFrame>();
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
                var (actuators, toolPose) = SampleMovement(
                    result.InitialContext.RobotProfile,
                    current,
                    next,
                    time);
                frames.Add(new DeltaPlaybackFrame(
                    time,
                    current.State,
                    actuators,
                    toolPose,
                    current.CommandIndex,
                    current.CommandName,
                    current.CommandSource));
            }
        }

        AddFrameIfNeeded(frames, CreateFrame(result.InitialContext.RobotProfile, result.Timeline[^1]));

        return new DeltaPlaybackSnapshot(
            result.InitialContext.RobotProfile,
            frames.AsReadOnly(),
            result.FinalContext.ElapsedTime,
            result.Succeeded,
            result.Failure?.Message);
    }

    private (DeltaActuatorPosition Actuators, DeltaToolPose ToolPose) SampleMovement(
        DeltaRobotProfile profile,
        DeltaSimulationStep current,
        DeltaSimulationStep next,
        TimeSpan time)
    {
        if (current.CartesianMotionPlan is
            {
                ToolMotionProfile: not null
            } cartesianPlan)
        {
            var sample = cartesianPlan.ToolMotionProfile.SampleAt(time - current.Time);
            var toolPose = DeltaCartesianMotionPlanner.Interpolate(
                cartesianPlan.StartToolPose,
                cartesianPlan.EndToolPose,
                sample.Progress);
            return (kinematics.Inverse(profile, toolPose), toolPose);
        }

        var progress = MotionProfileTimelineSampler.CalculateProgress(
            current.MotionProfile,
            current.Time,
            next.Time,
            time);
        var actuators = Interpolate(current.Actuators, next.Actuators, progress);
        return (actuators, kinematics.Forward(profile, actuators));
    }

    private DeltaPlaybackFrame CreateFrame(
        DeltaRobotProfile profile,
        DeltaSimulationStep step) =>
        new(
            step.Time,
            step.State,
            step.Actuators,
            kinematics.Forward(profile, step.Actuators),
            step.CommandIndex,
            step.CommandName,
            step.CommandSource);

    private static void AddFrameIfNeeded(
        List<DeltaPlaybackFrame> frames,
        DeltaPlaybackFrame frame)
    {
        if (frames.Count == 0 || frames[^1].Time != frame.Time)
        {
            frames.Add(frame);
        }
    }

    private static DeltaActuatorPosition Interpolate(
        DeltaActuatorPosition start,
        DeltaActuatorPosition end,
        double progress)
    {
        var clampedProgress = Math.Clamp(progress, 0, 1);
        return new DeltaActuatorPosition(
            start.AMillimeters + ((end.AMillimeters - start.AMillimeters) * clampedProgress),
            start.BMillimeters + ((end.BMillimeters - start.BMillimeters) * clampedProgress),
            start.CMillimeters + ((end.CMillimeters - start.CMillimeters) * clampedProgress));
    }
}
