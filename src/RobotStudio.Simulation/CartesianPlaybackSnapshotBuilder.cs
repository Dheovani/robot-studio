using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed class CartesianPlaybackSnapshotBuilder
{
    private readonly CartesianPlaybackSampler playbackSampler;
    private readonly CartesianRobotPoseMapper poseMapper;
    private readonly CartesianSceneFrameMapper sceneFrameMapper;
    private readonly CartesianViewportPlanner viewportPlanner;

    public CartesianPlaybackSnapshotBuilder()
        : this(
            new CartesianPlaybackSampler(),
            new CartesianRobotPoseMapper(),
            new CartesianSceneFrameMapper(),
            new CartesianViewportPlanner())
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
        : this(playbackSampler, poseMapper, sceneFrameMapper, new CartesianViewportPlanner())
    {
    }

    public CartesianPlaybackSnapshotBuilder(
        CartesianPlaybackSampler playbackSampler,
        CartesianRobotPoseMapper poseMapper,
        CartesianSceneFrameMapper sceneFrameMapper,
        CartesianViewportPlanner viewportPlanner)
    {
        ArgumentNullException.ThrowIfNull(playbackSampler);
        ArgumentNullException.ThrowIfNull(poseMapper);
        ArgumentNullException.ThrowIfNull(sceneFrameMapper);
        ArgumentNullException.ThrowIfNull(viewportPlanner);

        this.playbackSampler = playbackSampler;
        this.poseMapper = poseMapper;
        this.sceneFrameMapper = sceneFrameMapper;
        this.viewportPlanner = viewportPlanner;
    }

    public CartesianPlaybackSnapshot Build(
        CartesianRobotProfile profile,
        SimulationResult result,
        TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(result);

        var metadata = PlaybackSnapshotMetadata.CreateCartesian(interval);
        var workspaceBounds = CartesianWorkspaceBounds.FromProfile(profile);
        var viewport = viewportPlanner.Plan(workspaceBounds);
        var frames = playbackSampler.Sample(result, interval);
        var poses = frames.Select(poseMapper.Map).ToArray();
        var sceneFrames = poses.Select(pose => sceneFrameMapper.Map(workspaceBounds, pose)).ToArray();
        var commandMotions = CreateCommandMotionSummaries(result);

        return new CartesianPlaybackSnapshot(
            metadata,
            workspaceBounds,
            viewport,
            frames,
            poses,
            sceneFrames,
            result.FinalContext.ElapsedTime,
            result.Succeeded,
            result.Failure?.Message,
            commandMotions);
    }

    private static IReadOnlyList<CartesianCommandMotionSummary> CreateCommandMotionSummaries(
        SimulationResult result) =>
        result.Timeline
            .Where(step =>
                step.CommandIndex.HasValue &&
                step.CommandName is nameof(MoveToCommand) or nameof(HomeCommand))
            .GroupBy(step => step.CommandIndex!.Value)
            .Where(group => group.Count() >= 2)
            .OrderBy(group => group.Key)
            .Select(CreateCommandMotionSummary)
            .ToArray();

    private static CartesianCommandMotionSummary CreateCommandMotionSummary(
        IGrouping<int, SimulationStep> commandSteps)
    {
        var first = commandSteps.First();
        var last = commandSteps.Last();
        var motionProfile = commandSteps.Select(step => step.MotionProfile).FirstOrDefault(profile => profile is not null);
        var involvedAxes = GetInvolvedAxes(first.Position, last.Position);

        return new CartesianCommandMotionSummary(
            commandSteps.Key,
            first.CommandName!,
            first.Position,
            last.Position,
            involvedAxes,
            motionProfile?.Distance ?? 0,
            motionProfile?.MaximumVelocity ?? 0,
            motionProfile?.PeakVelocity ?? 0,
            motionProfile?.Acceleration ?? 0,
            motionProfile is null
                ? MotionProfileShape.Stationary
                : motionProfile.IsTriangular
                    ? MotionProfileShape.Triangular
                    : MotionProfileShape.Trapezoidal,
            motionProfile?.AccelerationDuration ?? TimeSpan.Zero,
            motionProfile?.ConstantVelocityDuration ?? TimeSpan.Zero,
            motionProfile?.DecelerationDuration ?? TimeSpan.Zero,
            motionProfile?.TotalDuration ?? TimeSpan.Zero,
            first.RequestedVelocityMillimetersPerSecond);
    }

    private static IReadOnlyList<AxisId> GetInvolvedAxes(
        CartesianPosition start,
        CartesianPosition end)
    {
        const double tolerance = 0.000_001;
        var axes = new List<AxisId>(capacity: 3);

        if (Math.Abs(end.X - start.X) > tolerance)
        {
            axes.Add(AxisId.X);
        }

        if (Math.Abs(end.Y - start.Y) > tolerance)
        {
            axes.Add(AxisId.Y);
        }

        if (Math.Abs(end.Z - start.Z) > tolerance)
        {
            axes.Add(AxisId.Z);
        }

        return axes;
    }
}
