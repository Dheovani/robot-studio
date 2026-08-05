using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public sealed record IndustrialArmSimulationContext(
    IndustrialArmRobotProfile RobotProfile,
    IndustrialArmJointPosition CurrentJoints,
    RobotState State,
    TimeSpan ElapsedTime)
{
    public static IndustrialArmSimulationContext Create(
        IndustrialArmRobotProfile robotProfile,
        IndustrialArmJointPosition currentJoints)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);
        robotProfile.ValidatePosition(currentJoints);

        return new IndustrialArmSimulationContext(
            robotProfile,
            currentJoints,
            RobotState.Idle,
            TimeSpan.Zero);
    }
}
