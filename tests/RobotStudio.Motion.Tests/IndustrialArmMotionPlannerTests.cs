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
        Assert.Equal(TimeSpan.FromSeconds(120d / 90d), plan.TotalDuration);
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

    private static IndustrialArmRobotProfile CreateProfile() =>
        new(
            100,
            180,
            140,
            80,
            [
                new(IndustrialArmJointId.J1Base, -180, 180, 120),
                new(IndustrialArmJointId.J2Shoulder, -120, 120, 100),
                new(IndustrialArmJointId.J3Elbow, -150, 150, 90),
                new(IndustrialArmJointId.J4WristRoll, -180, 180, 160),
                new(IndustrialArmJointId.J5WristPitch, -120, 120, 110),
                new(IndustrialArmJointId.J6ToolRoll, -360, 360, 200)
            ]);
}
