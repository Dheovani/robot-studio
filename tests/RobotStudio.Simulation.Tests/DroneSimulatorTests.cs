using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Commands;
using RobotStudio.Simulation;

namespace RobotStudio.Simulation.Tests;

public sealed class DroneSimulatorTests
{
    [Fact]
    public void Execute_WhenCommandIsHome_ShouldReturnToOrigin()
    {
        var simulator = new DroneSimulator();
        var context = DroneSimulationContext.Create(
            CreateProfile(),
            new DronePose(120, 80, 40, 90));

        var result = simulator.Execute(context, new RobotCommandSequence([new HomeCommand()]));

        Assert.True(result.Succeeded);
        Assert.Equal(new DronePose(0, 0, 0, 0), result.FinalContext.CurrentPose);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
    }

    [Fact]
    public void Execute_WhenCommandIsDroneMove_ShouldMoveToTargetPose()
    {
        var simulator = new DroneSimulator();
        var context = DroneSimulationContext.Create(
            CreateProfile(),
            new DronePose(0, 0, 0, 0));
        var target = new DronePose(120, 80, 40, 450);
        var sequence = new RobotCommandSequence(
            [new DroneMoveCommand(target, requestedLinearVelocityMillimetersPerSecond: 120)]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(target with { YawDegrees = 90 }, result.FinalContext.CurrentPose);
        Assert.True(result.FinalContext.ElapsedTime > TimeSpan.Zero);
    }

    [Fact]
    public void Execute_WhenCommandFails_ShouldReturnFaultedResult()
    {
        var simulator = new DroneSimulator();
        var context = DroneSimulationContext.Create(
            CreateProfile(),
            new DronePose(0, 0, 0, 0));
        var sequence = new RobotCommandSequence(
            [new DroneMoveCommand(new DronePose(0, 0, 251, 0))]);

        var result = simulator.Execute(context, sequence);

        Assert.False(result.Succeeded);
        Assert.Equal(RobotState.Faulted, result.FinalContext.State);
        Assert.NotNull(result.Failure);
    }

    private static DroneProfile CreateProfile() =>
        new(
            minimumXMillimeters: 0,
            maximumXMillimeters: 500,
            minimumYMillimeters: 0,
            maximumYMillimeters: 400,
            minimumZMillimeters: 0,
            maximumZMillimeters: 250,
            maximumLinearVelocityMillimetersPerSecond: 180,
            maximumYawVelocityDegreesPerSecond: 120,
            maximumLinearAccelerationMillimetersPerSecondSquared: 360,
            maximumYawAccelerationDegreesPerSecondSquared: 240);
}
