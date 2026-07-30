using RobotStudio.Domain;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation;

public sealed record DifferentialDriveSimulationContext(
    DifferentialDriveProfile RobotProfile,
    DifferentialDrivePose CurrentPose,
    RobotState State,
    TimeSpan ElapsedTime)
{
    public static DifferentialDriveSimulationContext Create(
        DifferentialDriveProfile robotProfile,
        DifferentialDrivePose currentPose)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);
        robotProfile.ValidatePosition(currentPose);

        return new DifferentialDriveSimulationContext(
            robotProfile,
            currentPose,
            RobotStateTransitions.InitialState,
            TimeSpan.Zero);
    }
}
