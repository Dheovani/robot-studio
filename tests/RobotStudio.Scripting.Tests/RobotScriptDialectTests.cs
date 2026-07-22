namespace RobotStudio.Scripting.Tests;

public sealed class RobotScriptDialectTests
{
    [Fact]
    public void RobotScriptParser_ShouldImplementScriptDialectContract()
    {
        IRobotScriptDialect dialect = new RobotScriptParser();

        Assert.Equal(RobotScriptDialectId.SimpleDsl, dialect.Descriptor.Id);
        Assert.Equal(RobotScriptDialectStatus.Available, dialect.Descriptor.Status);
    }

    [Fact]
    public void RobotScriptDialects_ShouldExposeSimpleDslAsAvailable()
    {
        var simpleDsl = RobotScriptDialects.All.Single(dialect =>
            dialect.Id == RobotScriptDialectId.SimpleDsl);

        Assert.Equal("Simple DSL", simpleDsl.Name);
        Assert.Equal(RobotScriptDialectStatus.Available, simpleDsl.Status);
    }

    [Fact]
    public void RobotScriptDialects_ShouldExposeGCodeAsPlanned()
    {
        var gCode = RobotScriptDialects.All.Single(dialect =>
            dialect.Id == RobotScriptDialectId.GCode);

        Assert.Equal("G-code", gCode.Name);
        Assert.Equal(RobotScriptDialectStatus.Planned, gCode.Status);
    }
}
