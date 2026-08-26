namespace RobotStudio.Desktop.Localization;

internal enum UiLanguage
{
    English,
    PortugueseBrazil
}

internal sealed record UiLanguageOption(
    UiLanguage Language,
    string DisplayName)
{
    public override string ToString() => DisplayName;
}
