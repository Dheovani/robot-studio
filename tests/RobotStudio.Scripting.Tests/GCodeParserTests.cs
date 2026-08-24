using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Scripting.Tests;

public sealed class GCodeParserTests
{
    [Fact]
    public void CompileProgram_WhenMoveOmitsAxes_ShouldPreserveToolSpaceIntentWithoutMapping()
    {
        var program = new GCodeParser().CompileProgram(
            "G1 X10 F3000");

        var move = Assert.IsType<GCodeLinearMoveInstruction>(
            Assert.Single(program.Instructions));
        Assert.Equal(10, move.XMillimeters);
        Assert.Null(move.YMillimeters);
        Assert.Null(move.ZMillimeters);
        Assert.Null(move.ADegrees);
        Assert.Equal(3000, move.FeedRateMillimetersPerMinute);
        Assert.Equal(1, move.Source.LineNumber);
    }

    [Fact]
    public void CompileProgram_WhenMoveContainsOrientation_ShouldPreservePoseWords()
    {
        var move = Assert.IsType<GCodeLinearMoveInstruction>(Assert.Single(
            new GCodeParser().CompileProgram("G1 X180 Y80 A30 F3600").Instructions));

        Assert.Equal(180, move.XMillimeters);
        Assert.Equal(80, move.YMillimeters);
        Assert.Equal(30, move.ADegrees);
        Assert.Null(move.BDegrees);
        Assert.Null(move.CDegrees);
    }

