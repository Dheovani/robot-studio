namespace RobotStudio.Desktop.Showcases;

internal sealed record MechanicalViewportPerformanceBudget(
    double MaximumWarmupMilliseconds,
    double MaximumManifestLoadMilliseconds,
    double MaximumImportMilliseconds,
    double MaximumScenePreparationMilliseconds,
    double MaximumFrameP95Milliseconds,
    double MaximumTransformP95Milliseconds,
    double MaximumHitTestP95Milliseconds)
{
    public static MechanicalViewportPerformanceBudget TeachingHardwareMinimum { get; } = new(
        MaximumWarmupMilliseconds: 2_000,
        MaximumManifestLoadMilliseconds: 100,
        MaximumImportMilliseconds: 500,
        MaximumScenePreparationMilliseconds: 50,
        // WPF composition intervals are scheduler-quantized, so the 30 FPS target includes 5% timing tolerance.
        MaximumFrameP95Milliseconds: (1_000d / 30) * 1.05,
        MaximumTransformP95Milliseconds: 8,
        MaximumHitTestP95Milliseconds: 8);

    public IReadOnlyList<string> EvaluateWarmup(double warmupMilliseconds)
    {
        return warmupMilliseconds <= MaximumWarmupMilliseconds
            ? []
            : [$"Import pipeline warm-up took {warmupMilliseconds:0.00} ms; budget is {MaximumWarmupMilliseconds:0.00} ms."];
    }

    public IReadOnlyList<string> Evaluate(MechanicalViewportPerformanceObservation observation)
    {
        var failures = new List<string>();
        AddFailureWhenExceeded(failures, "manifest load", observation.ManifestLoadMilliseconds, MaximumManifestLoadMilliseconds);
        AddFailureWhenExceeded(failures, "GLB import", observation.ImportMilliseconds, MaximumImportMilliseconds);
        AddFailureWhenExceeded(failures, "scene preparation", observation.ScenePreparationMilliseconds, MaximumScenePreparationMilliseconds);
        AddFailureWhenExceeded(failures, "frame p95", observation.FrameP95Milliseconds, MaximumFrameP95Milliseconds);
        AddFailureWhenExceeded(failures, "transform p95", observation.TransformP95Milliseconds, MaximumTransformP95Milliseconds);
        AddFailureWhenExceeded(failures, "hit-test p95", observation.HitTestP95Milliseconds, MaximumHitTestP95Milliseconds);
        return failures;
    }

    private static void AddFailureWhenExceeded(
        ICollection<string> failures,
        string measurement,
        double actualMilliseconds,
        double maximumMilliseconds)
    {
        if (actualMilliseconds > maximumMilliseconds)
        {
            failures.Add(
                $"{measurement} took {actualMilliseconds:0.00} ms; budget is {maximumMilliseconds:0.00} ms.");
        }
    }
}

internal sealed record MechanicalViewportPerformanceObservation(
    double ManifestLoadMilliseconds,
    double ImportMilliseconds,
    double ScenePreparationMilliseconds,
    double FrameP95Milliseconds,
    double TransformP95Milliseconds,
    double HitTestP95Milliseconds);
