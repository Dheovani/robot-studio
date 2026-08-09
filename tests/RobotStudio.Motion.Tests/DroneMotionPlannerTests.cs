using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Exceptions;
using RobotStudio.Motion;

namespace RobotStudio.Motion.Tests;

public sealed class DroneMotionPlannerTests
{
    [Fact]
    public void PlanMove_WhenPoseChanges_ShouldCreateCoordinatedSegment()
    {
        var planner = new DroneMotionPlanner();

        var plan = planner.PlanMove(
            new DronePose(0, 0, 0, 0),
            new DronePose(120, 90, 60, 90),
            CreateProfile(),
            requestedLinearVelocityMillimetersPerSecond: 200,
            requestedYawVelocityDegreesPerSecond: 60);

        Assert.Single(plan.Segments);
        Assert.True(plan.DistanceMillimeters > 0);
        Assert.Equal(90, plan.YawRotationDegrees);
        Assert.True(plan.TotalDuration > TimeSpan.Zero);
        Assert.True(plan.Segments[0].LinearVelocityMillimetersPerSecond <= 180);
        Assert.True(plan.Segments[0].YawVelocityDegreesPerSecond <= 60);
    }

    [Fact]
    public void PlanMove_WhenOnlyYawChanges_ShouldCreateRotationOnlySegment()
    {
        var planner = new DroneMotionPlanner();

        var plan = planner.PlanMove(
            new DronePose(50, 50, 50, 0),
            new DronePose(50, 50, 50, 90),
            CreateProfile(),
            requestedYawVelocityDegreesPerSecond: 45);

        var segment = Assert.Single(plan.Segments);
        Assert.Equal(0, segment.LinearVelocityMillimetersPerSecond);
        Assert.Equal(45, segment.YawProfile!.MaximumVelocity);
        Assert.Equal(240, segment.YawProfile.Acceleration);
        Assert.True(segment.YawVelocityDegreesPerSecond < 45);
        Assert.Equal(TimeSpan.FromMilliseconds(2187.5), segment.Duration);
    }

    [Fact]
    public void PlanMove_WhenOnlyAttitudeChanges_ShouldCreateSynchronizedTiltProfile()
    {
        var plan = new DroneMotionPlanner().PlanMove(
            new DronePose(50, 50, 50, YawDegrees: 0),
            new DronePose(50, 50, 50, YawDegrees: 0, RollDegrees: 30, PitchDegrees: -15),
            CreateProfile(),
            requestedAttitudeVelocityDegreesPerSecond: 60);

        var segment = Assert.Single(plan.Segments);
        Assert.Equal(30, plan.MaximumTiltRotationDegrees);
        Assert.NotNull(segment.AttitudeProfile);
        Assert.Null(segment.TranslationProfile);
        Assert.Null(segment.YawProfile);
        Assert.Equal(60, segment.AttitudeProfile.MaximumVelocity);
        Assert.Equal(360, segment.AttitudeProfile.Acceleration);
    }

    [Fact]
    public void PlanMove_WhenPoseIsUnchanged_ShouldReturnEmptyPlan()
    {
        var planner = new DroneMotionPlanner();
        var pose = new DronePose(50, 50, 50, 90);

        var plan = planner.PlanMove(pose, pose, CreateProfile());

        Assert.Empty(plan.Segments);
        Assert.Equal(TimeSpan.Zero, plan.TotalDuration);
    }

    [Fact]
    public void PlanMove_WhenTargetIsOutsideFlightVolume_ShouldThrow()
    {
        var planner = new DroneMotionPlanner();

        Assert.Throws<PositionOutOfRangeException>(() =>
            planner.PlanMove(
                new DronePose(0, 0, 0, 0),
                new DronePose(0, 0, 251, 0),
                CreateProfile()));
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
            maximumYawAccelerationDegreesPerSecondSquared: 240,
            maximumTiltDegrees: 45,
            maximumAttitudeVelocityDegreesPerSecond: 180,
            maximumAttitudeAccelerationDegreesPerSecondSquared: 360);
}
