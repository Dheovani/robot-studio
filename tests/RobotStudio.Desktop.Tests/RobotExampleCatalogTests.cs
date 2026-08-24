using RobotStudio.Desktop.Examples;
using RobotStudio.Desktop.Robots;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;
using RobotStudio.Scripting;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop.Tests;

public sealed class RobotExampleCatalogTests
{
    [Fact]
    public void ScaraExamples_WithGCode_ShouldProduceSuccessfulToolSpacePlayback()
    {
        var profile = CreateScaraProfile();
        var initialJoints = new ScaraJointPosition(0, 0);
        var parser = new GCodeParser(new ScaraGCodeCommandMapper(profile));

        foreach (var example in RobotExampleCatalog.GetFor(RobotViewerKind.ScaraThreeDimensional))
        {
            Assert.False(string.IsNullOrWhiteSpace(example.GCodeScript));
            var commands = parser.Parse(
                example.GCodeScript!,
                new RobotScriptParseContext(initialJoints));
            var result = new ScaraSimulator().Execute(
                ScaraSimulationContext.Create(profile, initialJoints),
                commands);

            Assert.True(result.Succeeded, result.Failure?.Message);
            Assert.Contains(
                commands.Commands,
                command => command is ScaraLinearMoveCommand);
        }
    }

    [Fact]
    public void ScaraBasicGCodeFile_ShouldMatchDesktopCatalog()
    {
        var example = RobotExampleCatalog.GetDefaultFor(
            RobotViewerKind.ScaraThreeDimensional);
        var path = Path.Combine(
            FindRepositoryRoot(),
            "examples",
            "scara",
            "basic.gcode");

        Assert.Equal(
            Normalize(example.GCodeScript!),
            Normalize(File.ReadAllText(path)));
    }

    [Fact]
    public void SimpleArmExamples_WithGCode_ShouldProduceSuccessfulToolPosePlayback()
    {
        var profile = CreateSimpleArmProfile();
        var initialJoints = new SimpleArmJointPosition(0, 0, 0);
        var parser = new GCodeParser(new SimpleArmGCodeCommandMapper(profile));

        foreach (var example in RobotExampleCatalog.GetFor(RobotViewerKind.SimpleArmThreeDimensional))
        {
            Assert.False(string.IsNullOrWhiteSpace(example.GCodeScript));
            var commands = parser.Parse(
                example.GCodeScript!,
                new RobotScriptParseContext(initialJoints));
            var result = new SimpleArmSimulator().Execute(
                SimpleArmSimulationContext.Create(profile, initialJoints),
                commands);

            Assert.True(result.Succeeded, result.Failure?.Message);
            Assert.Contains(commands.Commands, command => command is SimpleArmLinearMoveCommand);
        }
    }

    [Fact]
    public void SimpleArmBasicGCodeFile_ShouldMatchDesktopCatalog()
    {
        var example = RobotExampleCatalog.GetDefaultFor(
            RobotViewerKind.SimpleArmThreeDimensional);
        var path = Path.Combine(
            FindRepositoryRoot(),
            "examples",
            "simple-arm",
            "basic.gcode");

        Assert.Equal(
            Normalize(example.GCodeScript!),
            Normalize(File.ReadAllText(path)));
    }

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

    [Theory]
    [InlineData(RobotViewerKind.CartesianThreeDimensional, 40, 30, 20)]
    [InlineData(RobotViewerKind.XYPlotterTwoDimensional, 40, 30, 0)]
    public void CartesianFamilyDefault_WhenConvertedToGCode_ShouldProduceMovingPlayback(
        RobotViewerKind viewerKind,
        double initialX,
        double initialY,
        double initialZ)
    {
        var profile = CreateCartesianProfile();
        var initialPosition = new CartesianPosition(initialX, initialY, initialZ);
        var example = RobotExampleCatalog.GetDefaultFor(viewerKind);
        var gCode = example.GCodeScript ??
            GCodeWriter.Write(new RobotScriptParser().Parse(example.Script));
        var commands = new GCodeParser().Parse(
            gCode,
            new RobotScriptParseContext(initialPosition));
        var result = new RobotSimulator().Execute(
            SimulationContext.Create(profile, initialPosition),
            commands);
        var snapshot = new CartesianPlaybackSnapshotBuilder().Build(
            profile,
            result,
            TimeSpan.FromMilliseconds(100));

        Assert.True(snapshot.Succeeded);
        Assert.True(snapshot.SceneFrameCount > 1);
        Assert.Contains(
            snapshot.Frames,
            frame => frame.Position != new VisualVector3(initialX, initialY, initialZ));
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

    private static ScaraRobotProfile CreateScaraProfile() =>
        new(
            firstLinkLengthMillimeters: 180,
            secondLinkLengthMillimeters: 120,
            linkCollisionRadiusMillimeters: 12,
            shoulderJoint: new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120, 240),
            elbowJoint: new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100, 200));

    private static SimpleArmRobotProfile CreateSimpleArmProfile() =>
        new(
            firstLinkLengthMillimeters: 120,
            secondLinkLengthMillimeters: 90,
            thirdLinkLengthMillimeters: 60,
            linkCollisionRadiusMillimeters: 10,
            baseJoint: new SimpleArmJoint(SimpleArmJointId.Base, -180, 180, 100, 200),
            shoulderJoint: new SimpleArmJoint(SimpleArmJointId.Shoulder, -120, 120, 90, 180),
            elbowJoint: new SimpleArmJoint(SimpleArmJointId.Elbow, -150, 150, 80, 160));

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
