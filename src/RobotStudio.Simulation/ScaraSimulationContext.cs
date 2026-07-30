using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public sealed record ScaraSimulationContext(
    ScaraRobotProfile RobotProfile,
    ScaraJointPosition CurrentJoints,
    RobotState State,
    TimeSpan ElapsedTime)
{
    public static ScaraSimulationContext Create(
        ScaraRobotProfile robotProfile,
        ScaraJointPosition currentJoints)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);
        robotProfile.ValidatePosition(currentJoints);

        return new ScaraSimulationContext(
            robotProfile,
            currentJoints,
            RobotStateTransitions.InitialState,
            TimeSpan.Zero);
    }
}
