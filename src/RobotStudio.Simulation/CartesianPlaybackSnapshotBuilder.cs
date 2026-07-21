using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Simulation;

public sealed class CartesianPlaybackSnapshotBuilder
{
    private readonly CartesianPlaybackSampler playbackSampler;
    private readonly CartesianRobotPoseMapper poseMapper;
    private readonly CartesianSceneFrameMapper sceneFrameMapper;

    public CartesianPlaybackSnapshotBuilder()
        : this(new CartesianPlaybackSampler(), new CartesianRobotPoseMapper(), new CartesianSceneFrameMapper())
    {
    }

    public CartesianPlaybackSnapshotBuilder(CartesianPlaybackSampler playbackSampler)
        : this(playbackSampler, new CartesianRobotPoseMapper())
    {
    }

    public CartesianPlaybackSnapshotBuilder(
        CartesianPlaybackSampler playbackSampler,
        CartesianRobotPoseMapper poseMapper)
        : this(playbackSampler, poseMapper, new CartesianSceneFrameMapper())
    {
    }

    public CartesianPlaybackSnapshotBuilder(
        CartesianPlaybackSampler playbackSampler,
        CartesianRobotPoseMapper poseMapper,
        CartesianSceneFrameMapper sceneFrameMapper)
    {
        ArgumentNullException.ThrowIfNull(playbackSampler);
        ArgumentNullException.ThrowIfNull(poseMapper);
        ArgumentNullException.ThrowIfNull(sceneFrameMapper);

        this.playbackSampler = playbackSampler;
        this.poseMapper = poseMapper;
        this.sceneFrameMapper = sceneFrameMapper;
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
        var sceneFrames = poses.Select(pose => sceneFrameMapper.Map(workspaceBounds, pose)).ToArray();

        return new CartesianPlaybackSnapshot(
            workspaceBounds,
            frames,
            poses,
            sceneFrames,
            result.FinalContext.ElapsedTime,
            result.Succeeded,
            result.Failure?.Message);
    }
}
