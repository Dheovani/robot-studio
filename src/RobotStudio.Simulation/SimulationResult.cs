namespace RobotStudio.Simulation;

public sealed record SimulationResult(
    SimulationContext InitialContext,
    SimulationContext FinalContext,
    IReadOnlyList<SimulationStep> Timeline,
    Exception? Failure)
{
    public bool Succeeded => Failure is null;
}
