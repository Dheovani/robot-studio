using RobotStudio.Desktop.Showcases;

namespace RobotStudio.Desktop.Tests;

public sealed class MechanicalViewportPerformanceBudgetTests
{
    private static readonly MechanicalViewportPerformanceBudget Budget =
        MechanicalViewportPerformanceBudget.TeachingHardwareMinimum;

    [Fact]
    public void Evaluate_WhenMeasurementsAreWithinBudget_ShouldReturnNoFailures()
    {
        var observation = new MechanicalViewportPerformanceObservation(
            ManifestLoadMilliseconds: 10,
            ImportMilliseconds: 100,
            ScenePreparationMilliseconds: 10,
            FrameP95Milliseconds: 30,
            TransformP95Milliseconds: 4,
            HitTestP95Milliseconds: 4);

        var failures = Budget.Evaluate(observation);

        Assert.Empty(failures);
    }

    [Fact]
    public void Evaluate_WhenMeasurementsExceedBudget_ShouldExplainEveryFailure()
    {
        var observation = new MechanicalViewportPerformanceObservation(
            ManifestLoadMilliseconds: 101,
            ImportMilliseconds: 501,
            ScenePreparationMilliseconds: 51,
            FrameP95Milliseconds: 36,
            TransformP95Milliseconds: 9,
            HitTestP95Milliseconds: 9);

        var failures = Budget.Evaluate(observation);

        Assert.Collection(
            failures,
            failure => Assert.Contains("manifest load", failure),
            failure => Assert.Contains("GLB import", failure),
            failure => Assert.Contains("scene preparation", failure),
            failure => Assert.Contains("frame p95", failure),
            failure => Assert.Contains("transform p95", failure),
            failure => Assert.Contains("hit-test p95", failure));
    }

    [Theory]
    [InlineData(2_000, true)]
    [InlineData(2_001, false)]
    public void EvaluateWarmup_WhenComparedWithBudget_ShouldReturnExpectedResult(
        double elapsedMilliseconds,
        bool shouldPass)
    {
        var failures = Budget.EvaluateWarmup(elapsedMilliseconds);

        Assert.Equal(shouldPass, failures.Count == 0);
    }
}
