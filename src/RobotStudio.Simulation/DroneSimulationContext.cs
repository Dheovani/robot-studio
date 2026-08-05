using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;

namespace RobotStudio.Simulation;

public sealed record DroneSimulationContext(
    DroneProfile RobotProfile,
    DronePose CurrentPose,
    RobotState State,
    TimeSpan ElapsedTime)
{
    public static DroneSimulationContext Create(
        DroneProfile robotProfile,
        DronePose currentPose)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);
        robotProfile.ValidatePosition(currentPose);

        return new DroneSimulationContext(
            robotProfile,
            currentPose,
            RobotStateTransitions.InitialState,
            TimeSpan.Zero);
    }
}
