namespace RobotStudio.Domain.Articulated;

public readonly record struct SimpleArmJointPosition(
    double BaseDegrees,
    double ShoulderDegrees,
    double ElbowDegrees) : IRobotPosition
{
    public double GetCoordinate(SimpleArmJointId joint) => joint switch
    {
        SimpleArmJointId.Base => BaseDegrees,
        SimpleArmJointId.Shoulder => ShoulderDegrees,
        SimpleArmJointId.Elbow => ElbowDegrees,
        _ => throw new ArgumentOutOfRangeException(nameof(joint), joint, "Unknown simple arm joint.")
    };

    public double MaximumJointDeltaTo(SimpleArmJointPosition other) =>
        Math.Max(
            Math.Abs(other.BaseDegrees - BaseDegrees),
            Math.Max(
                Math.Abs(other.ShoulderDegrees - ShoulderDegrees),
                Math.Abs(other.ElbowDegrees - ElbowDegrees)));
}
