using System.Numerics;
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

    [Fact]
    public void Constructor_WhenParallelLinkReferencesUnknownEndpoint_ShouldRejectPresentation()
    {
        var source = DeltaMechanicalShowcaseDefinition.CreatePresentation();
        var original = source.ParallelLinkConstraints[0];
        var constraints = source.ParallelLinkConstraints
            .Select((constraint, index) => index == 0
                ? constraint with { EndPartId = new RobotPartId("unknown-platform") }
                : constraint)
            .ToArray();

        var exception = Assert.Throws<ArgumentException>(() => Create(
            source,
            parallelLinkConstraints: constraints));

        Assert.NotEqual(original.EndPartId, constraints[0].EndPartId);
        Assert.Equal("parallelLinkConstraints", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenParallelLinkHasNoLength_ShouldRejectPresentation()
    {
        var source = DeltaMechanicalShowcaseDefinition.CreatePresentation();
        var constraints = source.ParallelLinkConstraints
            .Select((constraint, index) => index == 0
                ? constraint with { AuthoredEndMillimeters = constraint.AuthoredStartMillimeters }
                : constraint)
            .ToArray();

        var exception = Assert.Throws<ArgumentException>(() => Create(
            source,
            parallelLinkConstraints: constraints));

        Assert.Equal("parallelLinkConstraints", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldComposeMotionAxesAsRendererIndependentOverlays()
    {
        var presentation = CartesianMechanicalShowcaseDefinition.CreatePresentation();

        var overlays = presentation.EducationalOverlays.Primitives
            .OfType<RobotOverlayLine>()
            .ToArray();

        Assert.Equal(presentation.MotionAxes.Count, overlays.Length);
        Assert.All(overlays, overlay => Assert.Equal(RobotOverlayKind.CoordinateAxis, overlay.Kind));
        Assert.Equal(
            [RobotOverlayAxis.X, RobotOverlayAxis.Y, RobotOverlayAxis.Z],
            overlays.Select(overlay => overlay.Axis));
    }

    private static MechanicalShowcasePresentation Create(
        MechanicalShowcasePresentation source,
        string? modelId = null,
        IReadOnlyList<MechanicalTeachingViewOption>? viewOptions = null,
        IReadOnlyList<MechanicalScenePrimitive>? fallbackPrimitives = null,
        IReadOnlyList<MechanicalParallelLinkConstraint>? parallelLinkConstraints = null) =>
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
            fallbackPrimitives ?? source.FallbackPrimitives,
            source.RevoluteJointPivots,
            parallelLinkConstraints ?? source.ParallelLinkConstraints);
}
