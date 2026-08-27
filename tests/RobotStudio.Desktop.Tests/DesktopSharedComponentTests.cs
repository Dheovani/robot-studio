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
