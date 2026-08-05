using RobotStudio.Domain;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Simulation;

public sealed record DeltaSimulationContext(
    DeltaRobotProfile RobotProfile,
    DeltaActuatorPosition CurrentActuators,
    RobotState State,
    TimeSpan ElapsedTime)
{
    public static DeltaSimulationContext Create(
        DeltaRobotProfile robotProfile,
        DeltaActuatorPosition currentActuators)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);
        robotProfile.ValidatePosition(currentActuators);

        return new DeltaSimulationContext(
            robotProfile,
            currentActuators,
            RobotStateTransitions.InitialState,
            TimeSpan.Zero);
    }
}
