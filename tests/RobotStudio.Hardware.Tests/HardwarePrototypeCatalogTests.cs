namespace RobotStudio.Hardware.Tests;

public sealed class HardwarePrototypeCatalogTests
{
    [Fact]
    public void All_ShouldExposeIntroductoryCartesianPrototype()
    {
        var prototype = Assert.Single(HardwarePrototypeCatalog.All);

        Assert.Equal(HardwarePrototypeCatalog.IntroductoryCartesianPrototype, prototype);
    }

    [Fact]
    public void IntroductoryCartesianPrototype_ShouldUseArduinoCompatibleTarget()
    {
        Assert.Equal(
            RobotHardwareTarget.Arduino,
            HardwarePrototypeCatalog.IntroductoryCartesianPrototype.Target);
    }

    [Fact]
    public void IntroductoryCartesianPrototype_ShouldUseStepperMotors()
    {
        Assert.Equal(
            HardwareActuatorKind.StepperMotor,
            HardwarePrototypeCatalog.IntroductoryCartesianPrototype.ActuatorKind);
    }

    [Fact]
    public void IntroductoryCartesianPrototype_ShouldRemainPlannedOnly()
    {
        Assert.False(HardwarePrototypeCatalog.IntroductoryCartesianPrototype.IsImplemented);
    }

    [Fact]
    public void Constructor_WhenNameIsBlank_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new HardwarePrototypeDescriptor(
                " ",
                RobotHardwareTarget.Arduino,
                HardwareActuatorKind.StepperMotor,
                "Description.",
                isImplemented: false));
    }

    [Fact]
    public void Constructor_WhenDescriptionIsBlank_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new HardwarePrototypeDescriptor(
                "Prototype",
                RobotHardwareTarget.Arduino,
                HardwareActuatorKind.StepperMotor,
                " ",
                isImplemented: false));
    }
}
