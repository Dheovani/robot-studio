namespace RobotStudio.Domain.Articulated;

public readonly record struct IndustrialArmJointPosition(
    double J1Degrees,
    double J2Degrees,
    double J3Degrees,
    double J4Degrees,
    double J5Degrees,
    double J6Degrees) : IRobotPosition
{
    public static IndustrialArmJointPosition Home => new(0, 0, 0, 0, 0, 0);

    public double GetCoordinate(IndustrialArmJointId joint) => joint switch
    {
        IndustrialArmJointId.J1Base => J1Degrees,
        IndustrialArmJointId.J2Shoulder => J2Degrees,
        IndustrialArmJointId.J3Elbow => J3Degrees,
        IndustrialArmJointId.J4WristRoll => J4Degrees,
        IndustrialArmJointId.J5WristPitch => J5Degrees,
        IndustrialArmJointId.J6ToolRoll => J6Degrees,
        _ => throw new ArgumentOutOfRangeException(nameof(joint), joint, "Unknown industrial arm joint.")
    };

    public double MaximumJointDeltaTo(IndustrialArmJointPosition other) =>
        new[]
        {
            Math.Abs(other.J1Degrees - J1Degrees),
            Math.Abs(other.J2Degrees - J2Degrees),
            Math.Abs(other.J3Degrees - J3Degrees),
            Math.Abs(other.J4Degrees - J4Degrees),
            Math.Abs(other.J5Degrees - J5Degrees),
            Math.Abs(other.J6Degrees - J6Degrees)
        }.Max();
}
