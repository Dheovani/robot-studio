namespace RobotStudio.Simulation;

public sealed record IndustrialArmSimulationResult(
    IndustrialArmSimulationContext InitialContext,
    IndustrialArmSimulationContext FinalContext,
    IReadOnlyList<IndustrialArmSimulationStep> Timeline,
    Exception? Failure)
{
    public bool Succeeded => Failure is null;
}
