using RobotStudio.Cli;

namespace RobotStudio.Cli.Tests;

public sealed class CliCommandLineTests
{
    [Fact]
    public void Parse_WhenDialectFollowsArguments_ShouldSeparateOption()
    {
        var commandLine = CliCommandLine.Parse(
            ["simulate", "lesson.txt", "--dialect", "gcode"]);

        Assert.Equal(["simulate", "lesson.txt"], commandLine.Arguments);
        Assert.Equal("gcode", commandLine.DialectName);
    }

    [Fact]
    public void Parse_WhenDialectUsesEqualsSyntax_ShouldSeparateOption()
    {
        var commandLine = CliCommandLine.Parse(
            ["--dialect=dsl", "validate", "lesson.txt"]);

        Assert.Equal(["validate", "lesson.txt"], commandLine.Arguments);
        Assert.Equal("dsl", commandLine.DialectName);
    }

    [Theory]
    [InlineData("--dialect")]
    [InlineData("--dialect=")]
    public void Parse_WhenDialectValueIsMissing_ShouldThrow(string option)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CliCommandLine.Parse(["simulate", "lesson.txt", option]));

        Assert.Contains("requires", exception.Message);
    }

    [Fact]
    public void Parse_WhenOptionIsUnknown_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CliCommandLine.Parse(["simulate", "lesson.robot", "--language", "dsl"]));

        Assert.Contains("Unknown option", exception.Message);
    }
}
