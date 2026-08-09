using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Motion.Tests;

public sealed class ScaraMotionPlannerTests
{
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
            shoulderJoint: new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120, 240),
            elbowJoint: new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100, 200));
}
