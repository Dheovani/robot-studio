using System.Collections.ObjectModel;

namespace RobotStudio.Visualization;

public sealed class RobotVisualModelDefinition
{
    private readonly IReadOnlyDictionary<RobotPartId, RobotPartDefinition> partsById;

    public RobotVisualModelDefinition(
        string id,
        string name,
        RobotPartId rootPartId,
        IEnumerable<RobotPartDefinition> parts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(parts);

        var partArray = parts.ToArray();
        if (partArray.Length == 0)
        {
            throw new ArgumentException("A visual model must define at least one part.", nameof(parts));
        }

        var duplicate = partArray
            .GroupBy(part => part.Id)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException($"Visual part id '{duplicate.Key}' is duplicated.", nameof(parts));
        }

        var index = partArray.ToDictionary(part => part.Id);
        if (!index.TryGetValue(rootPartId, out var rootPart))
        {
            throw new ArgumentException($"Root visual part '{rootPartId}' does not exist.", nameof(rootPartId));
        }

        if (rootPart.ParentId is not null)
        {
            throw new ArgumentException("The root visual part cannot have a parent.", nameof(parts));
        }

        ValidateHierarchy(rootPartId, partArray, index);

        Id = id.Trim();
        Name = name.Trim();
        RootPartId = rootPartId;
        Parts = Array.AsReadOnly(partArray);
        partsById = new ReadOnlyDictionary<RobotPartId, RobotPartDefinition>(index);
    }

    public string Id { get; }

    public string Name { get; }

    public RobotPartId RootPartId { get; }

    public IReadOnlyList<RobotPartDefinition> Parts { get; }

    public RobotPartDefinition GetPart(RobotPartId partId) =>
        partsById.TryGetValue(partId, out var part)
            ? part
            : throw new KeyNotFoundException($"Visual part '{partId}' is not defined by model '{Id}'.");

    private static void ValidateHierarchy(
        RobotPartId rootPartId,
        IReadOnlyList<RobotPartDefinition> parts,
        IReadOnlyDictionary<RobotPartId, RobotPartDefinition> index)
    {
        foreach (var part in parts)
        {
            if (part.Id == rootPartId)
            {
                continue;
            }

            if (part.ParentId is null)
            {
                throw new ArgumentException($"Visual part '{part.Id}' must define a parent.", nameof(parts));
            }

            if (!index.ContainsKey(part.ParentId.Value))
            {
                throw new ArgumentException(
                    $"Visual part '{part.Id}' references missing parent '{part.ParentId}'.",
                    nameof(parts));
            }

            var visited = new HashSet<RobotPartId> { part.Id };
            var current = part;
            while (current.ParentId is RobotPartId parentId)
            {
                if (!visited.Add(parentId))
                {
                    throw new ArgumentException(
                        $"Visual hierarchy contains a cycle involving part '{parentId}'.",
                        nameof(parts));
                }

                current = index[parentId];
            }

            if (current.Id != rootPartId)
            {
                throw new ArgumentException(
                    $"Visual part '{part.Id}' is not connected to root '{rootPartId}'.",
                    nameof(parts));
            }
        }
    }
}
