using RobotStudio.Desktop.Viewers;

namespace RobotStudio.Desktop.Tests;

public sealed class ResponsiveGridLayoutTests
{
    [Theory]
    [InlineData(620, 1)]
    [InlineData(760, 2)]
    [InlineData(1160, 3)]
    [InlineData(1880, 5)]
    [InlineData(2560, 6)]
    public void CalculateColumnCount_WhenWidthChanges_ShouldUseAvailableSpace(
        double availableWidth,
        int expectedColumns)
    {
        var columns = ResponsiveGridLayout.CalculateColumnCount(
            availableWidth,
            preferredItemWidth: 360,
            gap: 18,
            maximumColumns: 6);

        Assert.Equal(expectedColumns, columns);
    }

    [Fact]
    public void CalculateColumnCount_WhenWidthIsUnknown_ShouldReturnOneColumn()
    {
        var columns = ResponsiveGridLayout.CalculateColumnCount(
            availableWidth: 0,
            preferredItemWidth: 360,
            gap: 18,
            maximumColumns: 6);

        Assert.Equal(1, columns);
    }
}
