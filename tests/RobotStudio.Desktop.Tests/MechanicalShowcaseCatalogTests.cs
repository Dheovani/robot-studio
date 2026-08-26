using RobotStudio.Desktop.Robots;
using RobotStudio.Desktop.Showcases;

namespace RobotStudio.Desktop.Tests;

public sealed class MechanicalShowcaseCatalogTests
{
    [Fact]
    public void Create_WhenModelIsRegistered_ShouldReturnMatchingPresentation()
    {
        var presentation = MechanicalShowcaseCatalog.Create("cartesian-intro-mechanical");

        Assert.Equal("cartesian-intro-mechanical", presentation.ModelId);
        Assert.Equal(presentation.ModelId, presentation.Showcase.Model.Id);
    }

    [Fact]
    public void ModelIds_ShouldExposeEveryImplementedMechanicalShowcase()
    {
        Assert.Equal(
            ["cartesian-intro-mechanical", "xy-plotter-mechanical", "differential-drive-mechanical", "scara-mechanical", "simple-arm-mechanical", "delta-mechanical"],
            MechanicalShowcaseCatalog.ModelIds);
    }

    [Fact]
    public void Create_WhenModelIsUnknown_ShouldThrowClearException()
    {
        var exception = Assert.Throws<KeyNotFoundException>(() =>
            MechanicalShowcaseCatalog.Create("unknown-mechanical-model"));

        Assert.Contains("unknown-mechanical-model", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RobotCatalog_ShouldReferenceOnlyRegisteredMechanicalShowcases()
    {
        var descriptorIds = RobotCatalog.Templates
            .Where(RobotCatalog.CanExploreMechanics)
            .Select(template => template.MechanicalShowcase!.ModelId)
            .ToArray();

        Assert.Equal(MechanicalShowcaseCatalog.ModelIds.Order(), descriptorIds.Order());
    }
}
