namespace RobotStudio.Domain.Exceptions;

public sealed class InvalidRobotCommandException : InvalidOperationException
{
    public InvalidRobotCommandException(string message)
        : base(message)
    {
    }
}
