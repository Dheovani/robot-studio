namespace RobotStudio.Simulation;

public sealed record ScaraSimulationResult(
    ScaraSimulationContext InitialContext,
    ScaraSimulationContext FinalContext,
    IReadOnlyList<ScaraSimulationStep> Timeline,
    Exception? Failure)
{
    public bool Succeeded => Failure is null;
}
