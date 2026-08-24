using System.Numerics;

namespace RobotStudio.Visualization;

public static class MechanicalDemonstrationSampler
{
    public static IReadOnlyList<RobotComponentPose> Sample(
        MechanicalDemonstrationDefinition demonstration,
        TimeSpan time)
    {
        ArgumentNullException.ThrowIfNull(demonstration);

        var clampedTime = time < TimeSpan.Zero
            ? TimeSpan.Zero
            : time > demonstration.Duration
                ? demonstration.Duration
                : time;
        var keyframes = demonstration.Keyframes;
        if (clampedTime <= keyframes[0].Time)
        {
            return keyframes[0].ComponentPoses;
        }

        for (var index = 1; index < keyframes.Count; index++)
        {
            var end = keyframes[index];
            if (clampedTime > end.Time)
            {
                continue;
            }

            var start = keyframes[index - 1];
            var interval = end.Time - start.Time;
            var progress = interval == TimeSpan.Zero
                ? 1f
                : (float)((clampedTime - start.Time).TotalSeconds / interval.TotalSeconds);
            var endByPart = end.ComponentPoses.ToDictionary(pose => pose.PartId);

            return start.ComponentPoses
                .Select(startPose => Interpolate(startPose, endByPart[startPose.PartId], progress))
                .ToArray();
        }

        return keyframes[^1].ComponentPoses;
    }

    private static RobotComponentPose Interpolate(
        RobotComponentPose start,
        RobotComponentPose end,
        float progress) =>
        new(
            start.PartId,
            Vector3.Lerp(start.TranslationMillimeters, end.TranslationMillimeters, progress),
            Quaternion.Slerp(start.Rotation, end.Rotation, progress),
            Vector3.Lerp(start.Scale, end.Scale, progress));
}
