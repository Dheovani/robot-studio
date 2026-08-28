using System.Xml.Linq;

namespace RobotStudio.Desktop.Tests;

public sealed class DesktopSharedComponentTests
{
    private static readonly XNamespace Presentation =
        "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    [Fact]
    public void EverySimulationWorkspace_ShouldUseSharedViewerHeader()
    {
        var mainWindow = XDocument.Load(DesktopPath("MainWindow.xaml"));
        var headers = mainWindow
            .Descendants()
            .Where(element => element.Name.LocalName == "ViewerHeader")
            .ToArray();

        Assert.Equal(7, headers.Length);
        Assert.All(headers, header =>
        {
            Assert.NotNull(header.Attribute("Title"));
            Assert.NotNull(header.Attribute("Subtitle"));
            Assert.Contains(header.Elements(), element => element.Name.LocalName == "ViewerHeader.Actions");
        });
    }

    [Fact]
    public void MechanicalShowcase_ShouldUseSharedViewerHeader()
    {
        var showcase = XDocument.Load(DesktopPath(
            "Showcases",
            "MechanicalShowcaseView.xaml"));
        var headers = showcase
            .Descendants()
            .Where(element => element.Name.LocalName == "ViewerHeader")
            .ToArray();

        Assert.Single(headers);
        Assert.Contains(
            headers[0].Elements(),
            element => element.Name.LocalName == "ViewerHeader.Actions");
    }

    [Fact]
    public void ViewerHeader_ShouldOwnHeaderLayoutAndTypography()
    {
        var component = XDocument.Load(DesktopPath("Viewers", "ViewerHeader.xaml"));
        var dockPanel = component.Descendants(Presentation + "DockPanel").Single();

        Assert.Equal("{DynamicResource ViewerHeaderStyle}", (string?)dockPanel.Attribute("Style"));
        Assert.Equal(2, component.Descendants(Presentation + "TextBlock").Count());
        Assert.Single(component.Descendants(Presentation + "ContentPresenter"));
    }

    [Fact]
    public void SimulationTimelines_ShouldUseSharedTimelineSliderStyle()
    {
        var mainWindow = XDocument.Load(DesktopPath("MainWindow.xaml"));
        var sharedTimeline = XDocument.Load(DesktopPath("Viewers", "ViewerTimeline.xaml"));

        var cartesianSlider = mainWindow
            .Descendants(Presentation + "Slider")
            .Single(element => AttributeValue(element, "Name") == "TimelineSlider");
        var sharedSlider = sharedTimeline
            .Descendants(Presentation + "Slider")
            .Single(element => AttributeValue(element, "Name") == "FrameSlider");

        Assert.Equal(
            "{DynamicResource TimelineSliderStyle}",
            (string?)cartesianSlider.Attribute("Style"));
        Assert.Equal(
            "{DynamicResource TimelineSliderStyle}",
            (string?)sharedSlider.Attribute("Style"));
    }

    [Fact]
    public void TimelineSliderStyle_ShouldProvideProgressTrackAndDraggableThumb()
    {
        var styles = XDocument.Load(DesktopPath("Styles", "MainWindowStyles.xaml"));
        var timelineStyle = styles
            .Descendants(Presentation + "Style")
            .Single(element => AttributeValue(element, "Key") == "TimelineSliderStyle");

        Assert.Single(timelineStyle.Descendants(Presentation + "Track"));
        Assert.Single(timelineStyle.Descendants(Presentation + "Thumb"));
        Assert.Equal(2, timelineStyle.Descendants(Presentation + "RepeatButton").Count());
    }

    [Fact]
    public void SimulationTimelines_ShouldExposeSharedPlaybackStateBadge()
    {
        var mainWindow = XDocument.Load(DesktopPath("MainWindow.xaml"));
        var sharedTimeline = XDocument.Load(DesktopPath("Viewers", "ViewerTimeline.xaml"));

        Assert.Contains(
            mainWindow.Descendants(),
            element =>
                element.Name.LocalName == "PlaybackStateBadge" &&
                AttributeValue(element, "Name") == "CartesianTimelineStateBadge");
        Assert.Single(
            sharedTimeline.Descendants(),
            element => element.Name.LocalName == "PlaybackStateBadge");
    }

