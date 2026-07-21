using RobotStudio.Desktop.Robots;

namespace RobotStudio.Desktop.Tests;

public sealed class RobotCatalogTests
{
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
            "SCARA Robot",
            "Simple Articulated Arm",
            "Delta Robot",
            "Drone",
            "6-DOF Industrial Arm"
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
            ["SCARA Robot"] = RobotComplexityLevel.Intermediate,
            ["Simple Articulated Arm"] = RobotComplexityLevel.Advanced,
            ["Delta Robot"] = RobotComplexityLevel.Advanced,
            ["Drone"] = RobotComplexityLevel.Advanced,
            ["6-DOF Industrial Arm"] = RobotComplexityLevel.Expert
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
    public void Templates_ShouldOnlyAllowOpeningAvailableCartesianRobot()
    {
        var openableTemplates = RobotCatalog.Templates
            .Where(RobotCatalog.CanOpen)
            .ToArray();

        var template = Assert.Single(openableTemplates);
        Assert.Equal("Cartesian Robot", template.Name);
        Assert.Equal(RobotAvailabilityStatus.Available, template.Status);
        Assert.Equal(RobotViewerKind.CartesianThreeDimensional, template.Viewer.Kind);
    }

    [Fact]
    public void Templates_WhenPlanned_ShouldNotBeOpenable()
    {
        var plannedTemplates = RobotCatalog.Templates
            .Where(template => template.Status == RobotAvailabilityStatus.Planned);

        Assert.All(
            plannedTemplates,
            template => Assert.False(RobotCatalog.CanOpen(template)));
    }
}
