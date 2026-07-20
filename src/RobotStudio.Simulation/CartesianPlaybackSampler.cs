namespace RobotStudio.Simulation;

public sealed class CartesianPlaybackSampler
{
    private readonly CartesianVisualStateSampler visualStateSampler;

    public CartesianPlaybackSampler()
        : this(new CartesianVisualStateSampler())
    {
    }

    public CartesianPlaybackSampler(CartesianVisualStateSampler visualStateSampler)
    {
        ArgumentNullException.ThrowIfNull(visualStateSampler);

        this.visualStateSampler = visualStateSampler;
    }

    public IReadOnlyList<RobotVisualState> Sample(
        SimulationResult result,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interval),
                "Playback interval must be greater than zero.");
        }

        TimeSpan finalTime = result.FinalContext.ElapsedTime;
        if (finalTime == TimeSpan.Zero)
        {
            return new[] { visualStateSampler.SampleAt(result, TimeSpan.Zero) };
        }

        var frames = new List<RobotVisualState>();

        for (TimeSpan time = TimeSpan.Zero; time < finalTime; time += interval)
        {
            frames.Add(visualStateSampler.SampleAt(result, time));
        }

        frames.Add(visualStateSampler.SampleAt(result, finalTime));

        return frames.AsReadOnly();
    }
}
