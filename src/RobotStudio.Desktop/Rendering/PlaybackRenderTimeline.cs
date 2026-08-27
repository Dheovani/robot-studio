namespace RobotStudio.Desktop.Rendering;

internal sealed class PlaybackRenderTimeline
{
    private readonly TimeSpan[] frameTimes;

    public PlaybackRenderTimeline(IEnumerable<TimeSpan> frameTimes)
    {
        ArgumentNullException.ThrowIfNull(frameTimes);
        this.frameTimes = frameTimes.ToArray();
        if (this.frameTimes.Length == 0)
        {
            throw new ArgumentException("A render timeline must contain at least one frame.", nameof(frameTimes));
        }

        if (this.frameTimes[0] < TimeSpan.Zero)
        {
            throw new ArgumentException("Render frame times cannot be negative.", nameof(frameTimes));
        }

        for (var index = 1; index < this.frameTimes.Length; index++)
        {
            if (this.frameTimes[index] < this.frameTimes[index - 1])
            {
                throw new ArgumentException("Render frame times must be ordered.", nameof(frameTimes));
            }
        }
    }

    public TimeSpan Duration => frameTimes[^1];

    public PlaybackFrameSelection Select(TimeSpan requestedPosition, bool loop)
    {
        var position = Normalize(requestedPosition, loop);
        var upperIndex = Array.BinarySearch(frameTimes, position);
        if (upperIndex >= 0)
        {
            while (upperIndex + 1 < frameTimes.Length && frameTimes[upperIndex + 1] == position)
            {
                upperIndex++;
            }

            return new PlaybackFrameSelection(upperIndex, upperIndex, 0, position);
        }

        upperIndex = ~upperIndex;
        if (upperIndex == 0)
        {
            return new PlaybackFrameSelection(0, 0, 0, position);
        }

        if (upperIndex >= frameTimes.Length)
        {
            var finalIndex = frameTimes.Length - 1;
            return new PlaybackFrameSelection(finalIndex, finalIndex, 0, position);
        }

        var lowerIndex = upperIndex - 1;
        var interval = frameTimes[upperIndex] - frameTimes[lowerIndex];
        var progress = interval == TimeSpan.Zero
            ? 1
            : (position - frameTimes[lowerIndex]).TotalSeconds / interval.TotalSeconds;
        return new PlaybackFrameSelection(lowerIndex, upperIndex, progress, position);
    }

    private TimeSpan Normalize(TimeSpan requestedPosition, bool loop)
    {
        if (requestedPosition <= TimeSpan.Zero || Duration == TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        if (!loop || requestedPosition <= Duration)
        {
            return requestedPosition > Duration ? Duration : requestedPosition;
        }

        return TimeSpan.FromTicks(requestedPosition.Ticks % Duration.Ticks);
    }
}

internal readonly record struct PlaybackFrameSelection(
    int LowerFrameIndex,
    int UpperFrameIndex,
    double InterpolationProgress,
    TimeSpan Position)
{
    public int NearestFrameIndex =>
        InterpolationProgress < 0.5 ? LowerFrameIndex : UpperFrameIndex;
}
