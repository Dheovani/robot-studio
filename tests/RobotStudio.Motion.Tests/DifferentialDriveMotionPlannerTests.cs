using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Motion.Tests;

public sealed class DifferentialDriveMotionPlannerTests
{
    [Fact]
    public void PlanMove_ReturnsStationaryPlan_WhenPoseDoesNotChange()
    {
        var planner = new DifferentialDriveMotionPlanner();
        var profile = CreateProfile();
        var pose = new DifferentialDrivePose(X: 100, Y: 100, HeadingDegrees: 90);

        var plan = planner.PlanMove(pose, pose, profile);

        Assert.True(plan.IsStationary);
        Assert.Empty(plan.Segments);
        Assert.Equal(TimeSpan.Zero, plan.TotalDuration);
    }

    [Fact]
    public void PlanMove_ReturnsTranslationSegment_WhenPositionChanges()
    {
        var planner = new DifferentialDriveMotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanMove(
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0),
            new DifferentialDrivePose(X: 300, Y: 400, HeadingDegrees: 0),
            profile);

        var segment = Assert.Single(plan.Segments);
        Assert.Equal(DifferentialDriveMotionKind.Translation, segment.Kind);
        Assert.Equal(500, plan.TranslationDistanceMillimeters);
        Assert.Equal(250, segment.LinearVelocityMillimetersPerSecond);
        Assert.Equal(500, segment.Profile.Acceleration);
        Assert.True(plan.TotalDuration > TimeSpan.Zero);
    }

    [Fact]
    public void PlanMove_ReturnsRotationSegment_WhenHeadingChanges()
    {
        var planner = new DifferentialDriveMotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanMove(
            new DifferentialDrivePose(X: 100, Y: 100, HeadingDegrees: 350),
            new DifferentialDrivePose(X: 100, Y: 100, HeadingDegrees: 10),
            profile);

        var segment = Assert.Single(plan.Segments);
        Assert.Equal(DifferentialDriveMotionKind.Rotation, segment.Kind);
        Assert.Equal(20, plan.RotationDegrees);
        Assert.Equal(180, segment.Profile.MaximumVelocity);
        Assert.Equal(360, segment.Profile.Acceleration);
        Assert.True(segment.Profile.IsTriangular);
        Assert.True(segment.AngularVelocityDegreesPerSecond < 180);
    }

    [Fact]
    public void PlanMove_ReturnsTranslationThenRotation_WhenPositionAndHeadingChange()
    {
        var planner = new DifferentialDriveMotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanMove(
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0),
            new DifferentialDrivePose(X: 100, Y: 0, HeadingDegrees: 90),
            profile);

        Assert.Equal(
            [DifferentialDriveMotionKind.Translation, DifferentialDriveMotionKind.Rotation],
            plan.Segments.Select(segment => segment.Kind));
    }

    [Fact]
    public void PlanMove_WhenRequestedLinearVelocityIsLowerThanLimit_ShouldUseRequestedVelocity()
    {
        var planner = new DifferentialDriveMotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanMove(
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0),
            new DifferentialDrivePose(X: 100, Y: 0, HeadingDegrees: 0),
            profile,
            requestedLinearVelocityMillimetersPerSecond: 80);

        Assert.Equal(80, plan.Segments[0].LinearVelocityMillimetersPerSecond);
    }

    [Fact]
    public void PlanMove_WhenRequestedAngularVelocityIsHigherThanLimit_ShouldUseAngularLimit()
    {
        var planner = new DifferentialDriveMotionPlanner();
        var profile = CreateProfile();

        var plan = planner.PlanMove(
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0),
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 90),
            profile,
            requestedAngularVelocityDegreesPerSecond: 999);

        Assert.Equal(180, plan.Segments[0].AngularVelocityDegreesPerSecond);
    }

    [Fact]
    public void PlanMove_Throws_WhenEndPoseIsOutsideWorkspace()
    {
        var planner = new DifferentialDriveMotionPlanner();
        var profile = CreateProfile();

        Assert.Throws<PositionOutOfRangeException>(() =>
            planner.PlanMove(
                new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0),
                new DifferentialDrivePose(X: 501, Y: 0, HeadingDegrees: 0),
                profile));
    }

    private static DifferentialDriveProfile CreateProfile() =>
        new(
            minimumXMillimeters: 0,
            maximumXMillimeters: 500,
            minimumYMillimeters: 0,
            maximumYMillimeters: 400,
            wheelBaseMillimeters: 120,
            wheelRadiusMillimeters: 30,
            maximumLinearVelocityMillimetersPerSecond: 250,
            maximumAngularVelocityDegreesPerSecond: 180,
            maximumLinearAccelerationMillimetersPerSecondSquared: 500,
            maximumAngularAccelerationDegreesPerSecondSquared: 360);
}
