using System.Windows;
using System.Windows.Controls;
using RobotStudio.Desktop.Localization;

namespace RobotStudio.Desktop;

public partial class MainWindow
{
    private readonly UiLanguageService languageService = new();

    private void InitializeLanguageSelector()
    {
        LanguageComboBox.ItemsSource = UiLanguageService.AvailableLanguages;
        LanguageComboBox.SelectedItem = UiLanguageService.AvailableLanguages
            .Single(option => option.Language == languageService.CurrentLanguage);
    }

    private void LanguageComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (LanguageComboBox.SelectedItem is not UiLanguageOption selectedLanguage)
        {
            return;
        }

        languageService.Apply(selectedLanguage.Language);
        BuildRobotSelectionCards();
        UpdatePlaybackButtonLabels();
    }
}
