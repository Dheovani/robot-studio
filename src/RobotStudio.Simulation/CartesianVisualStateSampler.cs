namespace RobotStudio.Simulation;

public sealed class CartesianVisualStateSampler
{
    private readonly SimulationTimelineSampler timelineSampler;
    private readonly CartesianVisualStateMapper visualStateMapper;

    public CartesianVisualStateSampler()
        : this(new SimulationTimelineSampler(), new CartesianVisualStateMapper())
    {
    }

    public CartesianVisualStateSampler(
        SimulationTimelineSampler timelineSampler,
        CartesianVisualStateMapper visualStateMapper)
    {
        ArgumentNullException.ThrowIfNull(timelineSampler);
        ArgumentNullException.ThrowIfNull(visualStateMapper);

        this.timelineSampler = timelineSampler;
        this.visualStateMapper = visualStateMapper;
    }

    public RobotVisualState SampleAt(
        SimulationResult result,
        TimeSpan time)
    {
        var sample = timelineSampler.SampleAt(result, time);

        return visualStateMapper.Map(sample);
    }
}
