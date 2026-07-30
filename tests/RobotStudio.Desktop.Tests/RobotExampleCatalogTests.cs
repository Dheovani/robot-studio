using RobotStudio.Desktop.Examples;
using RobotStudio.Desktop.Robots;

namespace RobotStudio.Desktop.Tests;

public sealed class RobotExampleCatalogTests
{
    [Fact]
    public void All_ShouldExposeOneExampleForEachOpenableViewer()
    {
        var openableViewerKinds = RobotCatalog.Templates
            .Where(RobotCatalog.CanOpen)
            .Select(template => template.Viewer.Kind)
            .Order()
            .ToArray();

        var exampleViewerKinds = RobotExampleCatalog.All
            .Select(example => example.ViewerKind)
            .Order()
            .ToArray();

        Assert.Equal(openableViewerKinds, exampleViewerKinds);
    }

    [Fact]
    public void All_ShouldExposeNonEmptyScripts()
    {
        Assert.All(
            RobotExampleCatalog.All,
            example =>
            {
                Assert.False(string.IsNullOrWhiteSpace(example.Name));
                Assert.False(string.IsNullOrWhiteSpace(example.Description));
                Assert.False(string.IsNullOrWhiteSpace(example.Script));
            });
    }

    [Fact]
    public void GetDefaultFor_WhenViewerExists_ShouldReturnMatchingExample()
    {
        var example = RobotExampleCatalog.GetDefaultFor(RobotViewerKind.ScaraThreeDimensional);

        Assert.Equal(RobotViewerKind.ScaraThreeDimensional, example.ViewerKind);
        Assert.Contains("SCARA", example.Script);
    }
}
