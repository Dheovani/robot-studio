namespace RobotStudio.Desktop.Viewers;

internal enum SidebarArea
{
    Script,
    Control,
    Monitor,
    View
}

internal sealed class SidebarNavigationState
{
    private readonly Dictionary<SidebarArea, double> scrollOffsets = [];

    public SidebarArea SelectedArea { get; private set; } = SidebarArea.Script;

    public bool IsSelected(SidebarArea area) => SelectedArea == area;

    public double Select(SidebarArea area, double currentScrollOffset)
    {
        if (currentScrollOffset < 0 || !double.IsFinite(currentScrollOffset))
        {
            throw new ArgumentOutOfRangeException(nameof(currentScrollOffset));
        }

        scrollOffsets[SelectedArea] = currentScrollOffset;
        SelectedArea = area;
        return scrollOffsets.GetValueOrDefault(area);
    }
}
