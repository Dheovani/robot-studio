using RobotStudio.Desktop.Localization;

namespace RobotStudio.Desktop.Tests;

public sealed class UiLanguageServiceTests
{
    [Fact]
    public void AvailableLanguages_ShouldContainEnglishAndPortugueseBrazil()
    {
        var languages = UiLanguageService.AvailableLanguages
            .Select(option => option.Language)
            .ToArray();

        Assert.Equal(
            [UiLanguage.English, UiLanguage.PortugueseBrazil],
            languages);
    }

    [Theory]
    [InlineData(UiLanguage.English, "Strings.en.xaml")]
    [InlineData(UiLanguage.PortugueseBrazil, "Strings.pt-BR.xaml")]
    internal void GetResourcePath_WhenLanguageIsSupported_ShouldUseItsDictionary(
        UiLanguage language,
        string expectedFileName)
    {
        var resourcePath = UiLanguageService.GetResourcePath(language);

        Assert.EndsWith(expectedFileName, resourcePath, StringComparison.Ordinal);
    }
}
