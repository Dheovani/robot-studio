namespace RobotStudio.Simulation;

public sealed record DifferentialDriveSimulationResult(
    DifferentialDriveSimulationContext InitialContext,
    DifferentialDriveSimulationContext FinalContext,
    IReadOnlyList<DifferentialDriveSimulationStep> Timeline,
    Exception? Failure)
{
    public bool Succeeded => Failure is null;
}
