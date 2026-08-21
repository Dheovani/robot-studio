namespace RobotStudio.Desktop.Didactics;

public sealed record GlossaryEntry(
    string Term,
    string? Acronym,
    GlossaryCategory Category,
    string Definition,
    IReadOnlyList<string> RelatedTerms)
{
    public string DisplayTerm => Acronym is null ? Term : $"{Term} ({Acronym})";

    public string RelatedTermsText => RelatedTerms.Count == 0
        ? string.Empty
        : $"Related: {string.Join(", ", RelatedTerms)}";
}
