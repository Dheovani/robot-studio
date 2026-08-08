using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion.Tests;

public sealed class XYPlotterMotionPlannerTests
{
    [Fact]
    public void XYPlotterMotionPlanner_ShouldImplementMotionPlannerForXYPlotterProfile()
    {
        var planner = new XYPlotterMotionPlanner();

        Assert.IsAssignableFrom<IMotionPlanner<XYPlotterPosition, XYPlotterProfile>>(planner);
    }

    [Fact]
    public void PlanMove_ReturnsPlan_WhenMovementIsValid()
    {
        var planner = new XYPlotterMotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanMove(
            new XYPlotterPosition(X: 0, Y: 0),
            new XYPlotterPosition(X: 120, Y: 80),
            profile);

        Assert.False(plan.IsStationary);
        Assert.Single(plan.Segments);
        Assert.Equal(["X", "Y"], plan.Segments[0].InvolvedComponents.Select(component => component.Name));
        Assert.Equal(100, plan.Segments[0].VelocityMillimetersPerSecond);
    }

    [Fact]
    public void PlanMove_Throws_WhenEndPositionIsOutsideLimits()
    {
        var planner = new XYPlotterMotionPlanner();
        var profile = CreateProfile();

        Assert.Throws<PositionOutOfRangeException>(() =>
            planner.PlanMove(
                new XYPlotterPosition(X: 0, Y: 0),
                new XYPlotterPosition(X: 120, Y: 201),
                profile));
    }

    [Fact]
    public void PlanMove_WhenRequestedVelocityIsLowerThanAxisLimit_ShouldUseRequestedVelocity()
    {
        var planner = new XYPlotterMotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanMove(
            new XYPlotterPosition(X: 0, Y: 0),
            new XYPlotterPosition(X: 100, Y: 0),
            profile,
            requestedVelocityMillimetersPerSecond: 40);

        Assert.Equal(40, plan.Segments[0].VelocityMillimetersPerSecond);
    }

    [Fact]
    public void PlanMove_WhenMovementHasDisplacement_ShouldCreateAccelerationAwareProfile()
    {
        var planner = new XYPlotterMotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanMove(
            new XYPlotterPosition(X: 0, Y: 0),
            new XYPlotterPosition(X: 100, Y: 100),
            profile);

        var segment = Assert.Single(plan.Segments);
        Assert.True(segment.AccelerationMillimetersPerSecondSquared > 0);
        Assert.Equal(segment.Duration, segment.Profile.TotalDuration);
    }

    [Fact]
    public void PlanMove_ReturnsStationaryPlan_WhenStartEqualsEnd()
    {
        var planner = new XYPlotterMotionPlanner();
        var profile = CreateProfile();
        var position = new XYPlotterPosition(X: 10, Y: 20);

        var plan = planner.PlanMove(position, position, profile);

        Assert.True(plan.IsStationary);
        Assert.Empty(plan.Segments);
        Assert.Equal(0, plan.DistanceMillimeters);
        Assert.Equal(TimeSpan.Zero, plan.TotalDuration);
    }

    private static XYPlotterProfile CreateProfile() =>
        XYPlotterProfile.Create(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200));
}
