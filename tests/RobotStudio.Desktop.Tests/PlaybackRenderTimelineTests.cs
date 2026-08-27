using RobotStudio.Desktop.Rendering;

namespace RobotStudio.Desktop.Tests;

public sealed class PlaybackRenderTimelineTests
{
    [Fact]
    public void Select_WhenPositionFallsBetweenSamples_ShouldExposeInterpolationAndNearestFrame()
    {
        var timeline = CreateTimeline();

        var selection = timeline.Select(TimeSpan.FromMilliseconds(160), loop: false);

        Assert.Equal(1, selection.LowerFrameIndex);
        Assert.Equal(2, selection.UpperFrameIndex);
        Assert.Equal(0.6, selection.InterpolationProgress, precision: 6);
        Assert.Equal(2, selection.NearestFrameIndex);
    }

    [Fact]
    public void Select_WhenPositionMatchesSample_ShouldSelectExactFrame()
    {
        var selection = CreateTimeline().Select(TimeSpan.FromMilliseconds(100), loop: false);

        Assert.Equal(1, selection.LowerFrameIndex);
        Assert.Equal(1, selection.UpperFrameIndex);
        Assert.Equal(0, selection.InterpolationProgress);
    }

    [Fact]
    public void Select_WhenPositionExceedsDurationWithoutLoop_ShouldClampToFinalFrame()
    {
        var selection = CreateTimeline().Select(TimeSpan.FromSeconds(2), loop: false);

        Assert.Equal(2, selection.NearestFrameIndex);
        Assert.Equal(TimeSpan.FromMilliseconds(200), selection.Position);
    }

    [Fact]
    public void Select_WhenPositionExceedsDurationWithLoop_ShouldWrapBySimulatedTime()
    {
        var selection = CreateTimeline().Select(TimeSpan.FromMilliseconds(250), loop: true);

        Assert.Equal(TimeSpan.FromMilliseconds(50), selection.Position);
        Assert.Equal(1, selection.UpperFrameIndex);
    }

    [Fact]
    public void Constructor_WhenFrameTimesAreUnordered_ShouldRejectTimeline()
    {
        Assert.Throws<ArgumentException>(() => new PlaybackRenderTimeline(
        [
            TimeSpan.FromMilliseconds(100),
            TimeSpan.Zero
        ]));
    }

    [Fact]
    public void Constructor_WhenTimelineIsEmpty_ShouldRejectTimeline()
    {
        Assert.Throws<ArgumentException>(() => new PlaybackRenderTimeline([]));
    }

    private static PlaybackRenderTimeline CreateTimeline() =>
        new(
        [
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(200)
        ]);
}
