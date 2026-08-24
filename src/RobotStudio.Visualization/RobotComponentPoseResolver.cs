using System.Numerics;

namespace RobotStudio.Visualization;

public static class RobotComponentPoseResolver
{
    public static IReadOnlyDictionary<RobotPartId, Matrix4x4> ResolveWorldTransforms(
        RobotVisualModelDefinition model,
        IEnumerable<RobotComponentPose> localPoses)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(localPoses);

        var posesByPart = localPoses.ToDictionary(pose => pose.PartId);
        var resolved = new Dictionary<RobotPartId, Matrix4x4>();
        foreach (var part in model.Parts)
        {
            Resolve(part.Id, model, posesByPart, resolved);
        }

        return resolved;
    }

    private static Matrix4x4 Resolve(
        RobotPartId partId,
        RobotVisualModelDefinition model,
        IReadOnlyDictionary<RobotPartId, RobotComponentPose> posesByPart,
        IDictionary<RobotPartId, Matrix4x4> resolved)
    {
        if (resolved.TryGetValue(partId, out var existing))
        {
            return existing;
        }

        var part = model.GetPart(partId);
        var pose = posesByPart.TryGetValue(partId, out var specifiedPose)
            ? specifiedPose
            : RobotComponentPose.Identity(partId);
        var local = Matrix4x4.CreateScale(pose.Scale) *
                    Matrix4x4.CreateFromQuaternion(pose.Rotation) *
                    Matrix4x4.CreateTranslation(pose.TranslationMillimeters);
        var world = part.ParentId is RobotPartId parentId
            ? local * Resolve(parentId, model, posesByPart, resolved)
            : local;

        resolved.Add(partId, world);
        return world;
    }
}
