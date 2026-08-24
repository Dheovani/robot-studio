using RobotStudio.Desktop.Robots;

namespace RobotStudio.Desktop.Tests;

public sealed class RobotCatalogTests
{
    [Fact]
    public void Templates_WithAvailableGCodeMapping_ShouldExposeCapabilityBadge()
    {
        var gCodeTemplates = RobotCatalog.Templates
            .Where(template => template.Capabilities.Contains(RobotCapability.GCode))
            .Select(template => template.Name)
            .ToArray();

        Assert.Equal(
            ["Cartesian Robot", "XY Plotter", "SCARA Robot"],
            gCodeTemplates);
    }

    [Fact]
    public void Templates_ShouldContainCartesianRobot()
    {
        Assert.Contains(
            RobotCatalog.Templates,
            template => template.Name == "Cartesian Robot");
    }

    [Fact]
    public void Templates_ShouldKeepDidacticComplexityOrder()
    {
        string[] expectedOrder =
        [
            "Cartesian Robot",
            "XY Plotter",
            "Differential Drive Robot",
            "Cylindrical Robot",
            "Ackermann Steering Robot",
            "SCARA Robot",
            "Simple Articulated Arm",
            "Omnidirectional Robot",
            "Delta Robot",
            "Drone",
            "Self-Balancing Robot",
            "6-DOF Industrial Arm",
            "Stewart Platform",
            "Mobile Manipulator"
        ];

        Assert.Equal(expectedOrder, RobotCatalog.Templates.Select(template => template.Name));
    }

    [Fact]
    public void Templates_ShouldExposeExpectedComplexityLevels()
    {
        var expectedComplexity = new Dictionary<string, RobotComplexityLevel>
        {
            ["Cartesian Robot"] = RobotComplexityLevel.Introductory,
            ["XY Plotter"] = RobotComplexityLevel.Beginner,
            ["Differential Drive Robot"] = RobotComplexityLevel.Intermediate,
            ["Cylindrical Robot"] = RobotComplexityLevel.Intermediate,
            ["Ackermann Steering Robot"] = RobotComplexityLevel.Intermediate,
            ["SCARA Robot"] = RobotComplexityLevel.Intermediate,
            ["Simple Articulated Arm"] = RobotComplexityLevel.Advanced,
            ["Omnidirectional Robot"] = RobotComplexityLevel.Advanced,
            ["Delta Robot"] = RobotComplexityLevel.Advanced,
            ["Drone"] = RobotComplexityLevel.Advanced,
            ["Self-Balancing Robot"] = RobotComplexityLevel.Advanced,
            ["6-DOF Industrial Arm"] = RobotComplexityLevel.Expert,
            ["Stewart Platform"] = RobotComplexityLevel.Expert,
            ["Mobile Manipulator"] = RobotComplexityLevel.Expert
        };

        foreach (var template in RobotCatalog.Templates)
        {
            Assert.Equal(expectedComplexity[template.Name], template.Complexity);
        }
    }

    [Fact]
    public void Templates_ShouldHaveFamilyAndCapabilities()
    {
        foreach (var template in RobotCatalog.Templates)
        {
            Assert.NotNull(template.Family);
            Assert.False(string.IsNullOrWhiteSpace(template.Family.Id));
            Assert.False(string.IsNullOrWhiteSpace(template.Family.Name));
            Assert.NotEmpty(template.Capabilities);
        }
    }

    [Fact]
    public void Templates_ShouldAllowOpeningAvailableImplementedRobots()
    {
        var openableTemplates = RobotCatalog.Templates
            .Where(RobotCatalog.CanOpen)
            .ToArray();

        Assert.Equal(
            ["Cartesian Robot", "XY Plotter", "Differential Drive Robot", "SCARA Robot", "Simple Articulated Arm", "Delta Robot", "Drone", "6-DOF Industrial Arm"],
            openableTemplates.Select(template => template.Name));
        Assert.All(
            openableTemplates,
            template => Assert.Equal(RobotAvailabilityStatus.Available, template.Status));
        Assert.Equal(
            [
                RobotViewerKind.CartesianThreeDimensional,
                RobotViewerKind.XYPlotterTwoDimensional,
                RobotViewerKind.DifferentialDriveTwoDimensional,
                RobotViewerKind.ScaraThreeDimensional,
                RobotViewerKind.SimpleArmThreeDimensional,
                RobotViewerKind.DeltaThreeDimensional,
                RobotViewerKind.DroneThreeDimensional,
                RobotViewerKind.IndustrialArmThreeDimensional
            ],
            openableTemplates.Select(template => template.Viewer.Kind));
    }