    [Fact]
    public void EverySimulationTimeline_ShouldExposePlaybackSpeedSelection()
    {
        var mainWindow = XDocument.Load(DesktopPath("MainWindow.xaml"));
        var sharedTimeline = XDocument.Load(DesktopPath("Viewers", "ViewerTimeline.xaml"));
        var sharedSpeedSelector = sharedTimeline
            .Descendants(Presentation + "ComboBox")
            .Single(element => AttributeValue(element, "Name") == "PlaybackSpeedComboBox");
        var sharedTimelines = mainWindow
            .Descendants()
            .Where(element => element.Name.LocalName == "ViewerTimeline")
            .ToArray();

        Assert.Equal(4, sharedSpeedSelector.Elements(Presentation + "ComboBoxItem").Count());
        Assert.Equal(6, sharedTimelines.Length);
        Assert.All(sharedTimelines, timeline => Assert.Equal(
            "ViewerPlaybackSpeed_SelectionChanged",
            AttributeValue(timeline, "PlaybackSpeedChanged")));
    }

    [Fact]
    public void PlaybackStateBadge_ShouldExplainStationaryPlaybackStates()
    {
        var source = File.ReadAllText(DesktopPath("Viewers", "PlaybackStateBadge.xaml.cs"));

        Assert.Contains("Waiting · pose held", source, StringComparison.Ordinal);
        Assert.Contains("WAIT command keeps the robot pose fixed", source, StringComparison.Ordinal);
        Assert.Contains("Completed · final pose", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ViewerChrome_ShouldUseBandsAndSingleAxisDividers()
    {
        var styles = XDocument.Load(DesktopPath("Styles", "MainWindowStyles.xaml"));
        var headerSurface = FindStyle(styles, "ViewerHeaderSurfaceStyle");
        var sidebar = FindStyle(styles, "ViewerSidebarStyle");
        var timelineSurface = FindStyle(styles, "ViewerTimelineSurfaceStyle");

        AssertStyleSetter(headerSurface, "BorderThickness", "0,0,0,1");
        AssertStyleSetter(sidebar, "BorderThickness", "1,0,0,0");
        AssertStyleSetter(sidebar, "CornerRadius", "0");
        AssertStyleSetter(timelineSurface, "BorderThickness", "0,1,0,0");
        AssertStyleSetter(timelineSurface, "CornerRadius", "0");
    }

    [Fact]
    public void ViewerChrome_ShouldExposeResizeGripAndCompactFrameActions()
    {
        var styles = XDocument.Load(DesktopPath("Styles", "MainWindowStyles.xaml"));
        var splitter = FindStyle(styles, "ViewerSplitterStyle");
        var sharedTimeline = XDocument.Load(DesktopPath("Viewers", "ViewerTimeline.xaml"));
        var frameButtons = sharedTimeline.Descendants(Presentation + "Button").ToArray();

        Assert.Contains(
            splitter.Descendants(Presentation + "Border"),
            element => AttributeValue(element, "Name") == "ResizeGrip");
        Assert.Equal(2, frameButtons.Length);
        Assert.All(frameButtons, button =>
        {
            Assert.Equal("34", AttributeValue(button, "Width"));
            Assert.Equal("{DynamicResource GhostButtonStyle}", AttributeValue(button, "Style"));
        });
    }

    private static XElement FindStyle(XDocument styles, string key) => styles
        .Descendants(Presentation + "Style")
        .Single(element => AttributeValue(element, "Key") == key);

    private static void AssertStyleSetter(
        XElement style,
        string property,
        string expectedValue)
    {
        var setter = style
            .Elements(Presentation + "Setter")
            .Single(element => AttributeValue(element, "Property") == property);

        Assert.Equal(expectedValue, AttributeValue(setter, "Value"));
    }

    private static string? AttributeValue(XElement element, string localName) =>
        element.Attributes().SingleOrDefault(attribute => attribute.Name.LocalName == localName)?.Value;

    private static string DesktopPath(params string[] segments) => segments.Aggregate(
        Path.Combine(FindRepositoryRoot(), "src", "RobotStudio.Desktop"),
        Path.Combine);

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RobotStudio.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the RobotStudio repository root.");
    }
}
