using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Microsoft.Win32;
using RobotStudio.Desktop.Didactics;
using RobotStudio.Desktop.Examples;
using RobotStudio.Desktop.Profiles;
using RobotStudio.Desktop.Rendering;
using RobotStudio.Desktop.Robots;
using RobotStudio.Desktop.Scripting;
using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Domain.Parallel;
using RobotStudio.Scripting;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop;

public partial class MainWindow
{
    private void OpenGlossaryButton_Click(
        object sender,
        RoutedEventArgs e) =>
        OpenGlossary();

    private void CloseGlossaryButton_Click(
        object sender,
        RoutedEventArgs e) =>
        CloseGlossary();

    private void GlossaryFilter_Changed(
        object sender,
        EventArgs e) =>
        RefreshGlossaryEntries();

    private void ToggleGlossary()
    {
        if (GlossaryOverlay.Visibility == Visibility.Visible)
        {
            CloseGlossary();
            return;
        }

        OpenGlossary();
    }

    private void OpenGlossary()
    {
        StopPlayback();
        GlossaryOverlay.Visibility = Visibility.Visible;
        RefreshGlossaryEntries();
        GlossarySearchTextBox.Focus();
        GlossarySearchTextBox.SelectAll();
    }

    private void CloseGlossary()
    {
        GlossaryOverlay.Visibility = Visibility.Collapsed;
        Focus();
    }

    private void RefreshGlossaryEntries()
    {
        if (GlossaryEntriesListBox is null || GlossaryResultCountText is null)
        {
            return;
        }

        var category = GlossaryCategoryComboBox?.SelectedItem is GlossaryCategory selectedCategory
            ? selectedCategory
            : (GlossaryCategory?)null;
        var entries = GlossaryCatalog.Search(GlossarySearchTextBox?.Text, category);

        GlossaryEntriesListBox.ItemsSource = entries;
        GlossaryResultCountText.Text = entries.Count == 1
            ? "1 term"
            : $"{entries.Count} terms";
    }
}
