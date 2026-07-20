namespace RobotStudio.Domain.Exceptions;

public sealed class ImpossibleMovementException : InvalidOperationException
{
    public ImpossibleMovementException(string reason)
        : base($"Movement cannot be planned. {reason}")
    {
        Reason = reason;
    }

    public string Reason { get; }
}
