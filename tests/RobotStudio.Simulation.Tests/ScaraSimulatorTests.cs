using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Simulation.Tests;

public sealed class ScaraSimulatorTests
{
    [Fact]
    public void Execute_WhenCommandIsLinearToolMove_ShouldCreateOneContinuousTimelineInterval()
    {
        var profile = CreateProfile();
        var target = new ScaraToolPose(X: 220, Y: 80);
        var source = new RobotCommandSource(2, "G1 X220 Y80 F4800");
        var result = new ScaraSimulator().Execute(
            ScaraSimulationContext.Create(profile, new ScaraJointPosition(0, 0)),
            new RobotCommandSequence(
            [
                new ScaraLinearMoveCommand(
                    target,
                    requestedToolVelocityMillimetersPerSecond: 80,
                    source)
            ]));

        Assert.True(result.Succeeded);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        var finalPose = new ScaraKinematics().Forward(
            profile,
            result.FinalContext.CurrentJoints);
        Assert.Equal(target.X, finalPose.X, precision: 6);
        Assert.Equal(target.Y, finalPose.Y, precision: 6);
        Assert.Equal(3, result.Timeline.Count);
        Assert.NotNull(result.Timeline[1].CartesianMotionPlan);
        Assert.NotNull(result.Timeline[1].MotionProfile);
        Assert.All(
            result.Timeline.Where(step => step.CommandIndex == 0),
            step => Assert.Equal(source, step.CommandSource));
    }

    [Fact]
    public void Playback_WhenCommandIsLinearToolMove_ShouldKeepTcpOnRequestedLine()
    {
        var profile = CreateProfile();
        var start = new ScaraToolPose(300, 0);
        var target = new ScaraToolPose(220, 80);
        var result = new ScaraSimulator().Execute(
            ScaraSimulationContext.Create(profile, new ScaraJointPosition(0, 0)),
            new RobotCommandSequence([new ScaraLinearMoveCommand(target, 80)]));

        var playback = new ScaraPlaybackSampler().Sample(
            result,
            TimeSpan.FromMilliseconds(10));

        Assert.All(
            playback.Frames.Where(frame => frame.State == RobotState.Moving),
            frame => Assert.InRange(
                CrossProduct(start, target, frame.ToolPose),
                -0.000_001,
                0.000_001));
    }

    [Fact]
    public void Execute_WhenLinkPathCrossesObstacle_ShouldFaultWithoutMoving()
    {
        var environment = new PlanarSimulationEnvironment(
            [new PlanarObstacle("teaching-fixture", 80, 100, 20, 30)]);
        var start = new ScaraJointPosition(0, 0);
        var result = new ScaraSimulator(environment).Execute(
            ScaraSimulationContext.Create(CreateProfile(), start),
            new RobotCommandSequence(
                [new ScaraMoveJointsCommand(new ScaraJointPosition(30, 0))]));

        Assert.False(result.Succeeded);
        Assert.Equal(RobotState.Faulted, result.FinalContext.State);
        Assert.Equal(start, result.FinalContext.CurrentJoints);
        Assert.Equal(TimeSpan.Zero, result.FinalContext.ElapsedTime);
        var exception = Assert.IsType<ScaraPathObstructedException>(result.Failure);
        Assert.Equal("teaching-fixture", exception.ObstacleId);
        Assert.Equal(ScaraLinkId.FirstLink, exception.Link);
        Assert.InRange(exception.TrajectoryFraction, 0.01, 0.99);
    }

    [Fact]
    public void Execute_WhenHomePathCrossesObstacle_ShouldFaultAtCurrentJoints()
    {
        var environment = new PlanarSimulationEnvironment(
            [new PlanarObstacle("home-fixture", 80, 100, 20, 30)]);
        var start = new ScaraJointPosition(30, 0);
        var result = new ScaraSimulator(environment).Execute(
            ScaraSimulationContext.Create(CreateProfile(), start),
            new RobotCommandSequence([new HomeCommand()]));

        Assert.False(result.Succeeded);
        Assert.Equal(start, result.FinalContext.CurrentJoints);
        Assert.IsType<ScaraPathObstructedException>(result.Failure);
    }

    [Fact]
    public void Execute_WhenLinkPathAvoidsObstacle_ShouldCompleteNormally()
    {
        var target = new ScaraJointPosition(30, 20);
        var result = new ScaraSimulator(
            new PlanarSimulationEnvironment([new PlanarObstacle("clear", -300, -250, -300, -250)]))
            .Execute(
                ScaraSimulationContext.Create(CreateProfile(), new ScaraJointPosition(0, 0)),
                new RobotCommandSequence([new ScaraMoveJointsCommand(target)]));

        Assert.True(result.Succeeded);
        Assert.Equal(target, result.FinalContext.CurrentJoints);
    }

