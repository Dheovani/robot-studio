using System.Xml.Linq;

namespace RobotStudio.Desktop.Tests;

public sealed class DesktopScriptWorkflowTests
{
    private static readonly XNamespace Presentation =
        "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace Xaml =
        "http://schemas.microsoft.com/winfx/2006/xaml";

    private static readonly string[] ScriptEditorNames =
    [
        "ScriptEditorTextBox",
        "DifferentialDriveScriptTextBox",
        "ScaraScriptTextBox",
        "SimpleArmScriptTextBox",
        "DeltaScriptTextBox",
        "DroneScriptTextBox",
        "IndustrialArmScriptTextBox"
    ];

    private static readonly string[] ScriptStatusNames =
    [
        "ScriptStatusText",
        "DifferentialDriveScriptStatusText",
        "ScaraScriptStatusText",
        "SimpleArmScriptStatusText",
        "DeltaScriptStatusText",
        "DroneScriptStatusText",
        "IndustrialArmScriptStatusText"
    ];

    [Fact]
    public void EveryScriptEditor_ShouldUseAutomaticValidation()
    {
        var document = XDocument.Load(MainWindowPath());

        foreach (var editorName in ScriptEditorNames)
        {
            var editor = FindNamedElement(document, editorName);

            Assert.Equal("ScriptTextBox_TextChanged", (string?)editor.Attribute("TextChanged"));
        }
    }

    [Fact]
    public void ScriptActions_ShouldExposeSimulateWithoutManualValidation()
    {
        var document = XDocument.Load(MainWindowPath());
        var actions = document
            .Descendants(Presentation + "DataTemplate")
            .Single(element => (string?)element.Attribute(Xaml + "Key") == "ViewerScriptActionsTemplate");
        var contents = actions
            .Descendants(Presentation + "Button")
            .Select(button => (string?)button.Attribute("Content"))
            .OfType<string>()
            .ToArray();

        Assert.Contains("{DynamicResource Common.Simulate}", contents);
        Assert.DoesNotContain("{DynamicResource Common.Validate}", contents);
    }

    [Fact]
    public void EveryScriptStatus_ShouldUseCompactSharedPresentation()
    {
        var document = XDocument.Load(MainWindowPath());

        foreach (var statusName in ScriptStatusNames)
        {
            var status = FindNamedElement(document, statusName);

            Assert.Equal(
                "{StaticResource ViewerScriptStatusStyle}",
                (string?)status.Attribute("Style"));
        }
    }

    private static XElement FindNamedElement(XDocument document, string name) => document
        .Descendants()
        .Single(element => (string?)element.Attribute(Xaml + "Name") == name);

    private static string MainWindowPath() => Path.Combine(
        FindRepositoryRoot(),
        "src",
        "RobotStudio.Desktop",
        "MainWindow.xaml");

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
