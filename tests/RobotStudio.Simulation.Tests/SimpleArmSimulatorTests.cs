using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class SimpleArmSimulatorTests
{
    [Fact]
    public void Execute_WhenSpatialLinkPathIsObstructed_ShouldFaultWithoutMoving()
    {
        var start = new SimpleArmJointPosition(0, 0, 0);
        var environment = new SpatialSimulationEnvironment(
            [new SpatialObstacle("fixture", new SpatialPoint(80, 20, -5), new SpatialPoint(100, 30, 5))]);
        var result = new SimpleArmSimulator(environment).Execute(
            SimpleArmSimulationContext.Create(CreateProfile(), start),
            new RobotCommandSequence([new SimpleArmMoveJointsCommand(new SimpleArmJointPosition(30, 0, 0))]));

        Assert.False(result.Succeeded);
        Assert.Equal(start, result.FinalContext.CurrentJoints);
        Assert.Equal(TimeSpan.Zero, result.FinalContext.ElapsedTime);
        Assert.IsType<SpatialPathObstructedException>(result.Failure);
    }

    [Fact]
    public void Execute_WhenResettingFault_ShouldPreserveJointsAndElapsedTime()
    {
        var context = SimpleArmSimulationContext.Create(CreateProfile(), new SimpleArmJointPosition(45, 30, -15)) with
        {
            State = RobotState.Faulted,
            ElapsedTime = TimeSpan.FromSeconds(2)
        };
        var result = new SimpleArmSimulator().Execute(context, new RobotCommandSequence([new ResetFaultCommand()]));

        Assert.True(result.Succeeded);
        Assert.Equal(RobotState.Idle, result.FinalContext.State);
        Assert.Equal(context.CurrentJoints, result.FinalContext.CurrentJoints);
        Assert.Equal(context.ElapsedTime, result.FinalContext.ElapsedTime);
    }

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
            linkCollisionRadiusMillimeters: 10,
            baseJoint: new SimpleArmJoint(SimpleArmJointId.Base, -180, 180, 100, 200),
            shoulderJoint: new SimpleArmJoint(SimpleArmJointId.Shoulder, -120, 120, 90, 180),
            elbowJoint: new SimpleArmJoint(SimpleArmJointId.Elbow, -150, 150, 80, 160));
}
