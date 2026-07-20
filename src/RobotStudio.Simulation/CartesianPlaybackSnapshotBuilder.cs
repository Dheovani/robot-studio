using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Simulation;

public sealed class CartesianPlaybackSnapshotBuilder
{
    private readonly CartesianPlaybackSampler playbackSampler;

    public CartesianPlaybackSnapshotBuilder()
        : this(new CartesianPlaybackSampler())
    {
    }

    public CartesianPlaybackSnapshotBuilder(CartesianPlaybackSampler playbackSampler)
    {
        ArgumentNullException.ThrowIfNull(playbackSampler);

        this.playbackSampler = playbackSampler;
    }

    public CartesianPlaybackSnapshot Build(
        CartesianRobotProfile profile,
        SimulationResult result,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(result);

        var workspaceBounds = CartesianWorkspaceBounds.FromProfile(profile);
        var frames = playbackSampler.Sample(result, interval);

        return new CartesianPlaybackSnapshot(
            workspaceBounds,
            frames,
            result.FinalContext.ElapsedTime,
            result.Succeeded,
            result.Failure?.Message);
    }
}
