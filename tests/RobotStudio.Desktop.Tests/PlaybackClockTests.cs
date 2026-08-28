using RobotStudio.Desktop.Rendering;

namespace RobotStudio.Desktop.Tests;

public sealed class PlaybackClockTests
{
    [Fact]
    public void Advance_WhenTimeProgresses_ShouldScaleOnlyElapsedDelta()
    {
        var clock = new PlaybackClock();
        clock.Start(TimeSpan.FromSeconds(2), playbackSpeed: 2);

        var position = clock.Advance(TimeSpan.FromSeconds(0.5));

        Assert.Equal(TimeSpan.FromSeconds(3), position);
    }

    [Fact]
    public void ChangeSpeed_DuringPlayback_ShouldNotRescalePreviousElapsedTime()
    {
        var clock = new PlaybackClock();
        clock.Start(TimeSpan.Zero, playbackSpeed: 1);
        clock.Advance(TimeSpan.FromSeconds(1));

        clock.ChangeSpeed(playbackSpeed: 4, elapsed: TimeSpan.FromSeconds(1));
        var position = clock.Advance(TimeSpan.FromSeconds(1.5));

        Assert.Equal(TimeSpan.FromSeconds(3), position);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Start_WhenSpeedIsInvalid_ShouldThrow(double playbackSpeed)
    {
        var clock = new PlaybackClock();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            clock.Start(TimeSpan.Zero, playbackSpeed));
    }
}
