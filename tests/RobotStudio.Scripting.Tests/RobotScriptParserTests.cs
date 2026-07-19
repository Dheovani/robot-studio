using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting.Tests;

public sealed class RobotScriptParserTests
{
    [Fact]
    public void Parse_WhenScriptIsValid_ShouldReturnCommandSequence()
    {
        var parser = new RobotScriptParser();

        var sequence = parser.Parse(
            """
            HOME
            MOVE X=10 Y=20 Z=5 SPEED=100
            WAIT 500
            """);

        Assert.IsType<HomeCommand>(sequence.Commands[0]);

        var move = Assert.IsType<MoveToCommand>(sequence.Commands[1]);
        Assert.Equal(10, move.TargetPosition.X);
        Assert.Equal(20, move.TargetPosition.Y);
        Assert.Equal(5, move.TargetPosition.Z);
        Assert.Equal(100, move.RequestedVelocityMillimetersPerSecond);

        var wait = Assert.IsType<WaitCommand>(sequence.Commands[2]);
        Assert.Equal(TimeSpan.FromMilliseconds(500), wait.Duration);
    }

    [Fact]
    public void Parse_WhenMoveHasNoSpeed_ShouldReturnMoveCommandWithoutRequestedVelocity()
    {
        var parser = new RobotScriptParser();

        var sequence = parser.Parse("MOVE X=10 Y=20 Z=5");

        var move = Assert.IsType<MoveToCommand>(Assert.Single(sequence.Commands));
        Assert.Null(move.RequestedVelocityMillimetersPerSecond);
    }

    [Fact]
    public void Parse_WhenCommandIsUnknown_ShouldThrowWithLineNumber()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("JUMP X=10"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("Unknown command", exception.Message);
    }

    [Fact]
    public void Parse_WhenMoveMissesCoordinate_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("MOVE X=10 Y=20"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("MOVE requires Z", exception.Message);
    }

    [Fact]
    public void Parse_WhenNumberIsInvalid_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("MOVE X=abc Y=20 Z=5"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("X must be a valid number", exception.Message);
    }

    [Fact]
    public void Parse_WhenWaitDurationIsNegative_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("WAIT -1"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("WAIT duration cannot be negative", exception.Message);
    }

    [Fact]
    public void Parse_WhenScriptIsEmpty_ShouldThrowCommandSequenceError()
    {
        var parser = new RobotScriptParser();

        Assert.Throws<RobotStudio.Domain.Exceptions.InvalidRobotCommandException>(() => parser.Parse(""));
    }
}
