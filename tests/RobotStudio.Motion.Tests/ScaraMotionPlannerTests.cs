using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion.Tests;

public sealed class ScaraMotionPlannerTests
{
    [Fact]
    public void PlanLinearMove_WhenTargetIsReachable_ShouldCreateCollinearToolWaypoints()
    {
        var profile = CreateProfile();
        var startJoints = new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0);
        var target = new ScaraToolPose(X: 220, Y: 80);

        var plan = new ScaraCartesianMotionPlanner().PlanLinearMove(
            startJoints,
            target,
            profile,
            requestedToolVelocityMillimetersPerSecond: 80);

        Assert.False(plan.IsStationary);
        Assert.True(plan.Segments.Count > 1);
        Assert.Equal(target, plan.EndToolPose);
        Assert.NotNull(plan.ToolMotionProfile);
        Assert.InRange(plan.ToolMotionProfile.MaximumVelocity, 0, 80);
        Assert.All(
            plan.Segments,
            segment =>
            {
                Assert.InRange(
                    Distance(segment.StartToolPose, segment.EndToolPose),
                    0,
                    ScaraCartesianMotionPlanner.DefaultMaximumToolSegmentLengthMillimeters + 0.000_001);
                Assert.InRange(
                    CrossProduct(plan.StartToolPose, target, segment.EndToolPose),
                    -0.000_001,
                    0.000_001);
            });
        Assert.True(plan.TotalDuration > TimeSpan.Zero);
    }

    [Fact]
    public void PlanLinearMove_WhenRequestedVelocityExceedsJointLimits_ShouldLimitToolProfile()
    {
        var plan = new ScaraCartesianMotionPlanner().PlanLinearMove(
            new ScaraJointPosition(0, 0),
            new ScaraToolPose(220, 80),
            CreateProfile(),
            requestedToolVelocityMillimetersPerSecond: 10_000);

        Assert.NotNull(plan.ToolMotionProfile);
        Assert.InRange(plan.ToolMotionProfile.MaximumVelocity, 0, 9_999.999);
        Assert.True(plan.ToolMotionProfile.Acceleration > 0);
    }

    [Fact]
    public void PlanLinearMove_WhenTargetIsUnreachable_ShouldThrow()
    {
        var planner = new ScaraCartesianMotionPlanner();

        Assert.Throws<InvalidRobotCommandException>(() =>
            planner.PlanLinearMove(
                new ScaraJointPosition(0, 0),
                new ScaraToolPose(400, 0),
                CreateProfile()));
    }

    [Fact]
    public void PlanLinearMove_WhenStartUsesElbowUp_ShouldExplainConfigurationRequirement()
    {
        var exception = Assert.Throws<InvalidRobotCommandException>(() =>
            new ScaraCartesianMotionPlanner().PlanLinearMove(
                new ScaraJointPosition(35, -80),
                new ScaraToolPose(180, 80),
                CreateProfile()));

        Assert.Contains("elbow-down", exception.Message);
    }

    [Fact]
    public void PlanMove_ReturnsPlan_WhenMovementIsValid()
    {
        var planner = new ScaraMotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanMove(
            new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0),
            new ScaraJointPosition(ShoulderDegrees: 60, ElbowDegrees: 30),
            profile);

        Assert.False(plan.IsStationary);
        Assert.Single(plan.Segments);
        Assert.Equal(60, plan.MaximumJointTravelDegrees);
        Assert.Equal(["Shoulder", "Elbow"], plan.Segments[0].InvolvedComponents.Select(component => component.Name));
        Assert.Equal(100, plan.Segments[0].JointVelocityDegreesPerSecond);
        Assert.Equal(200, plan.Segments[0].Profile.Acceleration);
        Assert.True(plan.TotalDuration > TimeSpan.FromSeconds(60d / 100d));
    }

    [Fact]
    public void PlanMove_WhenRequestedVelocityIsLowerThanLimits_ShouldUseRequestedVelocity()
    {
        var planner = new ScaraMotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanMove(
            new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0),
            new ScaraJointPosition(ShoulderDegrees: 60, ElbowDegrees: 30),
            profile,
            requestedJointVelocityDegreesPerSecond: 40);

        Assert.Equal(40, plan.Segments[0].JointVelocityDegreesPerSecond);
    }

    [Fact]
    public void PlanMove_ReturnsStationaryPlan_WhenJointsDoNotChange()
    {
        var planner = new ScaraMotionPlanner();
        var profile = CreateProfile();
        var joints = new ScaraJointPosition(ShoulderDegrees: 10, ElbowDegrees: 20);

        var plan = planner.PlanMove(joints, joints, profile);

        Assert.True(plan.IsStationary);
        Assert.Empty(plan.Segments);
        Assert.Equal(TimeSpan.Zero, plan.TotalDuration);
    }

    [Fact]
    public void PlanMove_Throws_WhenTargetJointIsOutsideLimits()
    {
        var planner = new ScaraMotionPlanner();
        var profile = CreateProfile();

        Assert.Throws<InvalidRobotCommandException>(() =>
            planner.PlanMove(
                new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0),
                new ScaraJointPosition(ShoulderDegrees: 181, ElbowDegrees: 0),
                profile));
    }

    private static ScaraRobotProfile CreateProfile() =>
        new(
            firstLinkLengthMillimeters: 180,
            secondLinkLengthMillimeters: 120,
            linkCollisionRadiusMillimeters: 12,
            shoulderJoint: new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120, 240),
            elbowJoint: new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100, 200));

    private static double Distance(ScaraToolPose start, ScaraToolPose end)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }

    private static double CrossProduct(
        ScaraToolPose lineStart,
        ScaraToolPose lineEnd,
        ScaraToolPose point) =>
        ((lineEnd.X - lineStart.X) * (point.Y - lineStart.Y)) -
        ((lineEnd.Y - lineStart.Y) * (point.X - lineStart.X));
}
