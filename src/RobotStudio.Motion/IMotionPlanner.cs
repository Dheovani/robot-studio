using RobotStudio.Domain;

namespace RobotStudio.Motion;

public interface IMotionPlanner<TPosition, in TProfile>
    where TPosition : IRobotPosition
    where TProfile : IRobotProfile<TPosition>
{
    MotionPlan<TPosition> PlanMove(
        TPosition start,
        TPosition end,
        TProfile robotProfile,
        double? requestedVelocityMillimetersPerSecond = null);
}
