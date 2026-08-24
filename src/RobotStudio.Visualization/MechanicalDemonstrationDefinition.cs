namespace RobotStudio.Visualization;

public sealed class MechanicalDemonstrationDefinition
{
    public MechanicalDemonstrationDefinition(
        string id,
        string name,
        string description,
        TimeSpan duration,
        IEnumerable<MechanicalKeyframe> keyframes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(keyframes);
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Demonstration duration must be positive.");
        }

        var keyframeArray = keyframes.ToArray();
        if (keyframeArray.Length < 2)
        {
            throw new ArgumentException("A demonstration must contain at least two keyframes.", nameof(keyframes));
        }

        if (keyframeArray[0].Time != TimeSpan.Zero)
        {
            throw new ArgumentException("The first demonstration keyframe must start at zero.", nameof(keyframes));
        }

        for (var index = 1; index < keyframeArray.Length; index++)
        {
            if (keyframeArray[index].Time <= keyframeArray[index - 1].Time)
            {
                throw new ArgumentException("Demonstration keyframes must use strictly increasing times.", nameof(keyframes));
            }
        }

        if (keyframeArray[^1].Time > duration)
        {
            throw new ArgumentException("A demonstration keyframe cannot exceed its duration.", nameof(keyframes));
        }

        var expectedPartIds = keyframeArray[0].ComponentPoses
            .Select(pose => pose.PartId)
            .OrderBy(partId => partId.Value, StringComparer.Ordinal)
            .ToArray();
        foreach (var keyframe in keyframeArray.Skip(1))
        {
            var actualPartIds = keyframe.ComponentPoses
                .Select(pose => pose.PartId)
                .OrderBy(partId => partId.Value, StringComparer.Ordinal)
                .ToArray();
            if (!expectedPartIds.SequenceEqual(actualPartIds))
            {
                throw new ArgumentException(
                    "Every demonstration keyframe must define the same component poses.",
                    nameof(keyframes));
            }
        }

        Id = id.Trim();
        Name = name.Trim();
        Description = description.Trim();
        Duration = duration;
        Keyframes = Array.AsReadOnly(keyframeArray);
    }

    public string Id { get; }

    public string Name { get; }

    public string Description { get; }

    public TimeSpan Duration { get; }

    public IReadOnlyList<MechanicalKeyframe> Keyframes { get; }
}
