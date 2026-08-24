namespace RobotStudio.Architecture.Tests;

public sealed class SourceFileMaintainabilityTests
{
    private const int MaximumProductionCSharpLines = 1_000;

    [Fact]
    public void ProductionCSharpFiles_ShouldRemainReviewable()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src");
        var oversizedFiles = Directory
            .EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !ContainsGeneratedPathSegment(sourceRoot, path))
            .Select(path => new
            {
                Path = Path.GetRelativePath(sourceRoot, path),
                LineCount = File.ReadLines(path).Count()
            })
            .Where(file => file.LineCount > MaximumProductionCSharpLines)
            .OrderByDescending(file => file.LineCount)
            .ToArray();

        Assert.True(
            oversizedFiles.Length == 0,
            "Production C# files above the 1,000-line maintainability limit:" +
            Environment.NewLine +
            string.Join(
                Environment.NewLine,
                oversizedFiles.Select(file => $"{file.Path}: {file.LineCount} lines")));
    }

    private static bool ContainsGeneratedPathSegment(string sourceRoot, string path) =>
        Path.GetRelativePath(sourceRoot, path)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => segment is "bin" or "obj");

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
