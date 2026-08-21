using RobotStudio.Domain;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Motion;

namespace RobotStudio.Simulation;

public sealed class SimulationTimelineSampler
{
    public SimulationSample SampleAt(
        SimulationResult result,
        TimeSpan time)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Timeline.Count == 0)
        {
            throw new ArgumentException("Simulation result must contain at least one timeline step.", nameof(result));
        }

        if (time < result.Timeline[0].Time)
        {
            return CreateSample(time, result.Timeline[0]);
        }

        var nextStepIndex = FindFirstStepAfter(result.Timeline, time);

        if (nextStepIndex is null)
        {
            return CreateSample(time, result.Timeline[^1]);
        }

        var previousStep = result.Timeline[nextStepIndex.Value - 1];
        var nextStep = result.Timeline[nextStepIndex.Value];

        if (!CanInterpolate(previousStep, nextStep))
        {
            return CreateSample(time, previousStep);
        }

        var profileSample = SampleMotionProfile(previousStep, time);
        var progress = profileSample?.Progress ?? CalculateLinearProgress(previousStep, nextStep, time);
        var position = Interpolate(previousStep.Position, nextStep.Position, progress);

        return new SimulationSample(
            time,
            previousStep.State,
            position,
            previousStep.CommandIndex,
            previousStep.CommandName,
            previousStep.CommandSource,
            profileSample?.Velocity ?? 0,
            profileSample?.Acceleration ?? 0,
            profileSample?.Phase,
            previousStep.RequestedVelocityMillimetersPerSecond,
            previousStep.RequestedWaitDuration);
    }

    private static int? FindFirstStepAfter(
        IReadOnlyList<SimulationStep> timeline,
        TimeSpan time)
    {
        for (var index = 0; index < timeline.Count; index++)
        {
            if (timeline[index].Time > time)
            {
                return index;
            }
        }

        return null;
    }

    private static bool CanInterpolate(
        SimulationStep previousStep,
        SimulationStep nextStep)
    {
        if (previousStep.Time == nextStep.Time)
        {
            return false;
        }

        return previousStep.CommandIndex == nextStep.CommandIndex &&
            previousStep.CommandName == nextStep.CommandName &&
            RobotStateTransitions.IsActive(previousStep.State);
    }

    private static MotionProfileSample? SampleMotionProfile(
        SimulationStep step,
        TimeSpan current) =>
        step.MotionProfile?.SampleAt(current - step.Time);

    private static double CalculateLinearProgress(
        SimulationStep previousStep,
        SimulationStep nextStep,
        TimeSpan current) =>
        (current - previousStep.Time).TotalSeconds /
        (nextStep.Time - previousStep.Time).TotalSeconds;

    private static CartesianPosition Interpolate(
        CartesianPosition start,
        CartesianPosition end,
        double progress) =>
        new(
            X: Interpolate(start.X, end.X, progress),
            Y: Interpolate(start.Y, end.Y, progress),
            Z: Interpolate(start.Z, end.Z, progress));

    private static double Interpolate(
        double start,
        double end,
        double progress) =>
        start + ((end - start) * progress);

    private static SimulationSample CreateSample(
        TimeSpan time,
        SimulationStep step)
    {
        var profileSample = step.MotionProfile?.SampleAt(
            RobotStateTransitions.IsActive(step.State)
                ? TimeSpan.Zero
                : step.MotionProfile.TotalDuration);

        return new(
            time,
            step.State,
            step.Position,
            step.CommandIndex,
            step.CommandName,
            step.CommandSource,
            profileSample?.Velocity ?? 0,
            profileSample?.Acceleration ?? 0,
            profileSample?.Phase,
            step.RequestedVelocityMillimetersPerSecond,
            step.RequestedWaitDuration);
    }
}
