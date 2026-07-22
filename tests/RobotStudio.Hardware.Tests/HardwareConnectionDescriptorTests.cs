namespace RobotStudio.Hardware.Tests;

public sealed class HardwareConnectionDescriptorTests
{
    [Fact]
    public void Constructor_WhenDescriptorIsValid_ShouldPreserveValues()
    {
        var descriptor = new HardwareConnectionDescriptor(
            RobotHardwareTarget.Unknown,
            "Educational robot controller",
            "Serial");

        Assert.Equal(RobotHardwareTarget.Unknown, descriptor.Target);
        Assert.Equal("Educational robot controller", descriptor.DisplayName);
        Assert.Equal("Serial", descriptor.TransportName);
    }

    [Fact]
    public void Constructor_WhenDisplayNameIsBlank_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new HardwareConnectionDescriptor(RobotHardwareTarget.Unknown, " ", "Serial"));
    }

    [Fact]
    public void Constructor_WhenTransportNameIsBlank_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new HardwareConnectionDescriptor(RobotHardwareTarget.Unknown, "Controller", " "));
    }
}
