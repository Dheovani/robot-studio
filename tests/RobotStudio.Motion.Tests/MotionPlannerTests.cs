using RobotStudio.Domain;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion.Tests;

public sealed class MotionPlannerTests
{
    [Fact]
    public void PlanLinearMove_ReturnsPlan_WhenMovementIsValid()
    {
        var planner = new MotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanLinearMove(
            new CartesianPosition(X: 0, Y: 0, Z: 0),
            new CartesianPosition(X: 120, Y: 80, Z: 40),
            profile);

        Assert.False(plan.IsStationary);
        Assert.Single(plan.Segments);
        Assert.Equal(80, plan.Segments[0].VelocityMillimetersPerSecond);
    }

    [Fact]
    public void PlanLinearMove_Throws_WhenEndPositionIsOutsideLimits()
    {
        var planner = new MotionPlanner();
        var profile = CreateProfile();

        Assert.Throws<PositionOutOfRangeException>(() =>
            planner.PlanLinearMove(
                new CartesianPosition(X: 0, Y: 0, Z: 0),
                new CartesianPosition(X: 120, Y: 80, Z: 151),
                profile));
    }

    [Fact]
    public void PlanLinearMove_ReturnsPositiveDuration_WhenMovementHasDisplacement()
    {
        var planner = new MotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanLinearMove(
            new CartesianPosition(X: 0, Y: 0, Z: 0),
            new CartesianPosition(X: 50, Y: 0, Z: 0),
            profile);

        Assert.True(plan.TotalDuration > TimeSpan.Zero);
    }

    [Fact]
    public void PlanLinearMove_ShouldExposeTotalDistance()
    {
        var planner = new MotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanLinearMove(
            new CartesianPosition(X: 0, Y: 0, Z: 0),
            new CartesianPosition(X: 3, Y: 4, Z: 0),
            profile);

        Assert.Equal(5, plan.DistanceMillimeters);
    }

    [Fact]
    public void PlanLinearMove_WhenMovementUsesOneAxis_ShouldExposeInvolvedAxis()
    {
        var planner = new MotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanLinearMove(
            new CartesianPosition(X: 0, Y: 0, Z: 0),
            new CartesianPosition(X: 50, Y: 0, Z: 0),
            profile);

        var axis = Assert.Single(plan.Segments[0].InvolvedAxes);
        Assert.Equal(AxisId.X, axis);
    }

    [Fact]
    public void PlanLinearMove_WhenMovementUsesTwoAxes_ShouldExposeInvolvedAxes()
    {
        var planner = new MotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanLinearMove(
            new CartesianPosition(X: 0, Y: 0, Z: 0),
            new CartesianPosition(X: 50, Y: 25, Z: 0),
            profile);

        Assert.Equal([AxisId.X, AxisId.Y], plan.Segments[0].InvolvedAxes);
        Assert.Equal(100, plan.Segments[0].VelocityMillimetersPerSecond);
    }

    [Fact]
    public void PlanLinearMove_WhenMovementUsesThreeAxes_ShouldExposeInvolvedAxes()
    {
        var planner = new MotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanLinearMove(
            new CartesianPosition(X: 0, Y: 0, Z: 0),
            new CartesianPosition(X: 50, Y: 25, Z: 10),
            profile);

        Assert.Equal([AxisId.X, AxisId.Y, AxisId.Z], plan.Segments[0].InvolvedAxes);
        Assert.Equal(80, plan.Segments[0].VelocityMillimetersPerSecond);
    }

    [Fact]
    public void PlanLinearMove_WhenRequestedVelocityIsLowerThanAxisLimit_ShouldUseRequestedVelocity()
    {
        var planner = new MotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanLinearMove(
            new CartesianPosition(X: 0, Y: 0, Z: 0),
            new CartesianPosition(X: 50, Y: 0, Z: 0),
            profile,
            requestedVelocityMillimetersPerSecond: 25);

        Assert.Equal(25, plan.Segments[0].VelocityMillimetersPerSecond);
    }

    [Fact]
    public void PlanLinearMove_WhenRequestedVelocityIsHigherThanAxisLimit_ShouldUseAxisLimit()
    {
        var planner = new MotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanLinearMove(
            new CartesianPosition(X: 0, Y: 0, Z: 0),
            new CartesianPosition(X: 50, Y: 0, Z: 0),
            profile,
            requestedVelocityMillimetersPerSecond: 999);

        Assert.Equal(120, plan.Segments[0].VelocityMillimetersPerSecond);
    }

    [Fact]
    public void PlanLinearMove_ReturnsStationaryPlan_WhenStartEqualsEnd()
    {
        var planner = new MotionPlanner();
        var profile = CreateProfile();
        var position = new CartesianPosition(X: 10, Y: 20, Z: 30);

        var plan = planner.PlanLinearMove(position, position, profile);

        Assert.True(plan.IsStationary);
        Assert.Empty(plan.Segments);
        Assert.Equal(0, plan.DistanceMillimeters);
        Assert.Equal(TimeSpan.Zero, plan.TotalDuration);
    }

    private static RobotProfile CreateProfile() =>
        RobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120),
            new Axis(AxisId.Y, 0, 200, 100),
            new Axis(AxisId.Z, 0, 150, 80));
}