    [Fact]
    public void Templates_WhenUnavailable_ShouldNotBeOpenable()
    {
        var unavailableTemplates = RobotCatalog.Templates
            .Where(template => template.Status != RobotAvailabilityStatus.Available);

        Assert.All(
            unavailableTemplates,
            template => Assert.False(RobotCatalog.CanOpen(template)));
    }

    [Fact]
    public void Templates_ShouldExposeNextTeachingModelsAsPlanned()
    {
        string[] expectedPlannedModels =
        [
            "Cylindrical Robot",
            "Ackermann Steering Robot",
            "Omnidirectional Robot",
            "Self-Balancing Robot",
            "Stewart Platform",
            "Mobile Manipulator"
        ];
        var plannedTemplates = RobotCatalog.Templates
            .Where(template => template.Status == RobotAvailabilityStatus.Planned)
            .ToArray();

        Assert.Equal(expectedPlannedModels, plannedTemplates.Select(template => template.Name));
        Assert.All(plannedTemplates, template => Assert.Equal(RobotViewerKind.None, template.Viewer.Kind));
        Assert.All(plannedTemplates, template => Assert.False(RobotCatalog.CanOpen(template)));
    }

    [Fact]
    public void Templates_ShouldExposeScaraAsAvailableWithViewer()
    {
        var template = Assert.Single(
            RobotCatalog.Templates,
            template => template.Name == "SCARA Robot");

        Assert.Equal(RobotAvailabilityStatus.Available, template.Status);
        Assert.Equal(RobotViewerKind.ScaraThreeDimensional, template.Viewer.Kind);
        Assert.True(RobotCatalog.CanOpen(template));
    }

    [Fact]
    public void Templates_ShouldExposeSimpleArmAsAvailableWithViewer()
    {
        var template = Assert.Single(
            RobotCatalog.Templates,
            template => template.Name == "Simple Articulated Arm");

        Assert.Equal(RobotAvailabilityStatus.Available, template.Status);
        Assert.Equal(RobotViewerKind.SimpleArmThreeDimensional, template.Viewer.Kind);
        Assert.True(RobotCatalog.CanOpen(template));
    }

    [Fact]
    public void Templates_ShouldExposeDeltaAsAvailableWithViewer()
    {
        var template = Assert.Single(
            RobotCatalog.Templates,
            template => template.Name == "Delta Robot");

        Assert.Equal(RobotAvailabilityStatus.Available, template.Status);
        Assert.Equal(RobotViewerKind.DeltaThreeDimensional, template.Viewer.Kind);
        Assert.True(RobotCatalog.CanOpen(template));
    }

    [Fact]
    public void Templates_ShouldExposeDroneAsAvailableWithViewer()
    {
        var template = Assert.Single(
            RobotCatalog.Templates,
            template => template.Name == "Drone");

        Assert.Equal(RobotAvailabilityStatus.Available, template.Status);
        Assert.Equal(RobotViewerKind.DroneThreeDimensional, template.Viewer.Kind);
        Assert.True(RobotCatalog.CanOpen(template));
    }

    [Fact]
    public void Templates_ShouldExposeIndustrialArmAsAvailableWithViewer()
    {
        var template = Assert.Single(
            RobotCatalog.Templates,
            template => template.Name == "6-DOF Industrial Arm");

        Assert.Equal(RobotAvailabilityStatus.Available, template.Status);
        Assert.Equal(RobotViewerKind.IndustrialArmThreeDimensional, template.Viewer.Kind);
        Assert.True(RobotCatalog.CanOpen(template));
    }
}
