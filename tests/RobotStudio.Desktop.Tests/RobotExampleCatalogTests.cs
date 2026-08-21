using RobotStudio.Desktop.Examples;
using RobotStudio.Desktop.Robots;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Scripting;

namespace RobotStudio.Desktop.Tests;

public sealed class RobotExampleCatalogTests
{
    [Fact]
    public void All_ShouldExposeExamplesForEachOpenableViewer()
    {
        var openableViewerKinds = RobotCatalog.Templates
            .Where(RobotCatalog.CanOpen)
            .Select(template => template.Viewer.Kind)
            .ToArray();

        Assert.All(
            openableViewerKinds,
            viewerKind => Assert.NotEmpty(RobotExampleCatalog.GetFor(viewerKind)));
    }

    [Fact]
    public void GetFor_ShouldReturnOnlyExamplesForTheRequestedViewer()
    {
        var examples = RobotExampleCatalog.GetFor(RobotViewerKind.DifferentialDriveTwoDimensional);

        Assert.NotEmpty(examples);
        Assert.All(
            examples,
            example => Assert.Equal(RobotViewerKind.DifferentialDriveTwoDimensional, example.ViewerKind));
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

    [Fact]
    public void All_ShouldExposeMultipleExamplesForImplementedTrainingViewers()
    {
        Assert.True(RobotExampleCatalog.GetFor(RobotViewerKind.CartesianThreeDimensional).Count >= 2);
        Assert.True(RobotExampleCatalog.GetFor(RobotViewerKind.XYPlotterTwoDimensional).Count >= 2);
        Assert.True(RobotExampleCatalog.GetFor(RobotViewerKind.DifferentialDriveTwoDimensional).Count >= 2);
        Assert.True(RobotExampleCatalog.GetFor(RobotViewerKind.ScaraThreeDimensional).Count >= 2);
        Assert.True(RobotExampleCatalog.GetFor(RobotViewerKind.SimpleArmThreeDimensional).Count >= 2);
        Assert.True(RobotExampleCatalog.GetFor(RobotViewerKind.DeltaThreeDimensional).Count >= 2);
        Assert.True(RobotExampleCatalog.GetFor(RobotViewerKind.DroneThreeDimensional).Count >= 2);
        Assert.True(RobotExampleCatalog.GetFor(RobotViewerKind.IndustrialArmThreeDimensional).Count >= 2);
    }

    [Fact]
    public void CartesianExamples_WithDedicatedGCode_ShouldParseFromViewerInitialPosition()
    {
        var examples = RobotExampleCatalog
            .GetFor(RobotViewerKind.CartesianThreeDimensional)
            .Where(example => example.GCodeScript is not null);

        Assert.NotEmpty(examples);
        Assert.All(
            examples,
            example => Assert.NotEmpty(new GCodeParser().Parse(
                example.GCodeScript!,
                new RobotScriptParseContext(new CartesianPosition(40, 30, 20))).Commands));
    }
}
