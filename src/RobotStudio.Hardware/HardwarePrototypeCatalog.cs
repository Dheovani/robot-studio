namespace RobotStudio.Hardware;

public static class HardwarePrototypeCatalog
{
    public static HardwarePrototypeDescriptor IntroductoryCartesianPrototype { get; } = new(
        "Introductory Cartesian Prototype",
        RobotHardwareTarget.Arduino,
        HardwareActuatorKind.StepperMotor,
        "Planned educational Cartesian robot prototype using an Arduino-compatible controller and stepper motors.",
        isImplemented: false);

    public static IReadOnlyList<HardwarePrototypeDescriptor> All { get; } =
    [
        IntroductoryCartesianPrototype
    ];
}
