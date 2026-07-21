namespace RobotStudio.Simulation;

public sealed record PlaybackSnapshotMetadata(
    int FormatVersion,
    string RobotFamily,
    string DistanceUnit,
    string TimeUnit,
    double SampleIntervalMilliseconds)
{
    public static PlaybackSnapshotMetadata CreateCartesian(TimeSpan sampleInterval)
    {
        if (sampleInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleInterval),
                "Snapshot sample interval must be greater than zero.");
        }

        return new PlaybackSnapshotMetadata(
            FormatVersion: 1,
            RobotFamily: "Cartesian",
            DistanceUnit: "Millimeters",
            TimeUnit: "Seconds",
            SampleIntervalMilliseconds: sampleInterval.TotalMilliseconds);
    }
}
