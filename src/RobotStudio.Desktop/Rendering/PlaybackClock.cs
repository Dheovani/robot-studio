namespace RobotStudio.Desktop.Rendering;

internal sealed class PlaybackClock
{
    private TimeSpan lastElapsed;
    private double speed = 1;

    public TimeSpan Position { get; private set; }

    public void Start(TimeSpan position, double playbackSpeed)
    {
        Position = position;
        lastElapsed = TimeSpan.Zero;
        speed = ValidateSpeed(playbackSpeed);
    }

    public TimeSpan Advance(TimeSpan elapsed)
    {
        if (elapsed < lastElapsed)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsed),
                elapsed,
                "Elapsed playback time cannot move backward.");
        }

        var delta = elapsed - lastElapsed;
        Position += TimeSpan.FromSeconds(delta.TotalSeconds * speed);
        lastElapsed = elapsed;
        return Position;
    }

    public void ChangeSpeed(double playbackSpeed, TimeSpan elapsed)
    {
        Advance(elapsed);
        speed = ValidateSpeed(playbackSpeed);
    }

    public void Reset()
    {
        Position = TimeSpan.Zero;
        lastElapsed = TimeSpan.Zero;
        speed = 1;
    }

    private static double ValidateSpeed(double playbackSpeed)
    {
        if (!double.IsFinite(playbackSpeed) || playbackSpeed <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(playbackSpeed),
                playbackSpeed,
                "Playback speed must be finite and greater than zero.");
        }

        return playbackSpeed;
    }
}
