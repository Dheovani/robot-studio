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
}
