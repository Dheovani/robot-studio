using RobotStudio.Desktop.Showcases;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class MechanicalShowcasePresentationTests
{
    [Fact]
    public void Constructor_WhenModelIdDoesNotMatchDefinition_ShouldRejectPresentation()
    {
        var source = CartesianMechanicalShowcaseDefinition.CreatePresentation();

        var exception = Assert.Throws<ArgumentException>(() => Create(
            source,
            modelId: "different-model"));

        Assert.Equal("modelId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenViewReferencesUnknownDemonstration_ShouldRejectPresentation()
    {
        var source = CartesianMechanicalShowcaseDefinition.CreatePresentation();
        var viewOptions = new[]
        {
            new MechanicalTeachingViewOption(
                MechanicalTeachingViewMode.Assembled,
                "Invalid view",
                "References a demonstration that does not exist.",
                ["unknown-demonstration"])
        };

        var exception = Assert.Throws<ArgumentException>(() => Create(
            source,
            viewOptions: viewOptions));

        Assert.Equal("viewOptions", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenFallbackGeometryReferencesUnknownPart_ShouldRejectPresentation()
    {
        var source = CartesianMechanicalShowcaseDefinition.CreatePresentation();
        var fallback = new MechanicalScenePrimitive[]
        {
            new MechanicalBoxPrimitive(
                new RobotPartId("unknown-part"),
                System.Numerics.Vector3.Zero,
                System.Numerics.Vector3.One,
                MechanicalMaterialRole.Frame)
        };

        var exception = Assert.Throws<ArgumentException>(() => Create(
            source,
            fallbackPrimitives: fallback));

        Assert.Equal("fallbackPrimitives", exception.ParamName);
    }

    private static MechanicalShowcasePresentation Create(
        MechanicalShowcasePresentation source,
        string? modelId = null,
        IReadOnlyList<MechanicalTeachingViewOption>? viewOptions = null,
        IReadOnlyList<MechanicalScenePrimitive>? fallbackPrimitives = null) =>
        new(
            modelId ?? source.ModelId,
            source.Title,
            source.Subtitle,
            source.AssetDirectoryName,
            source.Showcase,
            source.InitiallySelectedPartId,
            viewOptions ?? source.ViewOptions,
            source.MotionAxes,
            source.ExplodedOffsets,
            fallbackPrimitives ?? source.FallbackPrimitives);
}
