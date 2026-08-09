namespace RobotStudio.Domain.Articulated;

public sealed record SimpleArmRobotProfile : IRobotProfile<SimpleArmJointPosition>
{
    public SimpleArmRobotProfile(
        double firstLinkLengthMillimeters,
        double secondLinkLengthMillimeters,
        double thirdLinkLengthMillimeters,
        double linkCollisionRadiusMillimeters,
        SimpleArmJoint baseJoint,
        SimpleArmJoint shoulderJoint,
        SimpleArmJoint elbowJoint)
    {
        ArgumentNullException.ThrowIfNull(baseJoint);
        ArgumentNullException.ThrowIfNull(shoulderJoint);
        ArgumentNullException.ThrowIfNull(elbowJoint);

        if (firstLinkLengthMillimeters <= 0)
        {
            throw new ArgumentException("First simple arm link length must be greater than zero.");
        }

        if (secondLinkLengthMillimeters <= 0)
        {
            throw new ArgumentException("Second simple arm link length must be greater than zero.");
        }

        if (thirdLinkLengthMillimeters <= 0)
        {
            throw new ArgumentException("Third simple arm link length must be greater than zero.");
        }

        if (!double.IsFinite(linkCollisionRadiusMillimeters) || linkCollisionRadiusMillimeters <= 0)
        {
            throw new ArgumentException("Simple arm link collision radius must be a finite number greater than zero.");
        }

        if (baseJoint.Id != SimpleArmJointId.Base)
        {
            throw new ArgumentException("The base joint descriptor must use the Base id.");
        }

        if (shoulderJoint.Id != SimpleArmJointId.Shoulder)
        {
            throw new ArgumentException("The shoulder joint descriptor must use the Shoulder id.");
        }

        if (elbowJoint.Id != SimpleArmJointId.Elbow)
        {
            throw new ArgumentException("The elbow joint descriptor must use the Elbow id.");
        }

        FirstLinkLengthMillimeters = firstLinkLengthMillimeters;
        SecondLinkLengthMillimeters = secondLinkLengthMillimeters;
        ThirdLinkLengthMillimeters = thirdLinkLengthMillimeters;
        LinkCollisionRadiusMillimeters = linkCollisionRadiusMillimeters;
        BaseJoint = baseJoint;
        ShoulderJoint = shoulderJoint;
        ElbowJoint = elbowJoint;
        Joints = [BaseJoint, ShoulderJoint, ElbowJoint];
    }

    public double FirstLinkLengthMillimeters { get; }

    public double SecondLinkLengthMillimeters { get; }

    public double ThirdLinkLengthMillimeters { get; }

    public double LinkCollisionRadiusMillimeters { get; }

    public SimpleArmJoint BaseJoint { get; }

    public SimpleArmJoint ShoulderJoint { get; }

    public SimpleArmJoint ElbowJoint { get; }

    public IReadOnlyList<SimpleArmJoint> Joints { get; }

    public void ValidatePosition(SimpleArmJointPosition position)
    {
        foreach (var joint in Joints)
        {
            joint.ValidateCoordinate(position.GetCoordinate(joint.Id));
        }
    }

    public SimpleArmJoint GetJoint(SimpleArmJointId joint) => joint switch
    {
        SimpleArmJointId.Base => BaseJoint,
        SimpleArmJointId.Shoulder => ShoulderJoint,
        SimpleArmJointId.Elbow => ElbowJoint,
        _ => throw new ArgumentOutOfRangeException(nameof(joint), joint, "Unknown simple arm joint.")
    };
}
