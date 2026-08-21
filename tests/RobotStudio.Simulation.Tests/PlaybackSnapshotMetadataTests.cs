namespace RobotStudio.Simulation.Tests;

public sealed class PlaybackSnapshotMetadataTests
{
    [Fact]
    public void CreateCartesian_ShouldCreateVersionedCartesianMetadata()
    {
        var metadata = PlaybackSnapshotMetadata.CreateCartesian(TimeSpan.FromMilliseconds(500));

        Assert.Equal(3, metadata.FormatVersion);
        Assert.Equal("Cartesian", metadata.RobotFamily);
        Assert.Equal("Millimeters", metadata.DistanceUnit);
        Assert.Equal("Seconds", metadata.TimeUnit);
        Assert.Equal(500, metadata.SampleIntervalMilliseconds);
    }

    [Fact]
    public void CreateCartesian_WhenSampleIntervalIsZero_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlaybackSnapshotMetadata.CreateCartesian(TimeSpan.Zero));
    }

    [Fact]
    public void CreateCartesian_WhenSampleIntervalIsNegative_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlaybackSnapshotMetadata.CreateCartesian(TimeSpan.FromMilliseconds(-1)));
    }
}
