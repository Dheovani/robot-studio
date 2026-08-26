using System.Windows;

namespace RobotStudio.Desktop.Localization;

internal sealed class UiLanguageService
{
    private const string ResourcePathPrefix =
        "/RobotStudio.Desktop;component/Localization/Strings.";

    public static IReadOnlyList<UiLanguageOption> AvailableLanguages { get; } =
    [
        new(UiLanguage.English, "English"),
        new(UiLanguage.PortugueseBrazil, "Português (Brasil)")
    ];

    public UiLanguage CurrentLanguage { get; private set; } = UiLanguage.English;

    public void Apply(UiLanguage language)
    {
        var resources = Application.Current.Resources;
        var currentDictionary = resources.MergedDictionaries.FirstOrDefault(dictionary =>
            dictionary.Source?.OriginalString.Contains(ResourcePathPrefix, StringComparison.Ordinal) == true);
        var nextDictionary = new ResourceDictionary
        {
            Source = new Uri(GetResourcePath(language), UriKind.Relative)
        };

        if (currentDictionary is null)
        {
            resources.MergedDictionaries.Add(nextDictionary);
        }
        else
        {
            var index = resources.MergedDictionaries.IndexOf(currentDictionary);
            resources.MergedDictionaries[index] = nextDictionary;
        }

        CurrentLanguage = language;
    }

    public string GetText(string key) => GetText(key, key);

    public string GetText(string key, string fallback) =>
        Application.Current.TryFindResource(key) as string ?? fallback;

    internal static string GetResourcePath(UiLanguage language) => language switch
    {
        UiLanguage.English => $"{ResourcePathPrefix}en.xaml",
        UiLanguage.PortugueseBrazil => $"{ResourcePathPrefix}pt-BR.xaml",
        _ => throw new ArgumentOutOfRangeException(nameof(language))
    };
}
