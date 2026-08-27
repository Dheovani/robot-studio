using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RobotStudio.Desktop.Tests;

public sealed class DesktopDesignTokenTests
{
    private static readonly XNamespace Presentation =
        "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace Xaml =
        "http://schemas.microsoft.com/winfx/2006/xaml";

    private static readonly string[] RequiredTokenKeys =
    [
        "AppBackgroundBrush",
        "PanelBackgroundBrush",
        "HeaderBackgroundBrush",
        "PrimaryTextBrush",
        "SecondaryTextBrush",
        "MutedTextBrush",
        "DisabledTextBrush",
        "BorderBrush",
        "FocusBorderBrush",
        "AccentBackgroundBrush",
        "AccentBorderBrush",
        "AccentTextBrush",
        "TagBackgroundBrush",
        "ModalOverlayBrush",
        "RobotCardBackgroundBrush",
        "RobotCardAvailableBorderBrush",
        "InputBackgroundBrush",
        "DropdownBackgroundBrush",
        "SelectedItemBackgroundBrush",
        "SelectedItemTextBrush",
        "InformationHeadingBrush",
        "ScrollbarThumbBrush",
        "InfoTextBrush",
        "ErrorSurfaceBrush",
        "ErrorBorderBrush",
        "ErrorTextBrush",
        "AxisXBrush",
        "AxisYBrush",
        "AxisZBrush",
        "PositiveSeriesBrush",
        "NegativeSeriesBrush",
        "RequestedSeriesBrush",
        "EffectiveSeriesBrush",
        "MechanicalInfoSurfaceBrush",
        "ViewportBackgroundColor",
        "AmbientLightColor",
        "KeyLightColor",
        "FillLightColor",
        "StandardControlHeight",
        "CompactControlHeight",
        "ControlCornerRadius",
        "PanelCornerRadius",
        "CardCornerRadius",
        "ControlPadding",
        "InputPadding",
        "PanelPadding",
        "CardPadding",
        "SectionSpacing",
        "TitleFontSize",
        "DisplayTitleFontSize",
        "DialogTitleFontSize",
        "CardTitleFontSize",
        "SectionTitleFontSize",
        "PanelTitleFontSize",
        "BodyFontSize",
        "MetadataFontSize"
    ];

    private static readonly string[] RequiredSemanticStyleKeys =
    [
        "CatalogPageTitleTextStyle",
        "CatalogCardTitleTextStyle",
        "DialogTitleTextStyle",
        "BadgeBorderStyle",
        "BadgeTextStyle",
        "CapabilityBadgeBorderStyle",
        "CapabilityBadgeTextStyle",
        "IconButtonStyle",
        "ViewerStateLabelStyle",
        "ViewerStateValueStyle",
        "ViewerScriptFieldLabelStyle",
        "ViewerStateGridStyle"
    ];

    private static readonly string[] TokenizedViewerNames =
    [
        "DifferentialDriveViewerView",
        "ScaraViewerView",
        "SimpleArmViewerView",
        "DeltaViewerView",
        "DroneViewerView",
        "IndustrialArmViewerView"
    ];

    private static readonly HashSet<string> LegacyNeutralColors = new(StringComparer.OrdinalIgnoreCase)
    {
        "#94A3B8",
        "#E5E7EB",
        "#CBD5E1",
        "#334155",
        "#111827",
        "#0B1220"
    };

    [Fact]
    public void DesignTokens_ShouldDeclareEverySharedVisualRole()
    {
        var document = XDocument.Load(StylePath("DesignTokens.xaml"));
        var keys = document.Root!
            .Elements()
            .Select(element => (string?)element.Attribute(Xaml + "Key"))
            .Where(key => key is not null)
            .ToHashSet(StringComparer.Ordinal);

        Assert.All(RequiredTokenKeys, key => Assert.Contains(key, keys));
    }

