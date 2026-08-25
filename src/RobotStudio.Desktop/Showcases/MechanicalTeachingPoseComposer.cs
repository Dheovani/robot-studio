using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class MechanicalTeachingPoseComposer
{
    public static IReadOnlyList<RobotComponentPose> Compose(
        RobotVisualModelDefinition model,
        IEnumerable<RobotComponentPose> demonstrationPoses,
        MechanicalTeachingViewMode mode,
        IEnumerable<MechanicalExplodedPartOffset> explodedOffsets)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(demonstrationPoses);
        ArgumentNullException.ThrowIfNull(explodedOffsets);

        var poseByPart = demonstrationPoses.ToDictionary(pose => pose.PartId);
        if (mode != MechanicalTeachingViewMode.ExplodedAssembly)
        {
            return poseByPart.Values.ToArray();
        }

        var offsetByPart = explodedOffsets.ToDictionary(offset => offset.PartId);
        return model.Parts
            .Select(part =>
            {
                var pose = poseByPart.GetValueOrDefault(part.Id, RobotComponentPose.Identity(part.Id));
                return pose with
                {
                    TranslationMillimeters = pose.TranslationMillimeters +
                                              (offsetByPart.GetValueOrDefault(part.Id)?.TranslationMillimeters ??
                                               System.Numerics.Vector3.Zero)
                };
            })
            .ToArray();
    }
}
