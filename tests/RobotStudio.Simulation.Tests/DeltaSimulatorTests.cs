using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Simulation.Tests;

public sealed class DeltaSimulatorTests
{
    [Fact]
    public void Execute_WhenParallelMechanismPathIsObstructed_ShouldFaultWithoutMoving()
    {
        var start = new DeltaActuatorPosition(0, 0, 0);
        var environment = new SpatialSimulationEnvironment(
            [new SpatialObstacle("fixture", new SpatialPoint(-30, -60, -75), new SpatialPoint(0, -30, -45))]);
        var result = new DeltaSimulator(environment).Execute(
            DeltaSimulationContext.Create(CreateProfile(), start),
            new RobotCommandSequence(
                [new DeltaMoveActuatorsCommand(new DeltaActuatorPosition(30, 60, 90))]));

        Assert.False(result.Succeeded);
        Assert.Equal(start, result.FinalContext.CurrentActuators);
        Assert.Equal(TimeSpan.Zero, result.FinalContext.ElapsedTime);
        Assert.IsType<SpatialPathObstructedException>(result.Failure);
    }

    [Fact]
    public void Execute_WhenResettingFault_ShouldPreserveActuatorsAndElapsedTime()
    {
        var context = DeltaSimulationContext.Create(CreateProfile(), new DeltaActuatorPosition(30, 60, 90)) with
        {
            State = RobotState.Faulted,
            ElapsedTime = TimeSpan.FromSeconds(2)
        };
        var result = new DeltaSimulator().Execute(context, new RobotCommandSequence([new ResetFaultCommand()]));

        Assert.True(result.Succeeded);
        Assert.Equal(RobotState.Idle, result.FinalContext.State);
        Assert.Equal(context.CurrentActuators, result.FinalContext.CurrentActuators);
        Assert.Equal(context.ElapsedTime, result.FinalContext.ElapsedTime);
    }

    [Fact]
    public void Execute_WhenCommandIsHome_ShouldReturnToZeroActuators()
    {
        var simulator = new DeltaSimulator();
        var context = DeltaSimulationContext.Create(
            CreateProfile(),
            new DeltaActuatorPosition(AMillimeters: 20, BMillimeters: 30, CMillimeters: 40));

        var result = simulator.Execute(context, new RobotCommandSequence([new HomeCommand()]));

        Assert.True(result.Succeeded);
        Assert.Equal(new DeltaActuatorPosition(0, 0, 0), result.FinalContext.CurrentActuators);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
    }

    [Fact]
    public void Execute_WhenCommandIsDeltaMove_ShouldMoveToTargetActuators()
    {
        var simulator = new DeltaSimulator();
        var context = DeltaSimulationContext.Create(
            CreateProfile(),
            new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 0, CMillimeters: 0));
        var target = new DeltaActuatorPosition(AMillimeters: 30, BMillimeters: 60, CMillimeters: 90);

        var result = simulator.Execute(
            context,
            new RobotCommandSequence([new DeltaMoveActuatorsCommand(target)]));

        Assert.True(result.Succeeded);
        Assert.Equal(target, result.FinalContext.CurrentActuators);
        Assert.Contains(result.Timeline, step => step.CommandName == nameof(DeltaMoveActuatorsCommand));
    }

    [Fact]
    public void Execute_WhenCommandFails_ShouldReturnFaulted()
    {
        var simulator = new DeltaSimulator();
        var context = DeltaSimulationContext.Create(
            CreateProfile(),
            new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 0, CMillimeters: 0));

        var result = simulator.Execute(
            context,
            new RobotCommandSequence(
            [
                new DeltaMoveActuatorsCommand(new DeltaActuatorPosition(AMillimeters: 30, BMillimeters: 60, CMillimeters: 90)),
                new DeltaMoveActuatorsCommand(new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 181, CMillimeters: 0))
            ]));

        Assert.False(result.Succeeded);
        Assert.Equal(RobotState.Faulted, result.FinalContext.State);
        Assert.Equal(new DeltaActuatorPosition(30, 60, 90), result.FinalContext.CurrentActuators);
    }

    private static DeltaRobotProfile CreateProfile() =>
        new(
            baseRadiusMillimeters: 140,
            toolZOffsetMillimeters: 0,
            movingComponentCollisionRadiusMillimeters: 14,
            actuatorA: new DeltaActuator(DeltaActuatorId.A, 0, 180, 120, 240),
            actuatorB: new DeltaActuator(DeltaActuatorId.B, 0, 180, 100, 200),
            actuatorC: new DeltaActuator(DeltaActuatorId.C, 0, 180, 90, 180));
}
