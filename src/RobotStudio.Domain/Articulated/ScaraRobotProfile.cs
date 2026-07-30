namespace RobotStudio.Domain.Articulated;

public sealed record ScaraRobotProfile : IRobotProfile<ScaraJointPosition>
{
    public ScaraRobotProfile(
        double firstLinkLengthMillimeters,
        double secondLinkLengthMillimeters,
        ScaraJoint shoulderJoint,
        ScaraJoint elbowJoint)
    {
        ArgumentNullException.ThrowIfNull(shoulderJoint);
        ArgumentNullException.ThrowIfNull(elbowJoint);

        if (firstLinkLengthMillimeters <= 0)
        {
            throw new ArgumentException("First SCARA link length must be greater than zero.");
        }

        if (secondLinkLengthMillimeters <= 0)
        {
            throw new ArgumentException("Second SCARA link length must be greater than zero.");
        }

        if (shoulderJoint.Id != ScaraJointId.Shoulder)
        {
            throw new ArgumentException("The shoulder joint descriptor must use the Shoulder id.");
        }

        if (elbowJoint.Id != ScaraJointId.Elbow)
        {
            throw new ArgumentException("The elbow joint descriptor must use the Elbow id.");
        }

        FirstLinkLengthMillimeters = firstLinkLengthMillimeters;
        SecondLinkLengthMillimeters = secondLinkLengthMillimeters;
        ShoulderJoint = shoulderJoint;
        ElbowJoint = elbowJoint;
        Joints = [ShoulderJoint, ElbowJoint];
    }

    public double FirstLinkLengthMillimeters { get; }

    public double SecondLinkLengthMillimeters { get; }

    public ScaraJoint ShoulderJoint { get; }

    public ScaraJoint ElbowJoint { get; }

    public IReadOnlyList<ScaraJoint> Joints { get; }

    public void ValidatePosition(ScaraJointPosition position)
    {
        foreach (var joint in Joints)
        {
            joint.ValidateCoordinate(position.GetCoordinate(joint.Id));
        }
    }

    public ScaraJoint GetJoint(ScaraJointId joint) => joint switch
    {
        ScaraJointId.Shoulder => ShoulderJoint,
        ScaraJointId.Elbow => ElbowJoint,
        _ => throw new ArgumentOutOfRangeException(nameof(joint), joint, "Unknown SCARA joint.")
    };
}
