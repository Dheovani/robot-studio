namespace RobotStudio.Domain.Parallel;

public sealed class DeltaKinematics
{
    private const double SqrtThree = 1.732_050_807_568_877_2;

    public DeltaToolPose Forward(
        DeltaRobotProfile profile,
        DeltaActuatorPosition position)
    {
        ArgumentNullException.ThrowIfNull(profile);

        profile.ValidatePosition(position);

        var average = (position.AMillimeters + position.BMillimeters + position.CMillimeters) / 3;
        var x = (position.BMillimeters - position.CMillimeters) / SqrtThree;
        var y = position.AMillimeters - ((position.BMillimeters + position.CMillimeters) / 2);
        var z = profile.ToolZOffsetMillimeters - average;

        return new DeltaToolPose(x, y, z);
    }

    public DeltaActuatorPosition Inverse(
        DeltaRobotProfile profile,
        DeltaToolPose pose)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (!double.IsFinite(pose.XMillimeters) ||
            !double.IsFinite(pose.YMillimeters) ||
            !double.IsFinite(pose.ZMillimeters))
        {
            throw new ArgumentException(
                "Delta target tool coordinates must be finite values.",
                nameof(pose));
        }

        var averageActuatorPosition = profile.ToolZOffsetMillimeters - pose.ZMillimeters;
        var actuators = new DeltaActuatorPosition(
            averageActuatorPosition + ((2d / 3d) * pose.YMillimeters),
            averageActuatorPosition - (pose.YMillimeters / 3d) +
                ((SqrtThree / 2d) * pose.XMillimeters),
            averageActuatorPosition - (pose.YMillimeters / 3d) -
                ((SqrtThree / 2d) * pose.XMillimeters));

        profile.ValidatePosition(actuators);
        return actuators;
    }
}
