using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public sealed record SimpleArmSimulationContext(
    SimpleArmRobotProfile RobotProfile,
    SimpleArmJointPosition CurrentJoints,
    RobotState State,
    TimeSpan ElapsedTime)
{
    public static SimpleArmSimulationContext Create(
        SimpleArmRobotProfile robotProfile,
        SimpleArmJointPosition currentJoints)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);
        robotProfile.ValidatePosition(currentJoints);

        return new SimpleArmSimulationContext(
            robotProfile,
            currentJoints,
            RobotState.Idle,
            TimeSpan.Zero);
    }
}
