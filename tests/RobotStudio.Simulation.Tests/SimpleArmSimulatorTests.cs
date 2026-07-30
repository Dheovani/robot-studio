using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class SimpleArmSimulatorTests
{
    [Fact]
    public void Execute_WhenCommandIsHome_ShouldReturnToZeroJoints()
    {
        var simulator = new SimpleArmSimulator();
        var context = SimpleArmSimulationContext.Create(
            CreateProfile(),
            new SimpleArmJointPosition(BaseDegrees: 45, ShoulderDegrees: 30, ElbowDegrees: -15));
        var sequence = new RobotCommandSequence([new HomeCommand()]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0), result.FinalContext.CurrentJoints);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
    }

    [Fact]
    public void Execute_WhenCommandIsSimpleArmMoveJoints_ShouldMoveToTargetJoints()
    {
        var simulator = new SimpleArmSimulator();
        var context = SimpleArmSimulationContext.Create(
            CreateProfile(),
            new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0));
        var target = new SimpleArmJointPosition(BaseDegrees: 45, ShoulderDegrees: 30, ElbowDegrees: -15);
        var sequence = new RobotCommandSequence([new SimpleArmMoveJointsCommand(target)]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(target, result.FinalContext.CurrentJoints);
        Assert.True(result.FinalContext.ElapsedTime > TimeSpan.Zero);
    }

    [Fact]
    public void Execute_WhenCommandIsWait_ShouldAdvanceTimeWithoutMoving()
    {
        var simulator = new SimpleArmSimulator();
        var joints = new SimpleArmJointPosition(BaseDegrees: 10, ShoulderDegrees: 20, ElbowDegrees: 30);
        var context = SimpleArmSimulationContext.Create(CreateProfile(), joints);
        var sequence = new RobotCommandSequence([new WaitCommand(TimeSpan.FromMilliseconds(500))]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(joints, result.FinalContext.CurrentJoints);
        Assert.Equal(TimeSpan.FromMilliseconds(500), result.FinalContext.ElapsedTime);
    }

    [Fact]
    public void Execute_WhenCommandFails_ShouldReturnFaulted()
    {
        var simulator = new SimpleArmSimulator();
        var context = SimpleArmSimulationContext.Create(
            CreateProfile(),
            new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0));
        var sequence = new RobotCommandSequence(
        [
            new SimpleArmMoveJointsCommand(new SimpleArmJointPosition(BaseDegrees: 45, ShoulderDegrees: 30, ElbowDegrees: -15)),
            new SimpleArmMoveJointsCommand(new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 121, ElbowDegrees: 0))
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.False(result.Succeeded);
        Assert.Equal(RobotState.Faulted, result.FinalContext.State);
        Assert.Equal(new SimpleArmJointPosition(BaseDegrees: 45, ShoulderDegrees: 30, ElbowDegrees: -15), result.FinalContext.CurrentJoints);
    }

    private static SimpleArmRobotProfile CreateProfile() =>
        new(
            firstLinkLengthMillimeters: 120,
            secondLinkLengthMillimeters: 90,
            thirdLinkLengthMillimeters: 60,
            baseJoint: new SimpleArmJoint(SimpleArmJointId.Base, -180, 180, 100),
            shoulderJoint: new SimpleArmJoint(SimpleArmJointId.Shoulder, -120, 120, 90),
            elbowJoint: new SimpleArmJoint(SimpleArmJointId.Elbow, -150, 150, 80));
}
