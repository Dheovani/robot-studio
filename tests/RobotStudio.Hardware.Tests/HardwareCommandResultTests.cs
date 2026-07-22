namespace RobotStudio.Hardware.Tests;

public sealed class HardwareCommandResultTests
{
    [Fact]
    public void Constructor_WhenResultIsValid_ShouldPreserveValues()
    {
        var commandId = Guid.NewGuid();

        var result = new HardwareCommandResult(
            commandId,
            HardwareCommandResultStatus.Completed,
            "Command completed.");

        Assert.Equal(commandId, result.CommandId);
        Assert.Equal(HardwareCommandResultStatus.Completed, result.Status);
        Assert.Equal("Command completed.", result.Message);
    }

    [Fact]
    public void Constructor_WhenCommandIdIsEmpty_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new HardwareCommandResult(Guid.Empty, HardwareCommandResultStatus.Failed, "Failed."));

        Assert.Contains("id cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WhenMessageIsBlank_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new HardwareCommandResult(Guid.NewGuid(), HardwareCommandResultStatus.Rejected, " "));
    }
}
