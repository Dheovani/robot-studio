using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation.Tests;

public sealed class DifferentialDriveOdometryCalculatorTests
{
    [Fact]
    public void Advance_WhenRobotTranslates_ShouldMoveBothWheelsEqually()
    {
        var odometry = DifferentialDriveOdometryCalculator.Advance(
            DifferentialDriveOdometry.Zero,
            CreateProfile(),
            new DifferentialDrivePose(0, 0, 0),
            new DifferentialDrivePose(100, 0, 0));

        Assert.Equal(100, odometry.LeftWheelTravelMillimeters, precision: 6);
        Assert.Equal(100, odometry.RightWheelTravelMillimeters, precision: 6);
        Assert.Equal(190.985932, odometry.LeftWheelRotationDegrees, precision: 6);
        Assert.Equal(190.985932, odometry.RightWheelRotationDegrees, precision: 6);
    }

    [Fact]
    public void Advance_WhenRobotTurnsNinetyDegrees_ShouldRotateWheelsInOppositeDirections()
    {
        var odometry = DifferentialDriveOdometryCalculator.Advance(
            DifferentialDriveOdometry.Zero,
            CreateProfile(),
            new DifferentialDrivePose(0, 0, 0),
            new DifferentialDrivePose(0, 0, 90));

        Assert.Equal(-30 * Math.PI, odometry.LeftWheelTravelMillimeters, precision: 6);
        Assert.Equal(30 * Math.PI, odometry.RightWheelTravelMillimeters, precision: 6);
        Assert.Equal(-180, odometry.LeftWheelRotationDegrees, precision: 6);
        Assert.Equal(180, odometry.RightWheelRotationDegrees, precision: 6);
    }

    [Fact]
    public void Advance_WhenCalledForMultipleSegments_ShouldAccumulateWheelMotion()
    {
        var profile = CreateProfile();
        var translated = DifferentialDriveOdometryCalculator.Advance(
            DifferentialDriveOdometry.Zero,
            profile,
            new DifferentialDrivePose(0, 0, 0),
            new DifferentialDrivePose(100, 0, 0));

        var rotated = DifferentialDriveOdometryCalculator.Advance(
            translated,
            profile,
            new DifferentialDrivePose(100, 0, 0),
            new DifferentialDrivePose(100, 0, 90));

        Assert.Equal(100 - (30 * Math.PI), rotated.LeftWheelTravelMillimeters, precision: 6);
        Assert.Equal(100 + (30 * Math.PI), rotated.RightWheelTravelMillimeters, precision: 6);
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
