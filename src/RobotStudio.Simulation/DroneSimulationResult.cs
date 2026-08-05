namespace RobotStudio.Simulation;

public sealed record DroneSimulationResult(
    DroneSimulationContext InitialContext,
    DroneSimulationContext FinalContext,
    IReadOnlyList<DroneSimulationStep> Timeline,
    Exception? Failure)
{
    public bool Succeeded => Failure is null;
}
