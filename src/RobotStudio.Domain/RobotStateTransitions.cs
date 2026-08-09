using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain;

public static class RobotStateTransitions
{
    public const RobotState InitialState = RobotState.Idle;

    public static void EnsureCanTransitionTo(RobotState current, RobotState next)
    {
        if (!CanTransitionTo(current, next))
        {
            throw new InvalidRobotStateTransitionException(current, next);
        }
    }

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

    public static bool IsActive(RobotState state) =>
        state is RobotState.Moving or RobotState.Homing or RobotState.Waiting;

    public static bool IsRecoverable(RobotState state) =>
        state is RobotState.Faulted;

    public static void EnsureCanResetFault(RobotState state)
    {
        if (!IsRecoverable(state))
        {
            throw new InvalidRobotStateTransitionException(state, RobotState.Idle);
        }
    }

    public static bool IsReadyForCommand(RobotState state) =>
        state is RobotState.Idle or RobotState.Completed;

    public static bool IsTerminalForCurrentCommand(RobotState state) =>
        state is RobotState.Completed or RobotState.Faulted;
}
