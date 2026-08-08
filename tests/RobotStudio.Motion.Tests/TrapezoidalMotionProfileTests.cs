namespace RobotStudio.Motion.Tests;

public sealed class TrapezoidalMotionProfileTests
{
    [Fact]
    public void Constructor_WhenMovementCanReachMaximumVelocity_ShouldCreateTrapezoidalProfile()
    {
        var profile = new TrapezoidalMotionProfile(
            distance: 100,
            maximumVelocity: 10,
            acceleration: 20);

        Assert.False(profile.IsTriangular);
        Assert.Equal(10, profile.PeakVelocity);
        Assert.Equal(TimeSpan.FromSeconds(0.5), profile.AccelerationDuration);
        Assert.Equal(TimeSpan.FromSeconds(9.5), profile.ConstantVelocityDuration);
        Assert.Equal(TimeSpan.FromSeconds(0.5), profile.DecelerationDuration);
        Assert.Equal(TimeSpan.FromSeconds(10.5), profile.TotalDuration);
    }

    [Fact]
    public void Constructor_WhenMovementIsTooShortForMaximumVelocity_ShouldCreateTriangularProfile()
    {
        var profile = new TrapezoidalMotionProfile(
            distance: 2,
            maximumVelocity: 10,
            acceleration: 20);

        Assert.True(profile.IsTriangular);
        Assert.Equal(Math.Sqrt(40), profile.PeakVelocity, precision: 10);
        Assert.Equal(TimeSpan.Zero, profile.ConstantVelocityDuration);
        Assert.Equal(
            2 * Math.Sqrt(40) / 20,
            profile.TotalDuration.TotalSeconds,
            precision: 7);
    }

    [Fact]
    public void SampleAt_WhenAccelerating_ShouldReturnQuadraticDistanceAndIncreasingVelocity()
    {
        var profile = CreateTrapezoidalProfile();

        var sample = profile.SampleAt(TimeSpan.FromSeconds(0.25));

        Assert.Equal(MotionProfilePhase.Acceleration, sample.Phase);
        Assert.Equal(0.625, sample.Distance, precision: 10);
        Assert.Equal(5, sample.Velocity, precision: 10);
        Assert.Equal(20, sample.Acceleration);
    }

    [Fact]
    public void SampleAt_WhenAtConstantVelocity_ShouldReturnLinearDistance()
    {
        var profile = CreateTrapezoidalProfile();

        var sample = profile.SampleAt(TimeSpan.FromSeconds(1.5));

        Assert.Equal(MotionProfilePhase.ConstantVelocity, sample.Phase);
        Assert.Equal(12.5, sample.Distance, precision: 10);
        Assert.Equal(0.125, sample.Progress, precision: 10);
        Assert.Equal(10, sample.Velocity);
        Assert.Equal(0, sample.Acceleration);
    }

    [Fact]
    public void SampleAt_WhenDecelerating_ShouldReturnFallingVelocity()
    {
        var profile = CreateTrapezoidalProfile();

        var sample = profile.SampleAt(TimeSpan.FromSeconds(10.25));

        Assert.Equal(MotionProfilePhase.Deceleration, sample.Phase);
        Assert.Equal(99.375, sample.Distance, precision: 10);
        Assert.Equal(5, sample.Velocity, precision: 10);
        Assert.Equal(-20, sample.Acceleration);
    }

    [Fact]
    public void SampleAt_WhenTimeIsOutsideProfile_ShouldClampToProfileBounds()
    {
        var profile = CreateTrapezoidalProfile();

        var beforeStart = profile.SampleAt(TimeSpan.FromSeconds(-1));
        var afterEnd = profile.SampleAt(TimeSpan.FromSeconds(20));

        Assert.Equal(0, beforeStart.Distance);
        Assert.Equal(0, beforeStart.Progress);
        Assert.Equal(MotionProfilePhase.Acceleration, beforeStart.Phase);
        Assert.Equal(100, afterEnd.Distance);
        Assert.Equal(1, afterEnd.Progress);
        Assert.Equal(0, afterEnd.Velocity);
        Assert.Equal(MotionProfilePhase.Completed, afterEnd.Phase);
    }

    [Theory]
    [InlineData(0, 10, 20)]
    [InlineData(100, 0, 20)]
    [InlineData(100, 10, 0)]
    [InlineData(double.PositiveInfinity, 10, 20)]
    public void Constructor_WhenInputIsNotPositiveAndFinite_ShouldThrow(
        double distance,
        double maximumVelocity,
        double acceleration)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TrapezoidalMotionProfile(distance, maximumVelocity, acceleration));
    }

    private static TrapezoidalMotionProfile CreateTrapezoidalProfile() =>
        new(distance: 100, maximumVelocity: 10, acceleration: 20);
}
