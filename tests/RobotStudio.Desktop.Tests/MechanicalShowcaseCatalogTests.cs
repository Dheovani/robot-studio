using RobotStudio.Desktop.Robots;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Desktop.Showcases.Assets;
using RobotStudio.Visualization;

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
            ["cartesian-intro-mechanical", "xy-plotter-mechanical", "differential-drive-mechanical", "scara-mechanical", "simple-arm-mechanical", "delta-mechanical", "drone-mechanical", "industrial-arm-mechanical"],
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

    [Fact]
    public void RegisteredShowcases_ShouldPassTheCrossFamilyRenderingSmokeContract()
    {
        var loader = new RobotVisualAssetPackageLoader();
        var importer = new HelixRobotVisualAssetImporter();

        foreach (var modelId in MechanicalShowcaseCatalog.ModelIds)
        {
            var presentation = MechanicalShowcaseCatalog.Create(modelId);
            var model = presentation.Showcase.Model;
            var manifestPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Robots",
                presentation.AssetDirectoryName,
                "robot.json");
            var package = loader.Load(manifestPath, model);
            using var scene = importer.Import(package);

            var initialPart = model.GetPart(presentation.InitiallySelectedPartId);
            Assert.True(initialPart.IsSelectable);
            Assert.NotEmpty(scene.NodesByPart[presentation.InitiallySelectedPartId]);

            foreach (var view in presentation.ViewOptions)
            {
                var demonstration = presentation.Showcase.Demonstrations.Single(item =>
                    item.Id == view.DemonstrationIds[0]);
                var sampledPoses = MechanicalDemonstrationSampler.Sample(
                    demonstration,
                    TimeSpan.FromTicks(demonstration.Duration.Ticks / 2));
                var viewPoses = MechanicalTeachingPoseComposer.Compose(
                    model,
                    sampledPoses,
                    view.Mode,
                    presentation.ExplodedOffsets);
                var worldTransforms = RobotComponentPoseResolver.ResolveWorldTransforms(model, viewPoses);

                Assert.Equal(model.Parts.Count, worldTransforms.Count);
            }
        }
    }
}
