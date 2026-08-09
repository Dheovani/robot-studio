using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class PlaybackSnapshotValidatorTests
{
    [Fact]
    public void Validate_WhenSnapshotIsValid_ShouldReturnValidResult()
    {
        var validator = new PlaybackSnapshotValidator();

        var result = validator.Validate(CreateSnapshot());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenSnapshotIsNull_ShouldReturnError()
    {
        var validator = new PlaybackSnapshotValidator();

        var result = validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains("Snapshot is missing.", result.Errors);
    }

    [Fact]
    public void Validate_WhenMetadataIsUnsupported_ShouldReturnErrors()
    {
        var validator = new PlaybackSnapshotValidator();
        var snapshot = CreateSnapshot() with
        {
            Metadata = new PlaybackSnapshotMetadata(
                FormatVersion: 3,
                RobotFamily: "Drone",
                DistanceUnit: "Meters",
                TimeUnit: "Milliseconds",
                SampleIntervalMilliseconds: 0)
        };

        var result = validator.Validate(snapshot);

        Assert.False(result.IsValid);
        Assert.Contains("Unsupported snapshot format version: 3.", result.Errors);
        Assert.Contains("Unsupported robot family: Drone.", result.Errors);
        Assert.Contains("Unsupported distance unit: Meters.", result.Errors);
        Assert.Contains("Unsupported time unit: Milliseconds.", result.Errors);
        Assert.Contains("Snapshot sample interval must be greater than zero.", result.Errors);
    }

    [Fact]
    public void Validate_WhenRequiredSectionsAreMissing_ShouldReturnErrors()
    {
        var validator = new PlaybackSnapshotValidator();
        var snapshot = CreateSnapshot() with
        {
            WorkspaceBounds = null!,
            Viewport = null!,
            Frames = null!,
            Poses = null!,
            SceneFrames = null!
        };

        var result = validator.Validate(snapshot);

        Assert.False(result.IsValid);
        Assert.Contains("Snapshot workspace bounds are missing.", result.Errors);
        Assert.Contains("Snapshot viewport is missing.", result.Errors);
        Assert.Contains("Snapshot frames are missing.", result.Errors);
        Assert.Contains("Snapshot poses are missing.", result.Errors);
        Assert.Contains("Snapshot scene frames are missing.", result.Errors);
    }

    [Fact]
    public void Validate_WhenCountsDoNotMatch_ShouldReturnErrors()
    {
        var validator = new PlaybackSnapshotValidator();
        var snapshot = CreateSnapshot() with
        {
            Poses = Array.Empty<CartesianRobotPose>(),
            SceneFrames = Array.Empty<CartesianSceneFrame>()
        };

        var result = validator.Validate(snapshot);

        Assert.False(result.IsValid);
        Assert.Contains("Snapshot frame count must match pose count.", result.Errors);
        Assert.Contains("Snapshot frame count must match scene frame count.", result.Errors);
    }

    [Fact]
    public void Validate_WhenFramesAreEmpty_ShouldReturnError()
    {
        var validator = new PlaybackSnapshotValidator();
        var snapshot = CreateSnapshot() with
        {
            Frames = Array.Empty<RobotVisualState>(),
            Poses = Array.Empty<CartesianRobotPose>(),
            SceneFrames = Array.Empty<CartesianSceneFrame>()
        };

        var result = validator.Validate(snapshot);

        Assert.False(result.IsValid);
        Assert.Contains("Snapshot must contain at least one frame.", result.Errors);
    }

    [Fact]
    public void Validate_WhenTotalDurationIsNegative_ShouldReturnError()
    {
        var validator = new PlaybackSnapshotValidator();
        var snapshot = CreateSnapshot() with { TotalDuration = TimeSpan.FromSeconds(-1) };

        var result = validator.Validate(snapshot);

        Assert.False(result.IsValid);
        Assert.Contains("Snapshot total duration cannot be negative.", result.Errors);
    }

    [Fact]
    public void Validate_WhenSnapshotUsesVersionOne_ShouldRemainCompatible()
    {
        var validator = new PlaybackSnapshotValidator();
        var snapshot = CreateSnapshot() with
        {
            Metadata = CreateSnapshot().Metadata with { FormatVersion = 1 }
        };

        var result = validator.Validate(snapshot);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenVersionTwoContainsInvalidMotionMetrics_ShouldReturnErrors()
    {
        var validator = new PlaybackSnapshotValidator();
        var snapshot = CreateSnapshot();
        var invalidFrame = snapshot.Frames[0] with
        {
            VelocityMillimetersPerSecond = -1,
            AccelerationMillimetersPerSecondSquared = double.NaN
        };

        var result = validator.Validate(snapshot with { Frames = [invalidFrame] });

        Assert.False(result.IsValid);
        Assert.Contains("Snapshot frame velocities must be finite and non-negative.", result.Errors);
        Assert.Contains("Snapshot frame accelerations must be finite.", result.Errors);
    }

    private static CartesianPlaybackSnapshot CreateSnapshot()
    {
        var metadata = PlaybackSnapshotMetadata.CreateCartesian(TimeSpan.FromMilliseconds(500));
        var bounds = new CartesianWorkspaceBounds(
            new VisualVector3(0, 0, 0),
            new VisualVector3(300, 200, 150));
        var viewport = new CartesianViewportPlanner().Plan(bounds);
        var frame = new RobotVisualState(
            TimeSpan.Zero,
            RobotState.Moving,
            new VisualVector3(120, 80, 40),
            CommandIndex: 0,
            CommandName: nameof(MoveToCommand),
            CommandSource: null);
        var pose = new CartesianRobotPoseMapper().Map(frame);
        var sceneFrame = new CartesianSceneFrameMapper().Map(bounds, pose);

        return new CartesianPlaybackSnapshot(
            metadata,
            bounds,
            viewport,
            [frame],
            [pose],
            [sceneFrame],
            TotalDuration: TimeSpan.Zero,
            Succeeded: true,
            FailureMessage: null);
    }
}
