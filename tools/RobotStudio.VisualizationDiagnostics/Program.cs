using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using HelixToolkit.SharpDX;
using HelixToolkit.SharpDX.Model.Scene;
using RobotStudio.Desktop.Showcases;
using RobotStudio.Desktop.Showcases.Assets;

namespace RobotStudio.VisualizationDiagnostics;

internal static class Program
{
    private const int RenderFrameCount = 60;
    private const int HitTestColumns = 9;
    private const int HitTestRows = 7;

    [STAThread]
    public static int Main(string[] args)
    {
        var outputPath = Path.GetFullPath(args.FirstOrDefault() ??
            Path.Combine("artifacts", "visualization-performance.md"));
        var hardwareLabel = args.Skip(1).FirstOrDefault() ?? "Unspecified Windows hardware";
        var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        LoadResources(application);
        Exception? failure = null;
        var exitCode = 0;

        application.Dispatcher.InvokeAsync(async () =>
        {
            try
            {
                var result = await MeasureAsync(hardwareLabel);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                await File.WriteAllTextAsync(outputPath, result.Report, Encoding.UTF8);
                Console.WriteLine($"Visualization performance report written to {outputPath}");
                exitCode = result.Passed ? 0 : 2;
                Console.WriteLine(result.Passed
                    ? "Visualization performance budget passed."
                    : "Visualization performance budget failed. Review the report for details.");
            }
            catch (Exception exception)
            {
                failure = exception;
                Console.Error.WriteLine(exception);
            }
            finally
            {
                application.Shutdown(failure is null ? exitCode : 1);
            }
        }, DispatcherPriority.ApplicationIdle);

        application.Run();
        return failure is null ? exitCode : 1;
    }

    private static async Task<PerformanceRunResult> MeasureAsync(string hardwareLabel)
    {
        var warmupMilliseconds = WarmUpImportPipeline();
        var measurements = new List<ModelMeasurement>();
        foreach (var modelId in MechanicalShowcaseCatalog.ModelIds)
        {
            Console.WriteLine($"Profiling {modelId}...");
            var presentation = MechanicalShowcaseCatalog.Create(modelId);
            var manifestPath = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Robots",
                presentation.AssetDirectoryName,
                "robot.json");

            var loader = new RobotVisualAssetPackageLoader();
            var loadClock = Stopwatch.StartNew();
            var package = loader.Load(manifestPath, presentation.Showcase.Model);
            loadClock.Stop();

            var importClock = Stopwatch.StartNew();
            using var importedScene = new HelixRobotVisualAssetImporter().Import(package);
            importClock.Stop();

            var preparationClock = Stopwatch.StartNew();
            TreeTraverser.ForceUpdateTransformsAndBounds(importedScene.Root);
            preparationClock.Stop();
            var nodeCount = importedScene.NodesByPart.Values
                .SelectMany(nodes => nodes)
                .Distinct()
                .Count();

            var view = new MechanicalShowcaseView(presentation);
            var window = new Window
            {
                Title = $"RobotStudio visualization diagnostics: {presentation.Title}",
                Width = 1280,
                Height = 800,
                Content = view,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            MechanicalViewportPerformanceProbe viewportProbe;
            try
            {
                window.Show();
                await WaitForLayoutAsync(window);
                viewportProbe = await view.MeasurePerformanceAsync(
                    RenderFrameCount,
                    HitTestColumns,
                    HitTestRows);
            }
            finally
            {
                window.Close();
                view.Dispose();
            }

            measurements.Add(new ModelMeasurement(
                presentation.Title,
                new FileInfo(package.AssetPath).Length,
                presentation.Showcase.Model.Parts.Count,
                nodeCount,
                loadClock.Elapsed.TotalMilliseconds,
                importClock.Elapsed.TotalMilliseconds,
                preparationClock.Elapsed.TotalMilliseconds,
                Statistics.From(viewportProbe.FrameIntervalsMilliseconds),
                Statistics.From(viewportProbe.TransformUpdateMilliseconds),
                Statistics.From(viewportProbe.HitTestMilliseconds),
                viewportProbe.SemanticHitCount,
                HitTestColumns * HitTestRows,
                view.IsDisposed));
        }

        var budget = MechanicalViewportPerformanceBudget.TeachingHardwareMinimum;
        var failures = Assess(measurements, warmupMilliseconds, budget);
        return new PerformanceRunResult(
            BuildReport(measurements, warmupMilliseconds, hardwareLabel, budget, failures),
            failures.Count == 0);
    }

