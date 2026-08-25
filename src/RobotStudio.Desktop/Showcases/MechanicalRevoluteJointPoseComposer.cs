using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class MechanicalRevoluteJointPoseComposer
{
    public static IReadOnlyList<RobotComponentPose> Compose(
        IEnumerable<RobotComponentPose> poses,
        IEnumerable<MechanicalRevoluteJointPivot> pivots)
    {
        ArgumentNullException.ThrowIfNull(poses);
        ArgumentNullException.ThrowIfNull(pivots);

        var pivotsByPart = pivots.ToDictionary(pivot => pivot.PartId);
        return poses.Select(pose => Compose(pose, pivotsByPart)).ToArray();
    }

    private static RobotComponentPose Compose(
        RobotComponentPose pose,
        IReadOnlyDictionary<RobotPartId, MechanicalRevoluteJointPivot> pivotsByPart)
    {
        if (!pivotsByPart.TryGetValue(pose.PartId, out var joint))
        {
            return pose;
        }

        var pivotCompensation = joint.PivotMillimeters -
                                Vector3.Transform(joint.PivotMillimeters, pose.Rotation);
        return pose with
        {
            TranslationMillimeters = pose.TranslationMillimeters + pivotCompensation
        };
    }
}
