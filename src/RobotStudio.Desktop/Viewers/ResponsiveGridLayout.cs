namespace RobotStudio.Desktop.Viewers;

public static class ResponsiveGridLayout
{
    public static int CalculateColumnCount(
        double availableWidth,
        double preferredItemWidth,
        double gap,
        int maximumColumns)
    {
        if (availableWidth <= 0)
        {
            return 1;
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(preferredItemWidth);
        ArgumentOutOfRangeException.ThrowIfNegative(gap);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumColumns);

        var columns = (int)Math.Floor(
            (availableWidth + gap) / (preferredItemWidth + gap));

        return Math.Clamp(columns, 1, maximumColumns);
    }
}
