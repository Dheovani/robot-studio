using RobotStudio.Desktop.Scripting;
using RobotStudio.Scripting;

namespace RobotStudio.Desktop.Tests;

public sealed class GCodeLineExplanationBuilderTests
{
    [Fact]
    public void Build_WhenProgramUsesSupportedCommands_ShouldExplainEveryExecutableLine()
    {
        const string script = "G21\nG90\nG28\nG1 X100 Y50 Z20 F4800\nG4 P500";

        var explanations = GCodeLineExplanationBuilder.Build(
            script,
            GCodeRobotMappingCatalog.Get(GCodeRobotTarget.CartesianRobot));

        Assert.Equal(["G21", "G90", "G28", "G1", "G4"], explanations.Select(item => item.Command));
        Assert.Contains("X/Y/Z", explanations[3].Explanation);
        Assert.Contains("millimeters per minute", explanations[3].Explanation);
    }

    [Fact]
    public void Build_WhenModeChanges_ShouldExplainRelativeLinearMovement()
    {
        var explanations = GCodeLineExplanationBuilder.Build(
            "G90\nG91\nG1 X10",
            GCodeRobotMappingCatalog.Get(GCodeRobotTarget.XYPlotter));

        Assert.Contains("relative tool-space", explanations[2].Explanation);
        Assert.Contains("X/Y", explanations[2].Explanation);
    }

    [Fact]
    public void Build_WhenIndustrialArmMoves_ShouldExplainAllPoseWords()
    {
        var explanation = Assert.Single(GCodeLineExplanationBuilder.Build(
            "N40 G01 X300 Y0 Z180 A20 B10 C0 F4200 ; tool pose",
            GCodeRobotMappingCatalog.Get(GCodeRobotTarget.IndustrialArm6Dof)));

        Assert.Equal(1, explanation.LineNumber);
        Assert.Equal("G1", explanation.Command);
        Assert.Contains("X/Y/Z", explanation.Explanation);
        Assert.Contains("A/B/C", explanation.Explanation);
        Assert.Contains("roll, pitch, and yaw", explanation.Explanation);
    }

    [Fact]
    public void Build_WhenLineContainsOnlyCommentOrUnknownCommand_ShouldOmitIt()
    {
        var explanations = GCodeLineExplanationBuilder.Build(
            "; comment\n(another comment)\nM3",
            GCodeRobotMappingCatalog.Get(GCodeRobotTarget.CartesianRobot));

        Assert.Empty(explanations);
    }
}
