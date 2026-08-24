using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public sealed record IndustrialArmLinearMoveCommand : RobotCommand
{
    public IndustrialArmLinearMoveCommand(
        IndustrialArmToolPose targetToolPose,
        double? requestedToolVelocityMillimetersPerSecond = null,
        IndustrialArmConfiguration configuration = IndustrialArmConfiguration.PositiveElbowWristNeutral,
        RobotCommandSource? source = null)
        : base(source)
    {
        if (requestedToolVelocityMillimetersPerSecond <= 0)
        {
            throw new InvalidRobotCommandException(
                "Industrial arm requested tool velocity must be greater than zero. " +
                $"Invalid value: {requestedToolVelocityMillimetersPerSecond:0.###} mm/s.");
        }

        TargetToolPose = targetToolPose;
        RequestedToolVelocityMillimetersPerSecond = requestedToolVelocityMillimetersPerSecond;
        Configuration = configuration;
    }

    public IndustrialArmToolPose TargetToolPose { get; }

    public double? RequestedToolVelocityMillimetersPerSecond { get; }

    public IndustrialArmConfiguration Configuration { get; }
}
