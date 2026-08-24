using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion.Tests;

public sealed class IndustrialArmMotionPlannerTests
{
    [Fact]
    public void PlanMove_WhenSixJointsMove_ShouldCreateOneCoordinatedSegment()
    {
        var plan = new IndustrialArmMotionPlanner().PlanMove(
            IndustrialArmJointPosition.Home,
            new IndustrialArmJointPosition(60, 30, -45, 90, 20, 120),
            CreateProfile(),
            requestedJointVelocityDegreesPerSecond: 500);

        var segment = Assert.Single(plan.Segments);
        Assert.Equal(6, segment.InvolvedJoints.Count);
        Assert.Equal(90, segment.EffectiveJointVelocityDegreesPerSecond);
        Assert.Equal(180, segment.Profile.Acceleration);
        Assert.Equal(TimeSpan.FromSeconds(11d / 6d), plan.TotalDuration);
    }

    [Fact]
    public void PlanMove_WhenTargetIsHome_ShouldReturnStationaryPlan()
    {
        var plan = new IndustrialArmMotionPlanner().PlanMove(
            IndustrialArmJointPosition.Home,
            IndustrialArmJointPosition.Home,
            CreateProfile());

        Assert.True(plan.IsStationary);
        Assert.Equal(TimeSpan.Zero, plan.TotalDuration);
    }

    [Fact]
    public void PlanMove_WhenTargetExceedsJointLimit_ShouldThrow()
    {
        Assert.Throws<InvalidRobotCommandException>(() =>
            new IndustrialArmMotionPlanner().PlanMove(
                IndustrialArmJointPosition.Home,
                new IndustrialArmJointPosition(181, 0, 0, 0, 0, 0),
                CreateProfile()));
    }

    [Fact]
    public void PlanMove_WhenOnlyWristJointsMove_ShouldListOnlyThoseJoints()
    {
        var plan = new IndustrialArmMotionPlanner().PlanMove(
            IndustrialArmJointPosition.Home,
            new IndustrialArmJointPosition(0, 0, 0, 45, -30, 90),
            CreateProfile(),
            requestedJointVelocityDegreesPerSecond: 70);

        var segment = Assert.Single(plan.Segments);
        Assert.Equal(
            ["J4WristRoll", "J5WristPitch", "J6ToolRoll"],
            segment.InvolvedJoints.Select(component => component.Name));
        Assert.Equal(70, segment.EffectiveJointVelocityDegreesPerSecond);
        Assert.Equal(220, segment.Profile.Acceleration);
        Assert.True(segment.Duration > TimeSpan.FromSeconds(90d / 70d));
    }

    [Fact]
    public void PlanMove_WhenRequestedVelocityIsNotPositive_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IndustrialArmMotionPlanner().PlanMove(
                IndustrialArmJointPosition.Home,
                new IndustrialArmJointPosition(10, 0, 0, 0, 0, 0),
                CreateProfile(),
                requestedJointVelocityDegreesPerSecond: 0));
    }

    [Fact]
    public void PlanLinearMove_WhenToolPoseIsReachable_ShouldSampleStraightPath()
    {
        var target = new IndustrialArmToolPose(300, 0, 180, 20, 10, 0);

        var plan = new IndustrialArmCartesianMotionPlanner().PlanLinearMove(
            IndustrialArmJointPosition.Home,
            target,
            CreateProfile(),
            requestedToolVelocityMillimetersPerSecond: 70);

        Assert.False(plan.IsStationary);
        Assert.True(plan.TotalDuration > TimeSpan.Zero);
        Assert.Equal(target, plan.EndToolPose);
        Assert.All(
            plan.Segments,
            segment => Assert.InRange(
                Distance(segment.StartToolPose, segment.EndToolPose),
                0,
                IndustrialArmCartesianMotionPlanner.DefaultMaximumToolSegmentLengthMillimeters + 0.000_001));
    }

    [Fact]
    public void PlanLinearMove_WhenFeedIsVeryHigh_ShouldRemainJointLimited()
    {
        var planner = new IndustrialArmCartesianMotionPlanner();
        var profile = CreateProfile();
        var target = new IndustrialArmToolPose(300, 0, 180, 20, 10, 0);

        var unrestricted = planner.PlanLinearMove(
            IndustrialArmJointPosition.Home,
            target,
            profile);
        var requested = planner.PlanLinearMove(
            IndustrialArmJointPosition.Home,
            target,
            profile,
            requestedToolVelocityMillimetersPerSecond: 100_000);

        Assert.Equal(unrestricted.TotalDuration, requested.TotalDuration);
    }

    [Fact]
    public void PlanLinearMove_WhenPoseViolatesYawCoupling_ShouldThrow()
    {
        Assert.Throws<InvalidRobotCommandException>(() =>
            new IndustrialArmCartesianMotionPlanner().PlanLinearMove(
                IndustrialArmJointPosition.Home,
                new IndustrialArmToolPose(300, 0, 180, 0, 0, 20),
                CreateProfile()));
    }

    private static double Distance(IndustrialArmToolPose start, IndustrialArmToolPose end)
    {
        var x = end.XMillimeters - start.XMillimeters;
        var y = end.YMillimeters - start.YMillimeters;
        var z = end.ZMillimeters - start.ZMillimeters;
        return Math.Sqrt((x * x) + (y * y) + (z * z));
    }

    private static IndustrialArmRobotProfile CreateProfile() =>
        new(
            100,
            180,
            140,
            80,
            12,
            [
                new(IndustrialArmJointId.J1Base, -180, 180, 120, 240),
                new(IndustrialArmJointId.J2Shoulder, -120, 120, 100, 200),
                new(IndustrialArmJointId.J3Elbow, -150, 150, 90, 180),
                new(IndustrialArmJointId.J4WristRoll, -180, 180, 160, 320),
                new(IndustrialArmJointId.J5WristPitch, -120, 120, 110, 220),
                new(IndustrialArmJointId.J6ToolRoll, -360, 360, 200, 400)
            ]);
}
