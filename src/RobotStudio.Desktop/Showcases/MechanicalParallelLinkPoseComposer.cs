using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class MechanicalParallelLinkPoseComposer
{
    private const float LengthTolerance = 0.0001f;

    public static IReadOnlyList<RobotComponentPose> Compose(
        RobotVisualModelDefinition model,
        IEnumerable<RobotComponentPose> poses,
        IEnumerable<MechanicalParallelLinkConstraint> constraints)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(poses);
        ArgumentNullException.ThrowIfNull(constraints);

        var poseArray = poses.ToArray();
        var constraintArray = constraints.ToArray();
        if (constraintArray.Length == 0)
        {
            return poseArray;
        }

        var posesByPart = poseArray.ToDictionary(pose => pose.PartId);
        var transforms = RobotComponentPoseResolver.ResolveWorldTransforms(model, poseArray);
        foreach (var constraint in constraintArray)
        {
            var start = Vector3.Transform(
                constraint.AuthoredStartMillimeters,
                transforms[constraint.StartPartId]);
            var end = Vector3.Transform(
                constraint.AuthoredEndMillimeters,
                transforms[constraint.EndPartId]);
            var worldTransform = CreateSegmentTransform(
                constraint.AuthoredStartMillimeters,
                constraint.AuthoredEndMillimeters,
                start,
                end);
            var parentTransform = model.GetPart(constraint.LinkPartId).ParentId is RobotPartId parentId
                ? transforms[parentId]
                : Matrix4x4.Identity;

            if (!Matrix4x4.Invert(parentTransform, out var inverseParent) ||
                !Matrix4x4.Decompose(
                    worldTransform * inverseParent,
                    out var scale,
                    out var rotation,
                    out var translation))
            {
                throw new InvalidOperationException(
                    $"Parallel link '{constraint.LinkPartId}' produced a non-decomposable transform.");
            }

            posesByPart[constraint.LinkPartId] = new RobotComponentPose(
                constraint.LinkPartId,
                translation,
                Quaternion.Normalize(rotation),
                scale);
        }

        return model.Parts
            .Select(part => posesByPart.GetValueOrDefault(part.Id, RobotComponentPose.Identity(part.Id)))
            .ToArray();
    }

    private static Matrix4x4 CreateSegmentTransform(
        Vector3 authoredStart,
        Vector3 authoredEnd,
        Vector3 currentStart,
        Vector3 currentEnd)
    {
        var authoredDirection = authoredEnd - authoredStart;
        var currentDirection = currentEnd - currentStart;
        var authoredLength = authoredDirection.Length();
        var currentLength = currentDirection.Length();
        if (authoredLength <= LengthTolerance || currentLength <= LengthTolerance)
        {
            throw new InvalidOperationException("Parallel link endpoints must remain measurably separated.");
        }

        var scale = currentLength / authoredLength;
        var rotation = RotationBetween(authoredDirection / authoredLength, currentDirection / currentLength);
        var scaleAndRotation = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation);
        var translation = currentStart - Vector3.Transform(authoredStart, scaleAndRotation);
        return scaleAndRotation * Matrix4x4.CreateTranslation(translation);
    }

    private static Quaternion RotationBetween(Vector3 from, Vector3 to)
    {
        var dot = Math.Clamp(Vector3.Dot(from, to), -1f, 1f);
        if (dot >= 1f - LengthTolerance)
        {
            return Quaternion.Identity;
        }

        if (dot <= -1f + LengthTolerance)
        {
            var axis = Vector3.Cross(from, Vector3.UnitX);
            if (axis.LengthSquared() <= LengthTolerance)
            {
                axis = Vector3.Cross(from, Vector3.UnitY);
            }

            return Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), MathF.PI);
        }

        var cross = Vector3.Cross(from, to);
        return Quaternion.Normalize(new Quaternion(cross, 1f + dot));
    }
}
