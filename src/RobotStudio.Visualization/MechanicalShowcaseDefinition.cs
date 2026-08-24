namespace RobotStudio.Visualization;

public sealed class MechanicalShowcaseDefinition
{
    public MechanicalShowcaseDefinition(
        RobotVisualModelDefinition model,
        IEnumerable<MechanicalDemonstrationDefinition> demonstrations)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(demonstrations);

        var demonstrationArray = demonstrations.ToArray();
        if (demonstrationArray.Length == 0)
        {
            throw new ArgumentException("A mechanical showcase must define a demonstration.", nameof(demonstrations));
        }

        var duplicate = demonstrationArray
            .GroupBy(demonstration => demonstration.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException(
                $"Mechanical demonstration id '{duplicate.Key}' is duplicated.",
                nameof(demonstrations));
        }

        var partIds = model.Parts.Select(part => part.Id).ToHashSet();
        var unknownPart = demonstrationArray
            .SelectMany(demonstration => demonstration.Keyframes)
            .SelectMany(keyframe => keyframe.ComponentPoses)
            .Select(pose => pose.PartId)
            .FirstOrDefault(partId => !partIds.Contains(partId));
        if (unknownPart != default)
        {
            throw new ArgumentException(
                $"A demonstration references unknown visual part '{unknownPart}'.",
                nameof(demonstrations));
        }

        Model = model;
        Demonstrations = Array.AsReadOnly(demonstrationArray);
    }

    public RobotVisualModelDefinition Model { get; }

    public IReadOnlyList<MechanicalDemonstrationDefinition> Demonstrations { get; }
}
