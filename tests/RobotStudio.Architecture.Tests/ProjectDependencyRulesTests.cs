using System.Xml.Linq;

namespace RobotStudio.Architecture.Tests;

public sealed class ProjectDependencyRulesTests
{
    private static readonly IReadOnlyDictionary<string, string[]> ExpectedReferences =
        new Dictionary<string, string[]>
        {
            ["RobotStudio.Domain"] = [],
            ["RobotStudio.Motion"] = ["RobotStudio.Domain"],
            ["RobotStudio.Simulation"] = ["RobotStudio.Domain", "RobotStudio.Motion"],
            ["RobotStudio.Scripting"] = ["RobotStudio.Domain", "RobotStudio.Motion"],
            ["RobotStudio.Hardware"] = ["RobotStudio.Domain"],
            ["RobotStudio.Cli"] = ["RobotStudio.Domain", "RobotStudio.Scripting", "RobotStudio.Simulation"],
            ["RobotStudio.Desktop"] = ["RobotStudio.Domain", "RobotStudio.Scripting", "RobotStudio.Simulation"]
        };

    [Fact]
    public void SourceProjects_ShouldFollowAllowedProjectReferenceMap()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sourceProjects = EnumerateSourceProjects(repositoryRoot)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedReferences.Keys.Order(StringComparer.Ordinal), sourceProjects.Select(GetProjectName).Order(StringComparer.Ordinal));

        foreach (var projectPath in sourceProjects)
        {
            var projectName = GetProjectName(projectPath);
            var actualReferences = ReadProjectReferences(projectPath)
                .Select(GetProjectName)
                .Order(StringComparer.Ordinal)
                .ToArray();
            var expectedReferences = ExpectedReferences[projectName]
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(expectedReferences, actualReferences);
        }
    }

    [Fact]
    public void DomainProject_ShouldRemainPure()
    {
        var repositoryRoot = FindRepositoryRoot();
        var domainProjectPath = Path.Combine(repositoryRoot, "src", "RobotStudio.Domain", "RobotStudio.Domain.csproj");
        var document = XDocument.Load(domainProjectPath);

        Assert.Empty(ReadProjectReferences(domainProjectPath));
        Assert.Empty(document.Descendants("PackageReference"));
        Assert.DoesNotContain(
            document.Descendants("TargetFramework").Select(element => element.Value),
            framework => framework.Contains("windows", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DesktopProject_ShouldBeTheOnlyWpfSourceProject()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sourceProjects = EnumerateSourceProjects(repositoryRoot).ToArray();

        var wpfProjects = sourceProjects
            .Where(projectPath => HasProperty(projectPath, "UseWPF", "true"))
            .Select(GetProjectName)
            .ToArray();

        Assert.Equal(["RobotStudio.Desktop"], wpfProjects);
    }

    [Fact]
    public void PortableSolution_ShouldExcludeDesktopProject()
    {
        var repositoryRoot = FindRepositoryRoot();
        var portableSolutionPath = Path.Combine(repositoryRoot, "build", "RobotStudio.Portable.slnx");
        var portableSolution = File.ReadAllText(portableSolutionPath);

        Assert.DoesNotContain("RobotStudio.Desktop", portableSolution);
        Assert.Contains("RobotStudio.Cli", portableSolution);
        Assert.Contains("RobotStudio.Domain", portableSolution);
    }

    [Theory]
    [InlineData(@"..\RobotStudio.Domain\RobotStudio.Domain.csproj")]
    [InlineData("../RobotStudio.Domain/RobotStudio.Domain.csproj")]
    public void GetProjectName_WhenReferenceUsesEitherDirectorySeparator_ShouldReturnProjectName(
        string projectReference)
    {
        Assert.Equal("RobotStudio.Domain", GetProjectName(projectReference));
    }

    [Fact]
    public void IsTemporaryWpfProject_WhenSdkGeneratesBuildProject_ShouldReturnTrue()
    {
        Assert.True(IsTemporaryWpfProject("RobotStudio.Desktop_ab12cd34_wpftmp.csproj"));
        Assert.False(IsTemporaryWpfProject("RobotStudio.Desktop.csproj"));
    }

    private static IReadOnlyList<string> ReadProjectReferences(string projectPath)
    {
        var document = XDocument.Load(projectPath);
        var projectDirectory = Path.GetDirectoryName(projectPath)
            ?? throw new InvalidOperationException($"Could not get project directory for {projectPath}.");

        return document
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => NormalizePathSeparators(include!))
            .Select(include => Path.GetFullPath(Path.Combine(projectDirectory, include)))
            .ToArray();
    }

    private static IEnumerable<string> EnumerateSourceProjects(string repositoryRoot) =>
        Directory
            .EnumerateFiles(Path.Combine(repositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(projectPath => !ContainsGeneratedPathSegment(projectPath))
            .Where(projectPath => !IsTemporaryWpfProject(projectPath));

    private static bool IsTemporaryWpfProject(string projectPath) =>
        GetProjectName(projectPath).EndsWith("_wpftmp", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsGeneratedPathSegment(string projectPath)
    {
        var segments = Path
            .GetRelativePath(FindRepositoryRoot(), projectPath)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return segments.Any(segment => segment is "bin" or "obj");
    }

    private static bool HasProperty(
        string projectPath,
        string propertyName,
        string expectedValue)
    {
        var document = XDocument.Load(projectPath);

        return document
            .Descendants(propertyName)
            .Any(element => string.Equals(element.Value, expectedValue, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetProjectName(string projectPath) =>
        Path.GetFileNameWithoutExtension(NormalizePathSeparators(projectPath));

    private static string NormalizePathSeparators(string path) =>
        path
            .Replace(Path.DirectorySeparatorChar == '\\' ? '/' : '\\', Path.DirectorySeparatorChar);

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RobotStudio.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the RobotStudio repository root.");
    }
}
