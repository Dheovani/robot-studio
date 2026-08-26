using RobotStudio.Desktop.Viewers;

namespace RobotStudio.Desktop.Tests;

public sealed class SidebarNavigationStateTests
{
    [Fact]
    public void Constructor_ShouldSelectScriptArea()
    {
        var state = new SidebarNavigationState();

        Assert.True(state.IsSelected(SidebarArea.Script));
    }

    [Fact]
    public void Select_WhenReturningToArea_ShouldRestoreItsScrollOffset()
    {
        var state = new SidebarNavigationState();

        state.Select(SidebarArea.Monitor, currentScrollOffset: 84);
        state.Select(SidebarArea.View, currentScrollOffset: 220);
        var restoredOffset = state.Select(SidebarArea.Monitor, currentScrollOffset: 12);

        Assert.Equal(220, restoredOffset);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Select_WhenScrollOffsetIsInvalid_ShouldThrow(double scrollOffset)
    {
        var state = new SidebarNavigationState();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            state.Select(SidebarArea.Control, scrollOffset));
    }
}
