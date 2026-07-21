namespace RobotStudio.Simulation;

public sealed record PlaybackSnapshotValidationResult(IReadOnlyList<string> Errors)
{
    public bool IsValid => Errors.Count == 0;

    public static PlaybackSnapshotValidationResult Valid { get; } = new(Array.Empty<string>());
}
