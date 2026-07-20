using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public static class RobotCommandValidator
{
    public static void Validate(RobotCommand command, RobotProfile profile)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(profile);

        switch (command)
        {
            case HomeCommand:
                return;

            case WaitCommand:
                return;

            case MoveToCommand moveToCommand:
                profile.ValidatePosition(moveToCommand.TargetPosition);
                return;

            default:
                throw new InvalidRobotCommandException($"Unsupported robot command type: {command.GetType().Name}.");
        }
    }
}
