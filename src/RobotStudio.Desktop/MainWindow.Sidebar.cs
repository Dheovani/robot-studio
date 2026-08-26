using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using RobotStudio.Desktop.Robots;
using RobotStudio.Desktop.Viewers;

namespace RobotStudio.Desktop;

public partial class MainWindow
{
    private readonly SidebarNavigationState sidebarNavigation = new();

    private void SidebarAreaButton_Checked(
        object sender,
        RoutedEventArgs e)
    {
        if (!IsLoaded ||
            sender is not RadioButton { Tag: string areaName } ||
            !Enum.TryParse<SidebarArea>(areaName, out var area))
        {
            return;
        }

        SelectSidebarArea(area);
    }

    private void SelectSidebarArea(
        SidebarArea area,
        bool restoreScrollPosition = true)
    {
        var targetOffset = sidebarNavigation.Select(
            area,
            CartesianSidebarScrollViewer.VerticalOffset);

        ScriptSidebarSection.Visibility = AreaVisibility(SidebarArea.Script);
        ScriptConsoleSection.Visibility = AreaVisibility(SidebarArea.Script);
        ControlManualSection.Visibility = AreaVisibility(SidebarArea.Control);
        CartesianProfileExpander.Visibility =
            sidebarNavigation.IsSelected(SidebarArea.Control) &&
            activeViewerKind == RobotViewerKind.CartesianThreeDimensional
                ? Visibility.Visible
                : Visibility.Collapsed;
        MonitorStateSection.Visibility = AreaVisibility(SidebarArea.Monitor);
        MonitorChartsSection.Visibility = AreaVisibility(SidebarArea.Monitor);
        ViewOverlaysSection.Visibility = AreaVisibility(SidebarArea.View);
        ViewCameraSection.Visibility = AreaVisibility(SidebarArea.View);
        UpdateContextualMonitorSections();

        if (!restoreScrollPosition)
        {
            targetOffset = 0;
        }

        Dispatcher.BeginInvoke(
            () => CartesianSidebarScrollViewer.ScrollToVerticalOffset(targetOffset),
            DispatcherPriority.Loaded);
    }

    private void ResetSidebarNavigation()
    {
        ScriptSidebarAreaButton.IsChecked = true;
        SelectSidebarArea(SidebarArea.Script, restoreScrollPosition: false);
    }

    private void UpdateContextualMonitorSections()
    {
        var isMonitorSelected = sidebarNavigation.IsSelected(SidebarArea.Monitor);
        var hasCurrentCommand = snapshot is not null &&
                                currentFrameIndex >= 0 &&
                                currentFrameIndex < snapshot.SceneFrameCount &&
                                snapshot.SceneFrames[currentFrameIndex].CommandSource is not null;
        var hasMarkers = CommandMarkersListBox.Items.Count > 0 ||
                         StateMarkersListBox.Items.Count > 0;

        MonitorExplanationSection.Visibility = isMonitorSelected && hasCurrentCommand
            ? Visibility.Visible
            : Visibility.Collapsed;
        MonitorTimelineSection.Visibility = isMonitorSelected && hasMarkers
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private Visibility AreaVisibility(SidebarArea area) =>
        sidebarNavigation.IsSelected(area)
            ? Visibility.Visible
            : Visibility.Collapsed;
}
