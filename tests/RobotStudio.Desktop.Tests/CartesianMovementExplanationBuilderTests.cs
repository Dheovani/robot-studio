using RobotStudio.Desktop.Didactics;
using RobotStudio.Domain;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Motion;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop.Tests;

public sealed class CartesianMovementExplanationBuilderTests
{
    private readonly CartesianMovementExplanationBuilder builder = new();

    [Fact]
    public void Create_WhenMoveIsLongEnough_ShouldExplainTrapezoidalProfileAndLimits()
    {
        var profile = CreateProfile();
        var snapshot = CreateSnapshot(
            profile,
            new MoveToCommand(
                new CartesianPosition(100, 0, 0),
                requestedVelocityMillimetersPerSecond: 50,
                source: new RobotCommandSource(1, "MOVE X=100 Y=0 Z=0 SPEED=50")));
        var cruiseFrameIndex = FindFrame(snapshot, MotionProfilePhase.ConstantVelocity);

        var explanation = builder.Create(profile, snapshot, cruiseFrameIndex);

        Assert.Contains("Profile: trapezoidal", explanation);
        Assert.Contains("capped at 10 mm/s by the X axis", explanation);
        Assert.Contains("Acceleration is limited to 20 mm/s^2 by the X axis", explanation);
        Assert.Contains("Current phase: constant-velocity cruise", explanation);
    }

    [Fact]
    public void Create_WhenMoveIsShort_ShouldExplainTriangularProfile()
    {
        var profile = CreateProfile();
        var snapshot = CreateSnapshot(
            profile,
            new MoveToCommand(
                new CartesianPosition(2, 0, 0),
                requestedVelocityMillimetersPerSecond: 10,
                source: new RobotCommandSource(1, "MOVE X=2 Y=0 Z=0 SPEED=10")),
            interval: TimeSpan.FromMilliseconds(10));

        var explanation = builder.Create(profile, snapshot, frameIndex: 0);

        Assert.Contains("Profile: triangular", explanation);
        Assert.Contains("too short to reach the velocity limit", explanation);
        Assert.Contains("cruise 0 s", explanation);
    }

    [Fact]
    public void Create_WhenMoveIsStationary_ShouldExplainZeroDistanceBehavior()
    {
        var profile = CreateProfile();
        var snapshot = CreateSnapshot(
            profile,
            new MoveToCommand(
                new CartesianPosition(0, 0, 0),
                source: new RobotCommandSource(1, "MOVE X=0 Y=0 Z=0")));

        var explanation = builder.Create(profile, snapshot, frameIndex: 0);

        Assert.Contains("target equals the command start position", explanation);
        Assert.Contains("no movement phases or simulated time", explanation);
    }

    [Fact]
    public void Create_WhenCommandIsWait_ShouldExplainTimeWithoutMovement()
    {
        var profile = CreateProfile();
        var snapshot = CreateSnapshot(
            profile,
            new WaitCommand(
                TimeSpan.FromMilliseconds(500),
                new RobotCommandSource(1, "WAIT 500")));

        var explanation = builder.Create(profile, snapshot, frameIndex: 0);

        Assert.Contains("keeps the current position fixed for 500 ms", explanation);
        Assert.Contains("Only simulated time advances", explanation);
    }

    [Fact]
    public void Create_WhenSnapshotIsLegacy_ShouldExplainMissingProfileDetails()
    {
        var profile = CreateProfile();
        var snapshot = CreateSnapshot(
            profile,
            new MoveToCommand(
                new CartesianPosition(20, 0, 0),
                source: new RobotCommandSource(1, "MOVE X=20 Y=0 Z=0"))) with
        {
            Metadata = PlaybackSnapshotMetadata.CreateCartesian(TimeSpan.FromMilliseconds(50)) with
            {
                FormatVersion = 3
            },
            CommandMotions = null
        };

        var explanation = builder.Create(profile, snapshot, frameIndex: 0);

        Assert.Contains("legacy playback snapshot", explanation);
    }

    private static CartesianPlaybackSnapshot CreateSnapshot(
        CartesianRobotProfile profile,
        RobotCommand command,
        TimeSpan? interval = null)
    {
        var context = SimulationContext.Create(profile, new CartesianPosition(0, 0, 0));
        var result = new RobotSimulator().Execute(context, new RobotCommandSequence([command]));

        return new CartesianPlaybackSnapshotBuilder().Build(
            profile,
            result,
            interval ?? TimeSpan.FromMilliseconds(50));
    }

    private static int FindFrame(
        CartesianPlaybackSnapshot snapshot,
        MotionProfilePhase phase) =>
        snapshot.Frames
            .Select((frame, index) => (frame, index))
            .First(item => item.frame.MotionProfilePhase == phase)
            .index;

    private static CartesianRobotProfile CreateProfile() =>
        CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 200, 10, 20),
            new Axis(AxisId.Y, 0, 200, 20, 40),
            new Axis(AxisId.Z, 0, 200, 30, 60));
}