    [Fact]
    public void Compile_WhenCartesianMoveContainsOrientation_ShouldRejectUnsupportedWords()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Compile("G1 X10 Y20 Z5 A30"));

        Assert.Contains("do not support A, B, or C", exception.Message);
    }

    [Fact]
    public void Compile_WhenProgramContainsPositioningModes_ShouldPreserveDirectivesAndCommands()
    {
        var compilation = new GCodeParser().Compile(
            """
            G90
            G1 X10 Y20 Z5
            G91
            G1 X5
            """);

        Assert.Collection(
            compilation.Statements,
            statement => Assert.Equal(
                RobotScriptPositioningMode.Absolute,
                Assert.IsType<RobotScriptPositioningModeStatement>(statement).Mode),
            statement => Assert.IsType<MoveToCommand>(
                Assert.IsType<RobotScriptCommandStatement>(statement).Command),
            statement => Assert.Equal(
                RobotScriptPositioningMode.Relative,
                Assert.IsType<RobotScriptPositioningModeStatement>(statement).Mode),
            statement => Assert.IsType<MoveToCommand>(
                Assert.IsType<RobotScriptCommandStatement>(statement).Command));
        Assert.Equal(2, compilation.Commands.Commands.Count);
    }

    [Fact]
    public void Compile_WhenProgramDeclaresMillimeters_ShouldPreserveUnitDirective()
    {
        var compilation = new GCodeParser().Compile(
            "G21\nG1 X10 Y20 Z5");

        var unit = Assert.IsType<RobotScriptUnitStatement>(
            compilation.Statements[0]);
        Assert.Equal(RobotScriptUnit.Millimeters, unit.Unit);
        Assert.Single(compilation.Commands.Commands);
    }

    [Fact]
    public void Compile_WhenContextPositionIsNotCartesian_ShouldRejectIncompatibleContext()
    {
        var context = new RobotScriptParseContext(
            new DifferentialDrivePose(0, 0, 0));

        var exception = Assert.Throws<ArgumentException>(() =>
            new GCodeParser().Compile("G28", context));

        Assert.Contains("requires a CartesianPosition initial position", exception.Message);
    }

    [Fact]
    public void Parse_WhenSupportedProgramIsValid_ShouldReturnSharedDomainCommands()
    {
        var sequence = new GCodeParser().Parse(
            """
            G28
            G1 X10 Y20 Z5 F6000
            G4 P500
            """);

        var home = Assert.IsType<HomeCommand>(sequence.Commands[0]);
        Assert.Equal(1, home.Source?.LineNumber);

        var move = Assert.IsType<MoveToCommand>(sequence.Commands[1]);
        Assert.Equal(10, move.TargetPosition.X);
        Assert.Equal(20, move.TargetPosition.Y);
        Assert.Equal(5, move.TargetPosition.Z);
        Assert.Equal(100, move.RequestedVelocityMillimetersPerSecond);
        Assert.Equal("G1 X10 Y20 Z5 F6000", move.Source?.Text);

        var wait = Assert.IsType<WaitCommand>(sequence.Commands[2]);
        Assert.Equal(TimeSpan.FromMilliseconds(500), wait.Duration);
    }

    [Fact]
    public void Parse_WhenWordsAreCompactAndNumbered_ShouldParseProgram()
    {
        var move = Assert.IsType<MoveToCommand>(Assert.Single(
            new GCodeParser().Parse("N10 G01X10.5Y20Z5F3000").Commands));

        Assert.Equal(10.5, move.TargetPosition.X);
        Assert.Equal(50, move.RequestedVelocityMillimetersPerSecond);
    }

    [Fact]
    public void Parse_WhenLineContainsComments_ShouldIgnoreCommentsAndPreserveSource()
    {
        const string source = "G1 X10 Y20 Z5 (teaching point) F3000 ; move to target";

        var move = Assert.IsType<MoveToCommand>(Assert.Single(
            new GCodeParser().Parse(source).Commands));

        Assert.Equal(source, move.Source?.Text);
        Assert.Equal(50, move.RequestedVelocityMillimetersPerSecond);
    }

    [Fact]
    public void Parse_WhenAbsoluteG1OmitsCoordinateWithoutContext_ShouldExplainRequiredPosition()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Parse("G1 X10 Y20"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("requires a known initial Cartesian position", exception.Message);
    }

    [Fact]
    public void Parse_WhenAbsoluteG1OmitsAxes_ShouldRetainCurrentCoordinates()
    {
        var context = new RobotScriptParseContext(
            new CartesianPosition(40, 30, 20));

        var move = Assert.IsType<MoveToCommand>(Assert.Single(
            new GCodeParser().Parse("G90\nG1 X100", context).Commands));

        Assert.Equal(100, move.TargetPosition.X);
        Assert.Equal(30, move.TargetPosition.Y);
        Assert.Equal(20, move.TargetPosition.Z);
        Assert.Equal(2, move.Source?.LineNumber);
    }

    [Fact]
    public void Parse_WhenRelativeMovesAreConsecutive_ShouldResolveAbsoluteTargets()
    {
        var context = new RobotScriptParseContext(
            new CartesianPosition(40, 30, 20));

        var sequence = new GCodeParser().Parse(
            """
            G91
            G1 X10 Y-5 F3000
            G1 Z15
            """,
            context);

        var first = Assert.IsType<MoveToCommand>(sequence.Commands[0]);
        Assert.Equal(new CartesianPosition(50, 25, 20), first.TargetPosition);
        Assert.Equal(50, first.RequestedVelocityMillimetersPerSecond);

        var second = Assert.IsType<MoveToCommand>(sequence.Commands[1]);
        Assert.Equal(new CartesianPosition(50, 25, 35), second.TargetPosition);
    }

    [Fact]
    public void Parse_WhenModeReturnsToAbsolute_ShouldStopAccumulatingCoordinates()
    {
        var context = new RobotScriptParseContext(
            new CartesianPosition(40, 30, 20));

        var sequence = new GCodeParser().Parse(
            """
            G91
            G1 X10
            G90
            G1 Y100
            """,
            context);

        Assert.Equal(
            new CartesianPosition(50, 100, 20),
            Assert.IsType<MoveToCommand>(sequence.Commands[1]).TargetPosition);
    }

    [Fact]
    public void Parse_WhenRelativeModeFollowsHome_ShouldUseOriginAsReference()
    {
        var sequence = new GCodeParser().Parse(
            """
            G28
            G91
            G1 X10 Y20
            """);

        Assert.IsType<HomeCommand>(sequence.Commands[0]);
        Assert.Equal(
            new CartesianPosition(10, 20, 0),
            Assert.IsType<MoveToCommand>(sequence.Commands[1]).TargetPosition);
    }

    [Fact]
    public void Parse_WhenRelativeModeHasNoKnownPosition_ShouldThrow()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Parse("G91\nG1 X10"));

        Assert.Equal(2, exception.LineNumber);
        Assert.Contains("G91 relative movement requires a known initial Cartesian position", exception.Message);
    }

    [Theory]
    [InlineData("G90 X0")]
    [InlineData("G91 X0")]
    public void Parse_WhenPositioningModeHasArguments_ShouldThrow(string script)
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Parse($"{script}\nG1 X0 Y0 Z0"));

        Assert.Contains("does not accept arguments", exception.Message);
    }

    [Fact]
    public void Parse_WhenFeedRateIsNotPositive_ShouldThrow()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Parse("G1 X10 Y20 Z5 F0"));

        Assert.Contains("greater than zero", exception.Message);
    }

    [Fact]
    public void Parse_WhenProgramRequestsInches_ShouldExplainMillimeterPolicy()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Parse("G20"));

        Assert.Contains("inch units are not supported", exception.Message);
        Assert.Contains("use G21", exception.Message);
    }

    [Fact]
    public void Parse_WhenG4OmitsP_ShouldExplainDurationSyntax()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Parse("G4 S1"));

        Assert.Contains("Unexpected G-code word 'S'", exception.Message);
    }

    [Fact]
    public void Parse_WhenCodeIsUnsupported_ShouldListSupportedCommands()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Parse("G92"));

        Assert.Contains("Supported commands are G28, G1, G4, G21, G90, and G91", exception.Message);
    }

    [Fact]
    public void Parse_WhenWordIsDuplicated_ShouldThrow()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Parse("G1 X10 X20 Y20 Z5"));

        Assert.Contains("Duplicate G-code word 'X'", exception.Message);
    }

    [Fact]
    public void Write_WhenSequenceIsSupported_ShouldProduceEquivalentGCode()
    {
        var source = new RobotScriptParser().Parse(
            """
            HOME
            MOVE X=10 Y=20 Z=5 SPEED=100
            WAIT 500
            """);

        var gCode = GCodeWriter.Write(source);
        var result = new GCodeParser().Compile(gCode);

        Assert.Equal("G21\r\nG90\r\nG28\r\nG1 X10 Y20 Z5 F6000\r\nG4 P500", gCode);
        Assert.IsType<RobotScriptUnitStatement>(result.Statements[0]);
        Assert.IsType<RobotScriptPositioningModeStatement>(result.Statements[1]);
        Assert.IsType<HomeCommand>(result.Commands.Commands[0]);
        Assert.Equal(100, Assert.IsType<MoveToCommand>(result.Commands.Commands[1]).RequestedVelocityMillimetersPerSecond);
        Assert.Equal(TimeSpan.FromMilliseconds(500), Assert.IsType<WaitCommand>(result.Commands.Commands[2]).Duration);
    }
}
