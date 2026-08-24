using RobotStudio.Visualization;

namespace RobotStudio.Visualization.Tests;

public sealed class RobotVisualModelDefinitionTests
{
    [Fact]
    public void Constructor_WhenHierarchyIsValid_ShouldExposePartsBySemanticId()
    {
        var model = CreateModel();

        var carriage = model.GetPart(new RobotPartId("x-carriage"));

        Assert.Equal("X carriage", carriage.Name);
        Assert.Equal(new RobotPartId("base"), carriage.ParentId);
    }

    [Fact]
    public void Constructor_WhenPartIdIsDuplicated_ShouldThrow()
    {
        var root = CreatePart("base", parentId: null);

        var exception = Assert.Throws<ArgumentException>(() => new RobotVisualModelDefinition(
            "cartesian",
            "Cartesian Robot",
            root.Id,
            [root, CreatePart("base", parentId: null)]));

        Assert.Contains("duplicated", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_WhenParentDoesNotExist_ShouldThrow()
    {
        var root = CreatePart("base", parentId: null);
        var orphan = CreatePart("tool", new RobotPartId("missing"));

        var exception = Assert.Throws<ArgumentException>(() => new RobotVisualModelDefinition(
            "cartesian",
            "Cartesian Robot",
            root.Id,
            [root, orphan]));

        Assert.Contains("missing parent", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_WhenHierarchyContainsCycle_ShouldThrow()
    {
        var root = CreatePart("base", parentId: null);
        var first = CreatePart("first", new RobotPartId("second"));
        var second = CreatePart("second", first.Id);

        var exception = Assert.Throws<ArgumentException>(() => new RobotVisualModelDefinition(
            "cartesian",
            "Cartesian Robot",
            root.Id,
            [root, first, second]));

        Assert.Contains("cycle", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static RobotVisualModelDefinition CreateModel()
    {
        var root = CreatePart("base", parentId: null);
        var carriage = new RobotPartDefinition(
            new RobotPartId("x-carriage"),
            "X carriage",
            RobotPartKind.Carriage,
            root.Id,
            "Carries the Y assembly along the X rail.",
            "Translates along the X axis.");

        return new RobotVisualModelDefinition(
            "cartesian",
            "Cartesian Robot",
            root.Id,
            [root, carriage]);
    }

    private static RobotPartDefinition CreatePart(string id, RobotPartId? parentId) =>
        new(
            new RobotPartId(id),
            id,
            RobotPartKind.Structure,
            parentId,
            "Supports the mechanism.",
            "Remains fixed.");
}
