namespace RobotStudio.Desktop.Showcases;

internal static class MechanicalShowcaseCatalog
{
    private static readonly IReadOnlyDictionary<string, Func<MechanicalShowcasePresentation>> Factories =
        new Dictionary<string, Func<MechanicalShowcasePresentation>>(StringComparer.Ordinal)
        {
            ["cartesian-intro-mechanical"] = CartesianMechanicalShowcaseDefinition.CreatePresentation,
            ["xy-plotter-mechanical"] = XYPlotterMechanicalShowcaseDefinition.CreatePresentation,
            ["differential-drive-mechanical"] = DifferentialDriveMechanicalShowcaseDefinition.CreatePresentation,
            ["scara-mechanical"] = ScaraMechanicalShowcaseDefinition.CreatePresentation
        };

    public static IReadOnlyList<string> ModelIds { get; } = Factories.Keys.ToArray();

    public static MechanicalShowcasePresentation Create(string modelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        return Factories.TryGetValue(modelId, out var factory)
            ? factory()
            : throw new KeyNotFoundException($"Mechanical showcase '{modelId}' is not registered.");
    }
}
