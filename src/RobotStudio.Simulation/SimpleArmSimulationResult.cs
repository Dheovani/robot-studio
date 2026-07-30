namespace RobotStudio.Simulation;

public sealed record SimpleArmSimulationResult(
    SimpleArmSimulationContext InitialContext,
    SimpleArmSimulationContext FinalContext,
    IReadOnlyList<SimpleArmSimulationStep> Timeline,
    Exception? Failure)
{
    public bool Succeeded => Failure is null;
}
