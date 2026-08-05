namespace RobotStudio.Simulation;

public sealed record DeltaSimulationResult(
    DeltaSimulationContext InitialContext,
    DeltaSimulationContext FinalContext,
    IReadOnlyList<DeltaSimulationStep> Timeline,
    Exception? Failure)
{
    public bool Succeeded => Failure is null;
}
