using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class MechanicalTeachingPoseComposer
{
    public static IReadOnlyList<RobotComponentPose> Compose(
        RobotVisualModelDefinition model,
        IEnumerable<RobotComponentPose> demonstrationPoses,
        MechanicalTeachingViewMode mode)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(demonstrationPoses);

        var poseByPart = demonstrationPoses.ToDictionary(pose => pose.PartId);
        if (mode != MechanicalTeachingViewMode.ExplodedAssembly)
        {
            return poseByPart.Values.ToArray();
        }

        return model.Parts
            .Select(part =>
            {
                var pose = poseByPart.GetValueOrDefault(part.Id, RobotComponentPose.Identity(part.Id));
                return pose with
                {
                    TranslationMillimeters = pose.TranslationMillimeters +
                                              MechanicalTeachingViewCatalog.GetExplodedOffset(part.Id)
                };
            })
            .ToArray();
    }
}
