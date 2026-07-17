namespace RobotStudio.Domain;

public static class RobotStateTransitions
{
    public static bool CanTransitionTo(RobotState current, RobotState next)
    {
        return current switch
        {
            RobotState.Idle => next is RobotState.Moving or RobotState.Homing or RobotState.Waiting or RobotState.Faulted,
            RobotState.Moving => next is RobotState.Homing or RobotState.Completed or RobotState.Faulted,
            RobotState.Homing => next is RobotState.Homing or RobotState.Completed or RobotState.Faulted,
            RobotState.Waiting => next is RobotState.Homing or RobotState.Completed or RobotState.Faulted,
            RobotState.Completed => next is RobotState.Idle or RobotState.Moving or RobotState.Homing or RobotState.Waiting or RobotState.Faulted,
            RobotState.Faulted => next is RobotState.Idle or RobotState.Homing,
            _ => false
        };
    }
}
