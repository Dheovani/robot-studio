using System.Windows;
using System.Windows.Controls;
using RobotStudio.Desktop.Scripting;
using RobotStudio.Scripting;

namespace RobotStudio.Desktop.Viewers;

public partial class GCodeExplanationPanel : UserControl
{
    private string script = string.Empty;
    private GCodeRobotMappingDescriptor? mapping;

    public GCodeExplanationPanel()
    {
        InitializeComponent();
    }

    public void SetContext(
        bool isGCode,
        string currentScript,
        GCodeRobotTarget target)
    {
        script = currentScript ?? string.Empty;
        mapping = GCodeRobotMappingCatalog.Get(target);
        Visibility = isGCode ? Visibility.Visible : Visibility.Collapsed;
        RefreshExplanations();
    }

    private void ExplanationToggle_Changed(object sender, RoutedEventArgs e) =>
        RefreshExplanations();

    private void RefreshExplanations()
    {
        if (ExplanationContent is null || ExplanationItems is null)
        {
            return;
        }

        var showExplanations = Visibility == Visibility.Visible && ExplanationToggle.IsChecked == true;
        ExplanationContent.Visibility = showExplanations ? Visibility.Visible : Visibility.Collapsed;
        ExplanationItems.ItemsSource = showExplanations && mapping is not null
            ? GCodeLineExplanationBuilder.Build(script, mapping)
            : null;
    }
}
