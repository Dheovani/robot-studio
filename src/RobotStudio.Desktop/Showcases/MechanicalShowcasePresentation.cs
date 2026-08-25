using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal sealed class MechanicalShowcasePresentation
{
    public MechanicalShowcasePresentation(
        string modelId,
        string title,
        string subtitle,
        string assetDirectoryName,
        MechanicalShowcaseDefinition showcase,
        RobotPartId initiallySelectedPartId,
        IEnumerable<MechanicalTeachingViewOption> viewOptions,
        IEnumerable<MechanicalMotionAxisGuide> motionAxes,
        IEnumerable<MechanicalExplodedPartOffset> explodedOffsets,
        IEnumerable<MechanicalScenePrimitive> fallbackPrimitives)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(subtitle);
        ArgumentException.ThrowIfNullOrWhiteSpace(assetDirectoryName);
        ArgumentNullException.ThrowIfNull(showcase);
        ArgumentNullException.ThrowIfNull(viewOptions);
        ArgumentNullException.ThrowIfNull(motionAxes);
        ArgumentNullException.ThrowIfNull(explodedOffsets);
        ArgumentNullException.ThrowIfNull(fallbackPrimitives);

        if (assetDirectoryName.IndexOfAny(['/', '\\']) >= 0 || assetDirectoryName is "." or "..")
        {
            throw new ArgumentException(
                "The asset directory name must be one local directory segment.",
                nameof(assetDirectoryName));
        }

        var partIds = showcase.Model.Parts.Select(part => part.Id).ToHashSet();
        if (!string.Equals(modelId, showcase.Model.Id, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The presentation model id must match the visual-model definition.",
                nameof(modelId));
        }

        if (!partIds.Contains(initiallySelectedPartId))
        {
            throw new ArgumentException(
                $"Initially selected part '{initiallySelectedPartId}' does not exist in the visual model.",
                nameof(initiallySelectedPartId));
        }

        var optionArray = viewOptions.ToArray();
        if (optionArray.Length == 0 ||
            optionArray.Select(option => option.Mode).Distinct().Count() != optionArray.Length ||
            optionArray.Any(option => option.DemonstrationIds.Count == 0))
        {
            throw new ArgumentException(
                "A presentation must define at least one uniquely identified view layer.",
                nameof(viewOptions));
        }

        var demonstrationIds = showcase.Demonstrations
            .Select(demonstration => demonstration.Id)
            .ToHashSet(StringComparer.Ordinal);
        var unknownDemonstrationId = optionArray
            .SelectMany(option => option.DemonstrationIds)
            .FirstOrDefault(id => !demonstrationIds.Contains(id));
        if (unknownDemonstrationId is not null)
        {
            throw new ArgumentException(
                $"View layer references unknown demonstration '{unknownDemonstrationId}'.",
                nameof(viewOptions));
        }

        var unassignedDemonstrationId = demonstrationIds.FirstOrDefault(id =>
            optionArray.All(option => !option.DemonstrationIds.Contains(id, StringComparer.Ordinal)));
        if (unassignedDemonstrationId is not null)
        {
            throw new ArgumentException(
                $"Demonstration '{unassignedDemonstrationId}' is not assigned to a view layer.",
                nameof(viewOptions));
        }

        var axisArray = motionAxes.ToArray();
        var unknownAxisPart = axisArray
            .Where(axis => axis.AttachedPartId is not null)
            .Select(axis => axis.AttachedPartId!.Value)
            .FirstOrDefault(partId => !partIds.Contains(partId));
        if (unknownAxisPart != default)
        {
            throw new ArgumentException(
                $"Motion-axis guide references unknown part '{unknownAxisPart}'.",
                nameof(motionAxes));
        }

        var offsetArray = explodedOffsets.ToArray();
        if (offsetArray.Select(offset => offset.PartId).Distinct().Count() != offsetArray.Length ||
            offsetArray.Any(offset => !partIds.Contains(offset.PartId)))
        {
            throw new ArgumentException(
                "Exploded offsets must reference unique parts in the visual model.",
                nameof(explodedOffsets));
        }

        var primitiveArray = fallbackPrimitives.ToArray();
        if (primitiveArray.Length == 0 || primitiveArray.Any(primitive => !partIds.Contains(primitive.PartId)))
        {
            throw new ArgumentException(
                "Fallback geometry must contain primitives mapped to known visual parts.",
                nameof(fallbackPrimitives));
        }

        ModelId = modelId;
        Title = title;
        Subtitle = subtitle;
        AssetDirectoryName = assetDirectoryName;
        Showcase = showcase;
        InitiallySelectedPartId = initiallySelectedPartId;
        ViewOptions = Array.AsReadOnly(optionArray);
        MotionAxes = Array.AsReadOnly(axisArray);
        ExplodedOffsets = Array.AsReadOnly(offsetArray);
        FallbackPrimitives = Array.AsReadOnly(primitiveArray);
    }

    public string ModelId { get; }

    public string Title { get; }

    public string Subtitle { get; }

    public string AssetDirectoryName { get; }

    public MechanicalShowcaseDefinition Showcase { get; }

    public RobotPartId InitiallySelectedPartId { get; }

    public IReadOnlyList<MechanicalTeachingViewOption> ViewOptions { get; }

    public IReadOnlyList<MechanicalMotionAxisGuide> MotionAxes { get; }

    public IReadOnlyList<MechanicalExplodedPartOffset> ExplodedOffsets { get; }

    public IReadOnlyList<MechanicalScenePrimitive> FallbackPrimitives { get; }
}
