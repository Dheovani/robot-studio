using RobotStudio.Desktop.Examples;
using RobotStudio.Desktop.Robots;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;
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

    [Fact]
    public void CartesianExamples_ShouldMatchTheirExpectedValidationResultInBothDialects()
    {
        var profile = CreateCartesianProfile();
        var context = new RobotScriptParseContext(new CartesianPosition(40, 30, 20));

        foreach (var example in RobotExampleCatalog.GetFor(RobotViewerKind.CartesianThreeDimensional))
        {
            AssertValidationResult(
                new RobotScriptParser().Parse(example.Script).Commands,
                profile,
                example.ExpectedResult);

            if (example.GCodeScript is not null)
            {
                AssertValidationResult(
                    new GCodeParser().Parse(example.GCodeScript, context).Commands,
                    profile,
                    example.ExpectedResult);
            }
        }
    }

    [Fact]
    public void CartesianTeachingFiles_ShouldMatchDesktopCatalogScripts()
    {
        var root = FindRepositoryRoot();
        var examplesByName = RobotExampleCatalog
            .GetFor(RobotViewerKind.CartesianThreeDimensional)
            .ToDictionary(example => example.Name, StringComparer.Ordinal);
        var mappings = new[]
        {
            ("Axis limit validation (invalid)", "invalid-axis-limit"),
            ("Requested vs effective speed", "speed-comparison"),
            ("Jog, wait, and home sequence", "jog-wait-home")
        };

        foreach (var (exampleName, fileName) in mappings)
        {
            var example = examplesByName[exampleName];
            Assert.Equal(
                Normalize(example.Script),
                Normalize(File.ReadAllText(Path.Combine(root, "examples", "cartesian", $"{fileName}.robot"))));
            Assert.Equal(
                Normalize(example.GCodeScript!),
                Normalize(File.ReadAllText(Path.Combine(root, "examples", "cartesian", $"{fileName}.gcode"))));
        }
    }

    [Fact]
    public void CartesianCatalog_ShouldContainOneIntentionalValidationFailure()
    {
        var invalidExample = Assert.Single(
            RobotExampleCatalog.GetFor(RobotViewerKind.CartesianThreeDimensional),
            example => example.ExpectedResult == RobotExampleExpectedResult.ValidationError);

        Assert.Contains("invalid", invalidExample.Name, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertValidationResult(
        IReadOnlyList<RobotCommand> commands,
        CartesianRobotProfile profile,
        RobotExampleExpectedResult expectedResult)
    {
        void ValidateCommands()
        {
            foreach (var command in commands)
            {
                RobotCommandValidator.Validate(command, profile);
            }
        }

        if (expectedResult == RobotExampleExpectedResult.ValidationError)
        {
            Assert.Throws<PositionOutOfRangeException>(ValidateCommands);
            return;
        }

        ValidateCommands();
    }

    private static CartesianRobotProfile CreateCartesianProfile() =>
        CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));

    private static string Normalize(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RobotStudio.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the RobotStudio repository root.");
    }
}
