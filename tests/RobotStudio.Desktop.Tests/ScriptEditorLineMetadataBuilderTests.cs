using RobotStudio.Desktop.Scripting;

namespace RobotStudio.Desktop.Tests;

public sealed class ScriptEditorLineMetadataBuilderTests
{
    [Fact]
    public void Build_WhenScriptContainsSupportedCommands_ShouldReturnLineNumbersAndKinds()
    {
        const string script =
            """
            HOME
            MOVE X=10 Y=20 Z=5 SPEED=100
            WAIT 500
            """;

        var metadata = ScriptEditorLineMetadataBuilder.Build(script);

        Assert.Collection(
            metadata,
            line =>
            {
                Assert.Equal(1, line.LineNumber);
                Assert.Equal("HOME", line.CommandText);
                Assert.Equal(ScriptEditorLineKind.Home, line.Kind);
            },
            line =>
            {
                Assert.Equal(2, line.LineNumber);
                Assert.Equal("MOVE", line.CommandText);
                Assert.Equal(ScriptEditorLineKind.Move, line.Kind);
            },
            line =>
            {
                Assert.Equal(3, line.LineNumber);
                Assert.Equal("WAIT", line.CommandText);
                Assert.Equal(ScriptEditorLineKind.Wait, line.Kind);
            });
    }

    [Fact]
    public void Build_WhenScriptContainsBlankAndUnknownLines_ShouldClassifyThemPredictably()
    {
        const string script =
            """
            HOME

            SPIN SPEED=10
            """;

        var metadata = ScriptEditorLineMetadataBuilder.Build(script);

        Assert.Collection(
            metadata,
            line => Assert.Equal(ScriptEditorLineKind.Home, line.Kind),
            line => Assert.Equal(ScriptEditorLineKind.Empty, line.Kind),
            line =>
            {
                Assert.Equal("SPIN", line.CommandText);
                Assert.Equal(ScriptEditorLineKind.Other, line.Kind);
            });
    }

    [Fact]
    public void Build_WhenScriptContainsGCode_ShouldReuseDidacticCommandKinds()
    {
        const string script =
            """
            G28
            G1 X10 Y20 Z5 F6000
            G4 P500
            """;

        var metadata = ScriptEditorLineMetadataBuilder.Build(script);

        Assert.Collection(
            metadata,
            line => Assert.Equal(ScriptEditorLineKind.Home, line.Kind),
            line => Assert.Equal(ScriptEditorLineKind.Move, line.Kind),
            line => Assert.Equal(ScriptEditorLineKind.Wait, line.Kind));
    }
}
