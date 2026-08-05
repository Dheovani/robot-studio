namespace RobotStudio.Domain.Articulated;

public sealed record IndustrialArmRobotProfile : IRobotProfile<IndustrialArmJointPosition>
{
    public IndustrialArmRobotProfile(
        double baseHeightMillimeters,
        double upperArmLengthMillimeters,
        double forearmLengthMillimeters,
        double wristLengthMillimeters,
        IReadOnlyList<IndustrialArmJoint> joints)
    {
        if (baseHeightMillimeters <= 0 || upperArmLengthMillimeters <= 0 ||
            forearmLengthMillimeters <= 0 || wristLengthMillimeters <= 0)
        {
            throw new ArgumentException("Industrial arm dimensions must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(joints);

        var expectedIds = Enum.GetValues<IndustrialArmJointId>();
        if (joints.Count != expectedIds.Length ||
            expectedIds.Any(id => joints.Count(joint => joint.Id == id) != 1))
        {
            throw new ArgumentException("Industrial arm profile must define each joint J1 through J6 exactly once.");
        }

        BaseHeightMillimeters = baseHeightMillimeters;
        UpperArmLengthMillimeters = upperArmLengthMillimeters;
        ForearmLengthMillimeters = forearmLengthMillimeters;
        WristLengthMillimeters = wristLengthMillimeters;
        Joints = joints.ToArray();
    }

    public double BaseHeightMillimeters { get; }

    public double UpperArmLengthMillimeters { get; }

    public double ForearmLengthMillimeters { get; }

    public double WristLengthMillimeters { get; }

    public IReadOnlyList<IndustrialArmJoint> Joints { get; }

    public void ValidatePosition(IndustrialArmJointPosition position)
    {
        foreach (var joint in Joints)
        {
            joint.ValidateCoordinate(position.GetCoordinate(joint.Id));
        }
    }
}
