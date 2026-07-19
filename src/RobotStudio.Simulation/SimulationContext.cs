using RobotStudio.Domain;

namespace RobotStudio.Simulation;

public sealed record SimulationContext(
    RobotProfile RobotProfile,
    CartesianPosition CurrentPosition,
    RobotState State,
    TimeSpan ElapsedTime)
{
    public static SimulationContext Create(
        RobotProfile robotProfile,
        CartesianPosition currentPosition)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);
        robotProfile.ValidatePosition(currentPosition);

        return new SimulationContext(
            robotProfile,
            currentPosition,
            RobotState.Idle,
            TimeSpan.Zero);
    }
}
