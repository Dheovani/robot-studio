using RobotStudio.Domain.Commands;

namespace RobotStudio.Hardware.Tests;

public sealed class HardwareCommandEnvelopeTests
{
    [Fact]
    public void Constructor_WhenCommandIsValid_ShouldPreserveCommandAndTimeout()
    {
        var command = new HomeCommand();
        var timeout = TimeSpan.FromSeconds(2);

        var envelope = HardwareCommandEnvelope.Create(command, timeout);

        Assert.NotEqual(Guid.Empty, envelope.CommandId);
        Assert.Same(command, envelope.Command);
        Assert.Equal(timeout, envelope.Timeout);
    }

    [Fact]
    public void Constructor_WhenCommandIdIsEmpty_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new HardwareCommandEnvelope(Guid.Empty, new HomeCommand(), TimeSpan.FromSeconds(1)));

        Assert.Contains("id cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WhenCommandIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new HardwareCommandEnvelope(Guid.NewGuid(), null!, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Constructor_WhenTimeoutIsZero_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HardwareCommandEnvelope(Guid.NewGuid(), new HomeCommand(), TimeSpan.Zero));

        Assert.Contains("timeout must be greater than zero", exception.Message);
    }
}
