using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Motion.Tests;

public sealed class DeltaMotionPlannerTests
{
    [Fact]
    public void PlanMove_WhenMovementIsValid_ShouldCreateCoordinatedActuatorPlan()
    {
        var planner = new DeltaMotionPlanner();

        var plan = planner.PlanMove(
            new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 0, CMillimeters: 0),
            new DeltaActuatorPosition(AMillimeters: 30, BMillimeters: 60, CMillimeters: 90),
            CreateProfile(),
            requestedActuatorVelocityMillimetersPerSecond: 200);

        var segment = Assert.Single(plan.Segments);
        Assert.Equal(90, plan.MaximumActuatorTravelMillimeters);
        Assert.Equal(90, segment.EffectiveActuatorVelocityMillimetersPerSecond);
        Assert.Equal(180, segment.Profile.Acceleration);
        Assert.True(plan.TotalDuration > TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void PlanMove_WhenStationary_ShouldReturnNoSegments()
    {
        var planner = new DeltaMotionPlanner();
        var actuators = new DeltaActuatorPosition(AMillimeters: 10, BMillimeters: 20, CMillimeters: 30);

        var plan = planner.PlanMove(actuators, actuators, CreateProfile());

        Assert.True(plan.IsStationary);
        Assert.Empty(plan.Segments);
        Assert.Equal(TimeSpan.Zero, plan.TotalDuration);
    }

    [Fact]
    public void PlanMove_WhenTargetIsOutsideLimits_ShouldThrow()
    {
        var planner = new DeltaMotionPlanner();

        Assert.Throws<InvalidRobotCommandException>(() =>
            planner.PlanMove(
                new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 0, CMillimeters: 0),
                new DeltaActuatorPosition(AMillimeters: 0, BMillimeters: 181, CMillimeters: 0),
                CreateProfile()));
    }

    private static DeltaRobotProfile CreateProfile() =>
        new(
            baseRadiusMillimeters: 140,
            toolZOffsetMillimeters: 0,
            actuatorA: new DeltaActuator(DeltaActuatorId.A, 0, 180, 120, 240),
            actuatorB: new DeltaActuator(DeltaActuatorId.B, 0, 180, 100, 200),
            actuatorC: new DeltaActuator(DeltaActuatorId.C, 0, 180, 90, 180));
}
