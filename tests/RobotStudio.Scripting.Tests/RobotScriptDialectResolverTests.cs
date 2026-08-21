namespace RobotStudio.Scripting.Tests;

public sealed class RobotScriptDialectResolverTests
{
    [Theory]
    [InlineData("dsl", RobotScriptDialectId.SimpleDsl)]
    [InlineData("simple-dsl", RobotScriptDialectId.SimpleDsl)]
    [InlineData("gcode", RobotScriptDialectId.GCode)]
    [InlineData("g-code", RobotScriptDialectId.GCode)]
    public void Resolve_WhenDialectIsExplicit_ShouldUseRequestedDialect(
        string name,
        RobotScriptDialectId expectedId)
    {
        var dialect = RobotScriptDialectResolver.Resolve(name, "lesson.robot");

        Assert.Equal(expectedId, dialect.Descriptor.Id);
    }

    [Theory]
    [InlineData("lesson.gcode", RobotScriptDialectId.GCode)]
    [InlineData("LESSON.GCODE", RobotScriptDialectId.GCode)]
    [InlineData("lesson.robot", RobotScriptDialectId.SimpleDsl)]
    [InlineData("lesson.txt", RobotScriptDialectId.SimpleDsl)]
    public void Resolve_WhenDialectIsOmitted_ShouldInferFromExtension(
        string path,
        RobotScriptDialectId expectedId)
    {
        var dialect = RobotScriptDialectResolver.Resolve(scriptPath: path);

        Assert.Equal(expectedId, dialect.Descriptor.Id);
    }

    [Fact]
    public void Resolve_WhenDialectIsUnknown_ShouldThrowWithExpectedNames()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            RobotScriptDialectResolver.Resolve("python"));

        Assert.Contains("Expected 'dsl' or 'gcode'", exception.Message);
    }
}
