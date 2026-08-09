using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Domain.Tests;

public sealed class DifferentialDriveProfileTests
{
    [Fact]
    public void ValidatePosition_DoesNotThrow_WhenPoseIsInsideWorkspace()
    {
        var profile = CreateProfile();
        var pose = new DifferentialDrivePose(X: 100, Y: 150, HeadingDegrees: 90);

        var exception = Record.Exception(() => profile.ValidatePosition(pose));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidatePosition_Throws_WhenXIsOutsideWorkspace()
    {
        var profile = CreateProfile();
        var pose = new DifferentialDrivePose(X: 501, Y: 150, HeadingDegrees: 90);

        var exception = Assert.Throws<PositionOutOfRangeException>(() => profile.ValidatePosition(pose));

        Assert.Equal(AxisId.X, exception.Axis);
    }

    [Fact]
    public void ValidatePosition_Throws_WhenYIsOutsideWorkspace()
    {
        var profile = CreateProfile();
        var pose = new DifferentialDrivePose(X: 100, Y: 401, HeadingDegrees: 90);

        var exception = Assert.Throws<PositionOutOfRangeException>(() => profile.ValidatePosition(pose));

        Assert.Equal(AxisId.Y, exception.Axis);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(360, 0)]
    [InlineData(450, 90)]
    [InlineData(-90, 270)]
    public void NormalizeHeadingDegrees_ShouldReturnHeadingInsideOneTurn(
        double input,
        double expected)
    {
        var normalized = DifferentialDrivePose.NormalizeHeadingDegrees(input);

        Assert.Equal(expected, normalized);
    }

    [Fact]
    public void AngularDistanceDegreesTo_ShouldUseShortestTurn()
    {
        var start = new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 350);
        var end = new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 10);

        var distance = start.AngularDistanceDegreesTo(end);

        Assert.Equal(20, distance);
    }

    [Fact]
    public void Constructor_WhenLinearAccelerationIsNotPositive_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => CreateProfile(linearAcceleration: 0));
    }

    [Fact]
    public void Constructor_WhenAngularAccelerationIsNotPositive_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => CreateProfile(angularAcceleration: 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_WhenCollisionRadiusIsInvalid_ShouldThrow(double collisionRadius)
    {
        Assert.Throws<ArgumentException>(() => CreateProfile(collisionRadius: collisionRadius));
    }

    private static DifferentialDriveProfile CreateProfile(
        double linearAcceleration = 500,
        double angularAcceleration = 360,
        double collisionRadius = 70) =>
        new(
            minimumXMillimeters: 0,
            maximumXMillimeters: 500,
            minimumYMillimeters: 0,
            maximumYMillimeters: 400,
            wheelBaseMillimeters: 120,
            wheelRadiusMillimeters: 30,
            collisionRadiusMillimeters: collisionRadius,
            maximumLinearVelocityMillimetersPerSecond: 250,
            maximumAngularVelocityDegreesPerSecond: 180,
            maximumLinearAccelerationMillimetersPerSecondSquared: linearAcceleration,
            maximumAngularAccelerationDegreesPerSecondSquared: angularAcceleration);
}
