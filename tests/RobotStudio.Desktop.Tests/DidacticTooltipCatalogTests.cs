using RobotStudio.Desktop.Didactics;

namespace RobotStudio.Desktop.Tests;

public sealed class DidacticTooltipCatalogTests
{
    public static TheoryData<string> Tooltips => new()
    {
        DidacticTooltipCatalog.AxisLimits,
        DidacticTooltipCatalog.EffectiveVelocity,
        DidacticTooltipCatalog.Homing,
        DidacticTooltipCatalog.Playback,
        DidacticTooltipCatalog.PlannedPath,
        DidacticTooltipCatalog.RequestedVelocity,
        DidacticTooltipCatalog.RobotState,
        DidacticTooltipCatalog.Timeline,
        DidacticTooltipCatalog.ToolCenterPoint,
        DidacticTooltipCatalog.Workspace
    };

    [Theory]
    [MemberData(nameof(Tooltips))]
    public void Tooltips_ShouldProvideReadableDidacticText(string tooltip)
    {
        Assert.False(string.IsNullOrWhiteSpace(tooltip));
        Assert.True(tooltip.Length >= 40);
        Assert.EndsWith(".", tooltip);
    }

    [Fact]
    public void ToolCenterPoint_ShouldExplainTcpAcronym()
    {
        Assert.Contains("TCP", DidacticTooltipCatalog.ToolCenterPoint);
        Assert.Contains("tool center point", DidacticTooltipCatalog.ToolCenterPoint);
    }

    [Fact]
    public void Workspace_ShouldExplainAxisLimits()
    {
        Assert.Contains("axis limits", DidacticTooltipCatalog.Workspace);
    }
}
