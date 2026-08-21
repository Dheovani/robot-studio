using RobotStudio.Desktop.Didactics;

namespace RobotStudio.Desktop.Tests;

public sealed class GlossaryCatalogTests
{
    [Fact]
    public void All_ShouldProvideAReadableUniqueAlphabeticalCatalog()
    {
        Assert.True(GlossaryCatalog.All.Count >= 40);
        Assert.Equal(
            GlossaryCatalog.All.Count,
            GlossaryCatalog.All.Select(entry => entry.Term).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(
            GlossaryCatalog.All.Select(entry => entry.Term).OrderBy(term => term, StringComparer.OrdinalIgnoreCase),
            GlossaryCatalog.All.Select(entry => entry.Term));
        Assert.All(
            GlossaryCatalog.All,
            entry =>
            {
                Assert.False(string.IsNullOrWhiteSpace(entry.Term));
                Assert.True(entry.Definition.Length >= 70);
                Assert.EndsWith(".", entry.Definition);
                Assert.NotEmpty(entry.RelatedTerms);
            });
    }

    [Theory]
    [InlineData("Acceleration")]
    [InlineData("Degree of freedom")]
    [InlineData("Forward kinematics")]
    [InlineData("G-code")]
    [InlineData("Motion profile")]
    [InlineData("Odometry")]
    [InlineData("Robot state")]
    [InlineData("Tool center point")]
    [InlineData("Trajectory")]
    [InlineData("Workspace")]
    public void All_ShouldContainCoreTeachingTerms(string term)
    {
        Assert.Contains(
            GlossaryCatalog.All,
            entry => string.Equals(entry.Term, term, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("TCP", "Tool center point")]
    [InlineData("DOF", "Degree of freedom")]
    [InlineData("DSL", "Domain-specific language")]
    public void Search_WhenQueryIsAnAcronym_ShouldFindExpandedTerm(
        string acronym,
        string expectedTerm)
    {
        var result = GlossaryCatalog.Search(acronym);

        Assert.Contains(result, entry => entry.Term == expectedTerm);
    }

    [Fact]
    public void Search_WhenQueryAppearsInDefinition_ShouldFindTerm()
    {
        var result = GlossaryCatalog.Search("wheel rotation or travel");

        Assert.Contains(result, entry => entry.Term == "Odometry");
    }

    [Fact]
    public void Search_WhenCategoryIsSelected_ShouldReturnOnlyThatCategory()
    {
        var result = GlossaryCatalog.Search(query: null, GlossaryCategory.Safety);

        Assert.NotEmpty(result);
        Assert.All(result, entry => Assert.Equal(GlossaryCategory.Safety, entry.Category));
    }

    [Fact]
    public void Search_WhenQueryAndCategoryDoNotIntersect_ShouldReturnEmpty()
    {
        var result = GlossaryCatalog.Search("G-code", GlossaryCategory.Kinematics);

        Assert.Empty(result);
    }
}
