using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Mobile;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Domain.Commands;

public static class RobotCommandValidator
{
    public static void Validate(RobotCommand command, CartesianRobotProfile profile)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(profile);

        switch (command)
        {
            case HomeCommand:
            case ResetFaultCommand:
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

    public static void Validate(RobotCommand command, XYPlotterProfile profile)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(profile);

        switch (command)
        {
            case HomeCommand:
            case ResetFaultCommand:
                return;

            case WaitCommand:
                return;

            case MoveToCommand moveToCommand:
                if (Math.Abs(moveToCommand.TargetPosition.Z) > 0.000_001)
                {
                    throw new InvalidRobotCommandException(
                        "XY Plotter MOVE commands must stay on the drawing plane. " +
                        $"Invalid Z value: {moveToCommand.TargetPosition.Z:0.###} mm. " +
                        "Expected value: 0 mm.");
                }

                profile.ValidatePosition(new XYPlotterPosition(
                    moveToCommand.TargetPosition.X,
                    moveToCommand.TargetPosition.Y));
                return;

            default:
                throw new InvalidRobotCommandException($"Unsupported robot command type: {command.GetType().Name}.");
        }
    }

    public static void Validate(RobotCommand command, DifferentialDriveProfile profile)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(profile);

        switch (command)
        {
            case HomeCommand:
            case ResetFaultCommand:
                return;

            case WaitCommand:
                return;

            case DifferentialDriveMoveCommand moveCommand:
                profile.ValidatePosition(moveCommand.TargetPose);
                return;

            default:
                throw new InvalidRobotCommandException($"Unsupported robot command type: {command.GetType().Name}.");
        }
    }

    public static void Validate(RobotCommand command, ScaraRobotProfile profile)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(profile);

        switch (command)
        {
            case HomeCommand:
            case ResetFaultCommand:
                return;

            case WaitCommand:
                return;

            case ScaraMoveJointsCommand moveCommand:
                profile.ValidatePosition(moveCommand.TargetJoints);
                return;

            default:
                throw new InvalidRobotCommandException($"Unsupported robot command type: {command.GetType().Name}.");
        }
    }

    public static void Validate(RobotCommand command, SimpleArmRobotProfile profile)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(profile);

        switch (command)
        {
            case HomeCommand:
            case ResetFaultCommand:
                return;

            case WaitCommand:
                return;

            case SimpleArmMoveJointsCommand moveCommand:
                profile.ValidatePosition(moveCommand.TargetJoints);
                return;

            default:
                throw new InvalidRobotCommandException($"Unsupported robot command type: {command.GetType().Name}.");
        }
    }

    public static void Validate(RobotCommand command, IndustrialArmRobotProfile profile)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(profile);

        switch (command)
        {
            case HomeCommand:
            case ResetFaultCommand:
            case WaitCommand:
                return;

            case IndustrialArmMoveJointsCommand moveCommand:
                profile.ValidatePosition(moveCommand.TargetJoints);
                return;

            default:
                throw new InvalidRobotCommandException($"Unsupported robot command type: {command.GetType().Name}.");
        }
    }

    public static void Validate(RobotCommand command, DeltaRobotProfile profile)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(profile);

        switch (command)
        {
            case HomeCommand:
            case ResetFaultCommand:
                return;

            case WaitCommand:
                return;

            case DeltaMoveActuatorsCommand moveCommand:
                profile.ValidatePosition(moveCommand.TargetActuators);
                return;

            default:
                throw new InvalidRobotCommandException($"Unsupported robot command type: {command.GetType().Name}.");
        }
    }

    public static void Validate(RobotCommand command, DroneProfile profile)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(profile);

        switch (command)
        {
            case HomeCommand:
            case ResetFaultCommand:
                return;

            case WaitCommand:
                return;

            case DroneMoveCommand moveCommand:
                profile.ValidatePosition(moveCommand.TargetPose);
                return;

            default:
                throw new InvalidRobotCommandException($"Unsupported robot command type: {command.GetType().Name}.");
        }
    }
}
