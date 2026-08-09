using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion.Tests;

public sealed class SimpleArmMotionPlannerTests
{
    [Fact]
    public void PlanMove_WhenMovementIsValid_ShouldReturnPlan()
    {
        var planner = new SimpleArmMotionPlanner();

        var plan = planner.PlanMove(
            new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0),
            new SimpleArmJointPosition(BaseDegrees: 60, ShoulderDegrees: 30, ElbowDegrees: -20),
            CreateProfile());

        Assert.False(plan.IsStationary);
        Assert.Single(plan.Segments);
        Assert.True(plan.TotalDuration > TimeSpan.Zero);
        Assert.Equal(160, plan.Segments[0].Profile.Acceleration);
        Assert.True(plan.TotalDuration > TimeSpan.FromSeconds(60d / 80d));
    }

    [Fact]
    public void PlanMove_WhenRequestedVelocityExceedsJointLimit_ShouldUseLowestInvolvedJointLimit()
    {
        var planner = new SimpleArmMotionPlanner();

        var plan = planner.PlanMove(
            new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0),
            new SimpleArmJointPosition(BaseDegrees: 60, ShoulderDegrees: 30, ElbowDegrees: -20),
            CreateProfile(),
            requestedJointVelocityDegreesPerSecond: 500);

        Assert.Equal(80, plan.Segments[0].EffectiveJointVelocityDegreesPerSecond);
    }

    [Fact]
    public void PlanMove_WhenPositionDoesNotChange_ShouldReturnStationaryPlan()
    {
        var planner = new SimpleArmMotionPlanner();
        var joints = new SimpleArmJointPosition(BaseDegrees: 10, ShoulderDegrees: 20, ElbowDegrees: 30);

        var plan = planner.PlanMove(joints, joints, CreateProfile());

        Assert.True(plan.IsStationary);
        Assert.Equal(TimeSpan.Zero, plan.TotalDuration);
    }

    [Fact]
    public void PlanMove_WhenTargetIsOutsideLimits_ShouldThrow()
    {
        var planner = new SimpleArmMotionPlanner();

        Assert.Throws<InvalidRobotCommandException>(
            () => planner.PlanMove(
                new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 0, ElbowDegrees: 0),
                new SimpleArmJointPosition(BaseDegrees: 0, ShoulderDegrees: 121, ElbowDegrees: 0),
                CreateProfile()));
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