    [Fact]
    public void Execute_WhenResettingFault_ShouldPreserveJointsAndElapsedTime()
    {
        var context = ScaraSimulationContext.Create(CreateProfile(), new ScaraJointPosition(45, 30)) with
        {
            State = RobotState.Faulted,
            ElapsedTime = TimeSpan.FromSeconds(2)
        };
        var result = new ScaraSimulator().Execute(context, new RobotCommandSequence([new ResetFaultCommand()]));

        Assert.True(result.Succeeded);
        Assert.Equal(RobotState.Idle, result.FinalContext.State);
        Assert.Equal(context.CurrentJoints, result.FinalContext.CurrentJoints);
        Assert.Equal(context.ElapsedTime, result.FinalContext.ElapsedTime);
    }

    [Fact]
    public void Execute_WhenCommandIsHome_ShouldMoveToZeroJoints()
    {
        var simulator = new ScaraSimulator();
        var context = ScaraSimulationContext.Create(
            CreateProfile(),
            new ScaraJointPosition(ShoulderDegrees: 45, ElbowDegrees: 30));
        var sequence = new RobotCommandSequence([new HomeCommand()]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0), result.FinalContext.CurrentJoints);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.True(result.FinalContext.ElapsedTime > TimeSpan.Zero);
    }

    [Fact]
    public void Execute_WhenCommandIsScaraMoveJoints_ShouldMoveToTargetJoints()
    {
        var simulator = new ScaraSimulator();
        var context = ScaraSimulationContext.Create(
            CreateProfile(),
            new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0));
        var target = new ScaraJointPosition(ShoulderDegrees: 45, ElbowDegrees: 30);
        var sequence = new RobotCommandSequence([new ScaraMoveJointsCommand(target)]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(target, result.FinalContext.CurrentJoints);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.True(result.FinalContext.ElapsedTime > TimeSpan.Zero);
        Assert.NotEqual(default, result.Timeline[^1].ToolPose);
    }

    [Fact]
    public void Execute_WhenCommandIsWait_ShouldAdvanceTimeWithoutMoving()
    {
        var simulator = new ScaraSimulator();
        var joints = new ScaraJointPosition(ShoulderDegrees: 10, ElbowDegrees: 20);
        var context = ScaraSimulationContext.Create(CreateProfile(), joints);
        var sequence = new RobotCommandSequence([new WaitCommand(TimeSpan.FromMilliseconds(500))]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(joints, result.FinalContext.CurrentJoints);
        Assert.Equal(TimeSpan.FromMilliseconds(500), result.FinalContext.ElapsedTime);
    }

    [Fact]
    public void Execute_WhenSequenceHasMultipleCommands_ShouldRecordStateTransitionsInOrder()
    {
        var simulator = new ScaraSimulator();
        var context = ScaraSimulationContext.Create(
            CreateProfile(),
            new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0));
        var sequence = new RobotCommandSequence(
        [
            new HomeCommand(),
            new ScaraMoveJointsCommand(new ScaraJointPosition(ShoulderDegrees: 45, ElbowDegrees: 30)),
            new WaitCommand(TimeSpan.FromMilliseconds(500))
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.Equal(
            [
                RobotState.Idle,
                RobotState.Homing,
                RobotState.Completed,
                RobotState.Moving,
                RobotState.Completed,
                RobotState.Waiting,
                RobotState.Completed
            ],
            result.Timeline.Select(step => step.State));
    }

    [Fact]
    public void Execute_WhenCommandFails_ShouldReturnFaultedResultAndPreserveLastValidJoints()
    {
        var simulator = new ScaraSimulator();
        var context = ScaraSimulationContext.Create(
            CreateProfile(),
            new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0));
        var lastValidJoints = new ScaraJointPosition(ShoulderDegrees: 45, ElbowDegrees: 30);
        var sequence = new RobotCommandSequence(
        [
            new ScaraMoveJointsCommand(lastValidJoints),
            new ScaraMoveJointsCommand(new ScaraJointPosition(ShoulderDegrees: 181, ElbowDegrees: 0))
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.False(result.Succeeded);
        Assert.Equal(RobotState.Faulted, result.FinalContext.State);
        Assert.Equal(lastValidJoints, result.FinalContext.CurrentJoints);
        Assert.IsType<InvalidRobotCommandException>(result.Failure);
    }

    private static ScaraRobotProfile CreateProfile() =>
        new(
            firstLinkLengthMillimeters: 180,
            secondLinkLengthMillimeters: 120,
            linkCollisionRadiusMillimeters: 12,
            shoulderJoint: new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120, 240),
            elbowJoint: new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100, 200));

    private static double CrossProduct(
        ScaraToolPose lineStart,
        ScaraToolPose lineEnd,
        ScaraToolPose point) =>
        ((lineEnd.X - lineStart.X) * (point.Y - lineStart.Y)) -
        ((lineEnd.Y - lineStart.Y) * (point.X - lineStart.X));
}
