using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Motion.Tests;

public sealed class DeltaMotionPlannerTests
{
    [Fact]
    public void PlanLinearMove_WhenTargetIsReachable_ShouldCreateCollinearToolSamples()
    {
        var target = new DeltaToolPose(0, -45, -60);

        var plan = new DeltaCartesianMotionPlanner().PlanLinearMove(
            new DeltaActuatorPosition(0, 0, 0),
            target,
            CreateProfile(),
            requestedToolVelocityMillimetersPerSecond: 70);

        Assert.False(plan.IsStationary);
        Assert.NotNull(plan.ToolMotionProfile);
        Assert.True(plan.Segments.Count > 1);
        Assert.Equal(target, plan.EndToolPose);
        Assert.All(
            plan.Segments,
            segment =>
            {
                Assert.InRange(
                    Distance(segment.StartToolPose, segment.EndToolPose),
                    0,
                    DeltaCartesianMotionPlanner.DefaultMaximumToolSegmentLengthMillimeters + 0.000_001);
                Assert.InRange(
                    CrossProductMagnitude(plan.StartToolPose, target, segment.EndToolPose),
                    0,
                    0.000_001);
            });
    }

    [Fact]
    public void PlanLinearMove_WhenTargetIsUnreachable_ShouldThrow()
    {
        Assert.Throws<InvalidRobotCommandException>(() =>
            new DeltaCartesianMotionPlanner().PlanLinearMove(
                new DeltaActuatorPosition(0, 0, 0),
                new DeltaToolPose(0, 0, 20),
                CreateProfile()));
    }

    [Fact]
    public void PlanLinearMove_WhenRequestedFeedIsHigh_ShouldUseActuatorLimit()
    {
        var plan = new DeltaCartesianMotionPlanner().PlanLinearMove(
            new DeltaActuatorPosition(0, 0, 0),
            new DeltaToolPose(0, -45, -60),
            CreateProfile(),
            requestedToolVelocityMillimetersPerSecond: 10_000);

        Assert.NotNull(plan.ToolMotionProfile);
        Assert.InRange(plan.ToolMotionProfile.MaximumVelocity, 0, 9_999.999);
        Assert.True(plan.ToolMotionProfile.Acceleration > 0);
    }

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
            movingComponentCollisionRadiusMillimeters: 14,
            actuatorA: new DeltaActuator(DeltaActuatorId.A, 0, 180, 120, 240),
            actuatorB: new DeltaActuator(DeltaActuatorId.B, 0, 180, 100, 200),
            actuatorC: new DeltaActuator(DeltaActuatorId.C, 0, 180, 90, 180));

    private static double Distance(DeltaToolPose start, DeltaToolPose end)
    {
        var x = end.XMillimeters - start.XMillimeters;
        var y = end.YMillimeters - start.YMillimeters;
        var z = end.ZMillimeters - start.ZMillimeters;
        return Math.Sqrt((x * x) + (y * y) + (z * z));
    }

    private static double CrossProductMagnitude(
        DeltaToolPose lineStart,
        DeltaToolPose lineEnd,
        DeltaToolPose point)
    {
        var lineX = lineEnd.XMillimeters - lineStart.XMillimeters;
        var lineY = lineEnd.YMillimeters - lineStart.YMillimeters;
        var lineZ = lineEnd.ZMillimeters - lineStart.ZMillimeters;
        var pointX = point.XMillimeters - lineStart.XMillimeters;
        var pointY = point.YMillimeters - lineStart.YMillimeters;
        var pointZ = point.ZMillimeters - lineStart.ZMillimeters;
        var crossX = (lineY * pointZ) - (lineZ * pointY);
        var crossY = (lineZ * pointX) - (lineX * pointZ);
        var crossZ = (lineX * pointY) - (lineY * pointX);
        return Math.Sqrt((crossX * crossX) + (crossY * crossY) + (crossZ * crossZ));
    }
}
