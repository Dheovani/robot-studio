namespace RobotStudio.Visualization.Assets;

public sealed class RobotVisualAssetException : Exception
{
    public RobotVisualAssetException(
        RobotVisualAssetErrorCode code,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public RobotVisualAssetErrorCode Code { get; }
}