    [Fact]
    public void MainStyles_ShouldLoadTokensBeforeControlTemplates()
    {
        var document = XDocument.Load(StylePath("MainWindowStyles.xaml"));
        var sources = document
            .Descendants(Presentation + "ResourceDictionary.MergedDictionaries")
            .Elements(Presentation + "ResourceDictionary")
            .Select(dictionary => (string?)dictionary.Attribute("Source"))
            .OfType<string>()
            .ToArray();

        Assert.Equal(["DesignTokens.xaml", "ControlStyles.xaml"], sources);
    }

    [Fact]
    public void Application_ShouldLoadSharedStylesForStandaloneDesktopViews()
    {
        var document = XDocument.Load(DesktopPath("App.xaml"));
        var sources = document
            .Descendants(Presentation + "ResourceDictionary.MergedDictionaries")
            .Elements(Presentation + "ResourceDictionary")
            .Select(dictionary => (string?)dictionary.Attribute("Source"))
            .OfType<string>()
            .ToArray();

        Assert.Equal(
            ["Localization/Strings.en.xaml", "Styles/MainWindowStyles.xaml"],
            sources);
    }

    [Fact]
    public void MainStyles_ShouldDeclareRequiredSemanticStyles()
    {
        var document = XDocument.Load(StylePath("MainWindowStyles.xaml"));
        var styleKeys = document
            .Descendants(Presentation + "Style")
            .Select(style => (string?)style.Attribute(Xaml + "Key"))
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

        Assert.All(RequiredSemanticStyleKeys, key => Assert.Contains(key, styleKeys));
    }

    [Fact]
    public void NonCartesianViewers_ShouldUseThemeRolesInsteadOfLegacyNeutralColors()
    {
        var document = XDocument.Load(DesktopPath("MainWindow.xaml"));

        foreach (var viewerName in TokenizedViewerNames)
        {
            var viewer = document
                .Descendants()
                .Single(element => (string?)element.Attribute(Xaml + "Name") == viewerName);
            var legacyAttributes = viewer
                .DescendantsAndSelf()
                .Attributes()
                .Where(attribute => LegacyNeutralColors.Contains(attribute.Value))
                .ToArray();

            Assert.True(
                legacyAttributes.Length == 0,
                $"{viewerName} still contains legacy color values: " +
                string.Join(", ", legacyAttributes.Select(attribute => attribute.Value)));
        }
    }

    [Fact]
    public void DesktopXaml_ShouldDeclareColorsOnlyInDesignTokens()
    {
        var desktopDirectory = DesktopPath(string.Empty);
        var colorPattern = new Regex("#[0-9A-Fa-f]{6,8}", RegexOptions.CultureInvariant);
        var violations = Directory
            .EnumerateFiles(desktopDirectory, "*.xaml", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(
                Path.Combine("Styles", "DesignTokens.xaml"),
                StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => colorPattern
                .Matches(File.ReadAllText(path))
                .Select(match => $"{Path.GetRelativePath(desktopDirectory, path)}: {match.Value}"))
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Desktop XAML contains colors outside DesignTokens.xaml: " + string.Join(", ", violations));
    }

    [Fact]
    public void MainStyles_ShouldNotRedeclareLegacyColorTokens()
    {
        var document = XDocument.Load(StylePath("MainWindowStyles.xaml"));
        var declaredKeys = document.Root!
            .Elements()
            .Select(element => (string?)element.Attribute(Xaml + "Key"))
            .Where(key => key is not null)
            .ToArray();

        Assert.DoesNotContain("ButtonBackgroundBrush", declaredKeys);
        Assert.DoesNotContain("ButtonBorderBrush", declaredKeys);
        Assert.DoesNotContain("ButtonHoverBorderBrush", declaredKeys);
    }

    private static string StylePath(string fileName) => Path.Combine(
        DesktopPath("Styles"),
        fileName);

    private static string DesktopPath(string fileName) => Path.Combine(
        FindRepositoryRoot(),
        "src",
        "RobotStudio.Desktop",
        fileName);

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
