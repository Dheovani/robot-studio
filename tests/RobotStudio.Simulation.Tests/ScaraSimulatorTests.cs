using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Simulation.Tests;

public sealed class ScaraSimulatorTests
{
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
            shoulderJoint: new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120),
            elbowJoint: new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100));
}
