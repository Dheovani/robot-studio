using System.Numerics;

namespace RobotStudio.Visualization;

public readonly record struct RobotComponentPose(
    RobotPartId PartId,
    Vector3 TranslationMillimeters,
    Quaternion Rotation,
    Vector3 Scale)
{
    public static RobotComponentPose Identity(RobotPartId partId) =>
        new(partId, Vector3.Zero, Quaternion.Identity, Vector3.One);
}
