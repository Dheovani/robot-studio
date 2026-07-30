namespace RobotStudio.Domain.Articulated;

public readonly record struct ScaraJointPosition(
    double ShoulderDegrees,
    double ElbowDegrees) : IRobotPosition
{
    public double GetCoordinate(ScaraJointId joint) => joint switch
    {
        ScaraJointId.Shoulder => ShoulderDegrees,
        ScaraJointId.Elbow => ElbowDegrees,
        _ => throw new ArgumentOutOfRangeException(nameof(joint), joint, "Unknown SCARA joint.")
    };

    public double MaximumJointDeltaTo(ScaraJointPosition other) =>
        Math.Max(
            Math.Abs(other.ShoulderDegrees - ShoulderDegrees),
            Math.Abs(other.ElbowDegrees - ElbowDegrees));
}
