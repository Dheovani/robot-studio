using RobotStudio.Domain;

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

        var progress = CalculateProgress(previousStep.Time, nextStep.Time, time);
        var position = Interpolate(previousStep.Position, nextStep.Position, progress);

        return new SimulationSample(
            time,
            previousStep.State,
            position,
            previousStep.CommandIndex,
            previousStep.CommandName);
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

    private static double CalculateProgress(
        TimeSpan start,
        TimeSpan end,
        TimeSpan current) =>
        (current - start).TotalSeconds / (end - start).TotalSeconds;

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
        SimulationStep step) =>
        new(
            time,
            step.State,
            step.Position,
            step.CommandIndex,
            step.CommandName);
}
