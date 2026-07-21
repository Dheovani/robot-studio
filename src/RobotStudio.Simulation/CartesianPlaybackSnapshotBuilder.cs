using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Simulation;

public sealed class CartesianPlaybackSnapshotBuilder
{
    private readonly CartesianPlaybackSampler playbackSampler;
    private readonly CartesianRobotPoseMapper poseMapper;

    public CartesianPlaybackSnapshotBuilder()
        : this(new CartesianPlaybackSampler(), new CartesianRobotPoseMapper())
    {
    }

    public CartesianPlaybackSnapshotBuilder(CartesianPlaybackSampler playbackSampler)
        : this(playbackSampler, new CartesianRobotPoseMapper())
    {
    }

    public CartesianPlaybackSnapshotBuilder(
        CartesianPlaybackSampler playbackSampler,
        CartesianRobotPoseMapper poseMapper)
    {
        ArgumentNullException.ThrowIfNull(playbackSampler);
        ArgumentNullException.ThrowIfNull(poseMapper);

        this.playbackSampler = playbackSampler;
        this.poseMapper = poseMapper;
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
        var poses = frames.Select(poseMapper.Map).ToArray();

        return new CartesianPlaybackSnapshot(
            workspaceBounds,
            frames,
            poses,
            result.FinalContext.ElapsedTime,
            result.Succeeded,
            result.Failure?.Message);
    }
}