    private static double WarmUpImportPipeline()
    {
        var presentation = MechanicalShowcaseCatalog.Create(MechanicalShowcaseCatalog.ModelIds[0]);
        var manifestPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Robots",
            presentation.AssetDirectoryName,
            "robot.json");
        var clock = Stopwatch.StartNew();
        var package = new RobotVisualAssetPackageLoader().Load(manifestPath, presentation.Showcase.Model);
        using var scene = new HelixRobotVisualAssetImporter().Import(package);
        TreeTraverser.ForceUpdateTransformsAndBounds(scene.Root);
        clock.Stop();
        return clock.Elapsed.TotalMilliseconds;
    }

    private static async Task WaitForLayoutAsync(Window window)
    {
        await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);
        await Task.Delay(350);
    }

    private static string BuildReport(
        IReadOnlyList<ModelMeasurement> measurements,
        double warmupMilliseconds,
        string hardwareLabel,
        MechanicalViewportPerformanceBudget budget,
        IReadOnlyList<BudgetFailure> failures)
    {
        var culture = CultureInfo.InvariantCulture;
        var builder = new StringBuilder();
        builder.AppendLine("# Visualization Performance Baseline");
        builder.AppendLine();
        builder.AppendLine($"Measured: {DateTimeOffset.Now:yyyy-MM-dd HH:mm zzz}");
        builder.AppendLine();
        builder.AppendLine("## Environment");
        builder.AppendLine();
        builder.AppendLine($"- Hardware role: {hardwareLabel}");
        builder.AppendLine($"- OS: {RuntimeInformation.OSDescription}");
        builder.AppendLine($"- Runtime: {RuntimeInformation.FrameworkDescription}");
        builder.AppendLine($"- Architecture: {RuntimeInformation.ProcessArchitecture}");
        builder.AppendLine($"- CPU: {Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "unknown"}");
        builder.AppendLine($"- Logical processors: {Environment.ProcessorCount}");
        builder.AppendLine($"- Available managed memory: {GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1_073_741_824d:0.0} GiB");
        builder.AppendLine($"- Viewport: 1280 x 800, {RenderFrameCount} presentation frames, {HitTestColumns} x {HitTestRows} hit-test grid");
        builder.AppendLine(string.Create(culture,
            $"- One-time import pipeline warm-up: {warmupMilliseconds:0.00} ms"));
        builder.AppendLine();
        builder.AppendLine("## Acceptance Budget");
        builder.AppendLine();
        builder.AppendLine("The minimum profile targets responsive use on teaching hardware. Every robot must remain within every limit.");
        builder.AppendLine();
        builder.AppendLine("| Measurement | Maximum |");
        builder.AppendLine("| --- | ---: |");
        builder.AppendLine(string.Create(culture, $"| One-time pipeline warm-up | {budget.MaximumWarmupMilliseconds:0.00} ms |"));
        builder.AppendLine(string.Create(culture, $"| Manifest load | {budget.MaximumManifestLoadMilliseconds:0.00} ms |"));
        builder.AppendLine(string.Create(culture, $"| Warmed GLB import | {budget.MaximumImportMilliseconds:0.00} ms |"));
        builder.AppendLine(string.Create(culture, $"| Scene preparation | {budget.MaximumScenePreparationMilliseconds:0.00} ms |"));
        builder.AppendLine(string.Create(culture, $"| Frame interval p95 | {budget.MaximumFrameP95Milliseconds:0.00} ms (30 FPS target + 5% scheduler tolerance) |"));
        builder.AppendLine(string.Create(culture, $"| Transform update p95 | {budget.MaximumTransformP95Milliseconds:0.00} ms |"));
        builder.AppendLine(string.Create(culture, $"| Semantic hit test p95 | {budget.MaximumHitTestP95Milliseconds:0.00} ms |"));
        builder.AppendLine();
        builder.AppendLine("## Measurements");
        builder.AppendLine();
        builder.AppendLine("| Robot | Status | GLB KiB | Parts | Nodes | Manifest ms | Import ms | Scene prep ms | Frame avg / p95 ms | Transform avg / p95 ms | Hit test avg / p95 ms | Semantic hits | Teardown |");
        builder.AppendLine("| --- | :---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | :---: |");
        foreach (var item in measurements)
        {
            var status = failures.Any(failure => failure.ModelName == item.Name) ? "FAIL" : "PASS";
            builder.AppendLine(string.Create(culture,
                $"| {item.Name} | {status} | {item.AssetBytes / 1024d:0.0} | {item.PartCount} | {item.NodeCount} | {item.ManifestMilliseconds:0.00} | {item.ImportMilliseconds:0.00} | {item.ScenePreparationMilliseconds:0.00} | {item.Frame.Average:0.00} / {item.Frame.P95:0.00} | {item.Transform.Average:0.000} / {item.Transform.P95:0.000} | {item.HitTest.Average:0.000} / {item.HitTest.P95:0.000} | {item.SemanticHitCount}/{item.HitTestCount} | {(item.TeardownCompleted ? "PASS" : "FAIL")} |"));
        }

        builder.AppendLine();
        builder.AppendLine("## Result");
        builder.AppendLine();
        builder.AppendLine(failures.Count == 0 ? "**PASS**" : "**FAIL**");
        foreach (var failure in failures)
        {
            builder.AppendLine($"- {failure.ModelName}: {failure.Reason}");
        }

        builder.AppendLine();
        builder.AppendLine("## Interpretation");
        builder.AppendLine();
        builder.AppendLine("- The one-time warm-up records native Assimp and runtime initialization separately. Per-model timings use a fresh loader after that warm-up so they remain comparable.");
        builder.AppendLine("- GLB import includes Assimp parsing and semantic node binding.");
        builder.AppendLine("- Scene preparation forces transform and bounds propagation before the scene is attached to the measured viewport.");
        builder.AppendLine("- Frame cadence is observed from WPF composition while each frame samples a demonstration and applies procedural and imported transforms.");
        builder.AppendLine("- Hit-test timing uses HelixToolkit `FindHits` across the live viewport and counts points that resolve to a RobotStudio semantic part.");
        builder.AppendLine("- Exit code `0` means every measurement passed; a nonzero exit code means at least one performance budget was exceeded or the diagnostic could not complete.");
        builder.AppendLine("- Passing on the development workstation does not qualify the intended teaching hardware. Repeat this command there before release.");
        builder.AppendLine();
        builder.AppendLine("Run again with:");
        builder.AppendLine();
        builder.AppendLine("```powershell");
        builder.AppendLine($"dotnet run --project tools/RobotStudio.VisualizationDiagnostics --configuration Release -- docs/performance-baseline.md \"{hardwareLabel}\"");
        builder.AppendLine("```");
        return builder.ToString();
    }

    private static IReadOnlyList<BudgetFailure> Assess(
        IReadOnlyList<ModelMeasurement> measurements,
        double warmupMilliseconds,
        MechanicalViewportPerformanceBudget budget)
    {
        var failures = budget.EvaluateWarmup(warmupMilliseconds)
            .Select(reason => new BudgetFailure("Import pipeline", reason))
            .ToList();

        foreach (var measurement in measurements)
        {
            var observation = new MechanicalViewportPerformanceObservation(
                measurement.ManifestMilliseconds,
                measurement.ImportMilliseconds,
                measurement.ScenePreparationMilliseconds,
                measurement.Frame.P95,
                measurement.Transform.P95,
                measurement.HitTest.P95);
            failures.AddRange(budget.Evaluate(observation)
                .Select(reason => new BudgetFailure(measurement.Name, reason)));
            if (!measurement.TeardownCompleted)
            {
                failures.Add(new BudgetFailure(
                    measurement.Name,
                    "The viewport did not release its imported scene and rendering resources."));
            }
        }

        return failures;
    }

    private static void LoadResources(Application application)
    {
        foreach (var path in new[]
        {
            "/RobotStudio.Desktop;component/Localization/Strings.en.xaml",
            "/RobotStudio.Desktop;component/Styles/ControlStyles.xaml",
            "/RobotStudio.Desktop;component/Styles/MainWindowStyles.xaml"
        })
        {
            application.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(path, UriKind.Relative)
            });
        }
    }

    private sealed record ModelMeasurement(
        string Name,
        long AssetBytes,
        int PartCount,
        int NodeCount,
        double ManifestMilliseconds,
        double ImportMilliseconds,
        double ScenePreparationMilliseconds,
        Statistics Frame,
        Statistics Transform,
        Statistics HitTest,
        int SemanticHitCount,
        int HitTestCount,
        bool TeardownCompleted);

    private sealed record BudgetFailure(string ModelName, string Reason);

    private sealed record PerformanceRunResult(string Report, bool Passed);

    private sealed record Statistics(double Average, double P95)
    {
        public static Statistics From(IReadOnlyList<double> values)
        {
            if (values.Count == 0)
            {
                return new Statistics(0, 0);
            }

            var ordered = values.Order().ToArray();
            var percentileIndex = Math.Clamp(
                (int)Math.Ceiling(ordered.Length * 0.95) - 1,
                0,
                ordered.Length - 1);
            return new Statistics(values.Average(), ordered[percentileIndex]);
        }
    }
}
