namespace RobotStudio.Domain.Exceptions;

public sealed class InvalidRobotStateTransitionException : InvalidOperationException
{
    public InvalidRobotStateTransitionException(RobotState current, RobotState next)
        : base($"Cannot transition robot state from {current} to {next}.")
    {
        Current = current;
        Next = next;
    }

    public RobotState Current { get; }

    public RobotState Next { get; }
}
