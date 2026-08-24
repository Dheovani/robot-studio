namespace RobotStudio.Visualization;

public sealed record RobotPartDefinition
{
    public RobotPartDefinition(
        RobotPartId id,
        string name,
        RobotPartKind kind,
        RobotPartId? parentId,
        string function,
        string movement,
        bool isSelectable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(function);
        ArgumentException.ThrowIfNullOrWhiteSpace(movement);

        Id = id;
        Name = name.Trim();
        Kind = kind;
        ParentId = parentId;
        Function = function.Trim();
        Movement = movement.Trim();
        IsSelectable = isSelectable;
    }

    public RobotPartId Id { get; }

    public string Name { get; }

    public RobotPartKind Kind { get; }

    public RobotPartId? ParentId { get; }

    public string Function { get; }

    public string Movement { get; }

    public bool IsSelectable { get; }
}
