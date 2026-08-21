using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting.Tests;

public sealed class GCodeParserTests
{
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
    public void Parse_WhenG1OmitsCoordinate_ShouldExplainRequiredCoordinate()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Parse("G1 X10 Y20"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("requires a Z coordinate", exception.Message);
    }

    [Fact]
    public void Parse_WhenFeedRateIsNotPositive_ShouldThrow()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            new GCodeParser().Parse("G1 X10 Y20 Z5 F0"));

        Assert.Contains("greater than zero", exception.Message);
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
            new GCodeParser().Parse("G90"));

        Assert.Contains("Supported commands are G28, G1, and G4", exception.Message);
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
        var result = new GCodeParser().Parse(gCode);

        Assert.Equal("G28\r\nG1 X10 Y20 Z5 F6000\r\nG4 P500", gCode);
        Assert.IsType<HomeCommand>(result.Commands[0]);
        Assert.Equal(100, Assert.IsType<MoveToCommand>(result.Commands[1]).RequestedVelocityMillimetersPerSecond);
        Assert.Equal(TimeSpan.FromMilliseconds(500), Assert.IsType<WaitCommand>(result.Commands[2]).Duration);
    }
}
