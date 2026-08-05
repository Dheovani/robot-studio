using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Simulation.Tests;

public sealed class RobotPlaybackContractTests
{
    [Fact]
    public void PlaybackSnapshots_ShouldExposeCommonSnapshotContract()
    {
        IRobotPlaybackSnapshot[] snapshots =
        [
            CreateCartesianSnapshot(),
            CreateDifferentialDriveSnapshot(),
            CreateScaraSnapshot(),
            CreateSimpleArmSnapshot(),
            CreateDeltaSnapshot()
        ];

        Assert.All(
            snapshots,
            snapshot =>
            {
                Assert.True(snapshot.FrameCount > 0);
                Assert.True(snapshot.TotalDuration >= TimeSpan.Zero);
                Assert.Equal(TimeSpan.Zero, snapshot.FirstFrame.Time);
                Assert.Equal(snapshot.TotalDuration, snapshot.LastFrame.Time);
            });
    }

    [Fact]
    public void PlaybackFrames_ShouldExposeCommonTimelineMetadata()
    {
        IRobotPlaybackFrame[] frames =
        [
            CreateCartesianSnapshot().LastFrame,
            CreateDifferentialDriveSnapshot().LastFrame,
            CreateScaraSnapshot().LastFrame,
            CreateSimpleArmSnapshot().LastFrame,
            CreateDeltaSnapshot().LastFrame
        ];

        Assert.All(
            frames,
            frame =>
            {
                Assert.Equal(RobotState.Completed, frame.State);
                Assert.True(frame.Time >= TimeSpan.Zero);
                Assert.NotNull(frame.CommandName);
            });
    }

    [Fact]
    public void RobotPlaybackSummary_ShouldSummarizeAnySnapshotFamily()
    {
        var summary = RobotPlaybackSummary.Create(CreateScaraSnapshot());

        Assert.True(summary.FrameCount > 0);
        Assert.True(summary.Succeeded);
        Assert.Null(summary.FailureMessage);
        Assert.Equal(TimeSpan.Zero, summary.FirstFrameTime);
        Assert.Equal(summary.TotalDuration, summary.LastFrameTime);
        Assert.Equal(RobotState.Idle, summary.FirstState);
        Assert.Equal(RobotState.Completed, summary.LastState);
        Assert.Equal(nameof(ScaraMoveJointsCommand), summary.LastCommandName);
    }

    [Fact]
    public void RobotPlaybackSummary_WhenSnapshotIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RobotPlaybackSummary.Create(null!));
    }

    private static CartesianPlaybackSnapshot CreateCartesianSnapshot()
    {
        var profile = new CartesianRobotProfile(
            new Axis(AxisId.X, 0, 300, 120, 500),
            new Axis(AxisId.Y, 0, 200, 100, 500),
            new Axis(AxisId.Z, 0, 120, 80, 400));
        var context = SimulationContext.Create(profile, new CartesianPosition(0, 0, 0));
        var sequence = new RobotCommandSequence(
        [
            new MoveToCommand(
                new CartesianPosition(100, 50, 20),
                requestedVelocityMillimetersPerSecond: 80)
        ]);
        var result = new RobotSimulator().Execute(context, sequence);

        return new CartesianPlaybackSnapshotBuilder()
            .Build(profile, result, TimeSpan.FromMilliseconds(100));
    }

    private static DifferentialDrivePlaybackSnapshot CreateDifferentialDriveSnapshot()
    {
        var profile = new DifferentialDriveProfile(
            minimumXMillimeters: 0,
            maximumXMillimeters: 500,
            minimumYMillimeters: 0,
            maximumYMillimeters: 400,
            wheelBaseMillimeters: 120,
            wheelRadiusMillimeters: 30,
            maximumLinearVelocityMillimetersPerSecond: 250,
            maximumAngularVelocityDegreesPerSecond: 180);
        var context = DifferentialDriveSimulationContext.Create(
            profile,
            new DifferentialDrivePose(0, 0, 0));
        var sequence = new RobotCommandSequence(
        [
            new DifferentialDriveMoveCommand(
                new DifferentialDrivePose(120, 80, 45),
                requestedLinearVelocityMillimetersPerSecond: 100,
                requestedAngularVelocityDegreesPerSecond: 90)
        ]);
        var result = new DifferentialDriveSimulator().Execute(context, sequence);

        return new DifferentialDrivePlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private static ScaraPlaybackSnapshot CreateScaraSnapshot()
    {
        var profile = new ScaraRobotProfile(
            firstLinkLengthMillimeters: 180,
            secondLinkLengthMillimeters: 120,
            shoulderJoint: new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120),
            elbowJoint: new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100));
        var context = ScaraSimulationContext.Create(
            profile,
            new ScaraJointPosition(0, 0));
        var sequence = new RobotCommandSequence(
        [
            new ScaraMoveJointsCommand(
                new ScaraJointPosition(45, 30),
                requestedJointVelocityDegreesPerSecond: 80)
        ]);
        var result = new ScaraSimulator().Execute(context, sequence);

        return new ScaraPlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private static SimpleArmPlaybackSnapshot CreateSimpleArmSnapshot()
    {
        var profile = new SimpleArmRobotProfile(
            firstLinkLengthMillimeters: 120,
            secondLinkLengthMillimeters: 90,
            thirdLinkLengthMillimeters: 60,
            baseJoint: new SimpleArmJoint(SimpleArmJointId.Base, -180, 180, 100),
            shoulderJoint: new SimpleArmJoint(SimpleArmJointId.Shoulder, -120, 120, 90),
            elbowJoint: new SimpleArmJoint(SimpleArmJointId.Elbow, -150, 150, 80));
        var context = SimpleArmSimulationContext.Create(
            profile,
            new SimpleArmJointPosition(0, 0, 0));
        var sequence = new RobotCommandSequence(
        [
            new SimpleArmMoveJointsCommand(
                new SimpleArmJointPosition(60, 30, -20),
                requestedJointVelocityDegreesPerSecond: 80)
        ]);
        var result = new SimpleArmSimulator().Execute(context, sequence);

        return new SimpleArmPlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }

    private static DeltaPlaybackSnapshot CreateDeltaSnapshot()
    {
        var profile = new DeltaRobotProfile(
            baseRadiusMillimeters: 140,
            toolZOffsetMillimeters: 0,
            actuatorA: new DeltaActuator(DeltaActuatorId.A, 0, 180, 120),
            actuatorB: new DeltaActuator(DeltaActuatorId.B, 0, 180, 100),
            actuatorC: new DeltaActuator(DeltaActuatorId.C, 0, 180, 90));
        var context = DeltaSimulationContext.Create(
            profile,
            new DeltaActuatorPosition(0, 0, 0));
        var sequence = new RobotCommandSequence(
        [
            new DeltaMoveActuatorsCommand(
                new DeltaActuatorPosition(30, 60, 90),
                requestedActuatorVelocityMillimetersPerSecond: 80)
        ]);
        var result = new DeltaSimulator().Execute(context, sequence);

        return new DeltaPlaybackSampler()
            .Sample(result, TimeSpan.FromMilliseconds(100));
    }
}
