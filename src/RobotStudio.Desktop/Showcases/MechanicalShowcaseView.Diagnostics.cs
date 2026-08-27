using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using HelixToolkit.SharpDX.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

public partial class MechanicalShowcaseView
{
    internal async Task<MechanicalViewportPerformanceProbe> MeasurePerformanceAsync(
        int frameCount,
        int hitTestColumns,
        int hitTestRows)
    {
        if (frameCount < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(frameCount));
        }

        if (hitTestColumns <= 0 || hitTestRows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hitTestColumns));
        }

        if (importedScene is null || SelectedDemonstration is null ||
            ShowcaseViewport.ActualWidth <= 0 || ShowcaseViewport.ActualHeight <= 0)
        {
            throw new InvalidOperationException("The mechanical viewport must be loaded with an imported scene before profiling.");
        }

        var frameIntervals = new List<double>(frameCount);
        var transformUpdates = new List<double>(frameCount);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var clock = Stopwatch.StartNew();
        var previousTimestamp = clock.Elapsed;
        var sampledFrame = 0;

        void OnRendering(object? sender, EventArgs args)
        {
            var now = clock.Elapsed;
            if (sampledFrame > 0)
            {
                frameIntervals.Add((now - previousTimestamp).TotalMilliseconds);
            }

            previousTimestamp = now;
            var demonstration = SelectedDemonstration!;
            var sampleTime = TimeSpan.FromTicks(
                demonstration.Duration.Ticks * sampledFrame / Math.Max(frameCount - 1, 1));
            var updateClock = Stopwatch.StartNew();
            ApplyDemonstrationTime(sampleTime);
            updateClock.Stop();
            transformUpdates.Add(updateClock.Elapsed.TotalMilliseconds);
            sampledFrame++;
            if (sampledFrame >= frameCount)
            {
                CompositionTarget.Rendering -= OnRendering;
                completion.TrySetResult();
            }
        }

        CompositionTarget.Rendering += OnRendering;
        try
        {
            await completion.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        var hitTests = new List<double>(hitTestColumns * hitTestRows);
        var semanticHitCount = 0;
        for (var row = 0; row < hitTestRows; row++)
        {
            for (var column = 0; column < hitTestColumns; column++)
            {
                var point = new Point(
                    ShowcaseViewport.ActualWidth * (column + 0.5) / hitTestColumns,
                    ShowcaseViewport.ActualHeight * (row + 0.5) / hitTestRows);
                var hitClock = Stopwatch.StartNew();
                var hits = ShowcaseViewport.FindHits(point);
                hitClock.Stop();
                hitTests.Add(hitClock.Elapsed.TotalMilliseconds);
                if (hits.Any(hit => hit.ModelHit is SceneNode { Tag: RobotPartId }))
                {
                    semanticHitCount++;
                }
            }
        }

        return new MechanicalViewportPerformanceProbe(
            frameIntervals.AsReadOnly(),
            transformUpdates.AsReadOnly(),
            hitTests.AsReadOnly(),
            semanticHitCount);
    }
}
