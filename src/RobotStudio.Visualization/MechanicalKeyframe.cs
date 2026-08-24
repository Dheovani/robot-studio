namespace RobotStudio.Visualization;

public sealed class MechanicalKeyframe
{
    public MechanicalKeyframe(
        TimeSpan time,
        IEnumerable<RobotComponentPose> componentPoses)
    {
        ArgumentNullException.ThrowIfNull(componentPoses);
        if (time < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(time), time, "Keyframe time cannot be negative.");
        }

        var poseArray = componentPoses.ToArray();
        var duplicate = poseArray
            .GroupBy(pose => pose.PartId)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException(
                $"Keyframe contains more than one pose for part '{duplicate.Key}'.",
                nameof(componentPoses));
        }

        Time = time;
        ComponentPoses = Array.AsReadOnly(poseArray);
    }

    public TimeSpan Time { get; }

    public IReadOnlyList<RobotComponentPose> ComponentPoses { get; }
}
