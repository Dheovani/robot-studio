using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Microsoft.Win32;
using RobotStudio.Desktop.Examples;
using RobotStudio.Desktop.Profiles;
using RobotStudio.Desktop.Rendering;
using RobotStudio.Desktop.Robots;
using RobotStudio.Desktop.Scripting;
using RobotStudio.Domain;
using RobotStudio.Domain.Aerial;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Mobile;
using RobotStudio.Domain.Parallel;
using RobotStudio.Scripting;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop;

public partial class MainWindow
{
    private void UpdatePositionChart()
    {
        PositionChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.Poses.Count == 0)
        {
            return;
        }

        var width = PositionChartCanvas.ActualWidth;
        var height = PositionChartCanvas.ActualHeight;
        if (width <= ChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        DrawPositionChartGrid(width, height);
        DrawPositionSeries(
            pose => pose.ToolCenterPoint.XMillimeters,
            profile.XAxis.MinimumMillimeters,
            profile.XAxis.MaximumMillimeters,
            Color.FromRgb(248, 113, 113),
            width,
            height);
        DrawPositionSeries(
            pose => pose.ToolCenterPoint.YMillimeters,
            profile.YAxis.MinimumMillimeters,
            profile.YAxis.MaximumMillimeters,
            Color.FromRgb(34, 197, 94),
            width,
            height);
        DrawPositionSeries(
            pose => pose.ToolCenterPoint.ZMillimeters,
            profile.ZAxis.MinimumMillimeters,
            profile.ZAxis.MaximumMillimeters,
            Color.FromRgb(96, 165, 250),
            width,
            height);
        DrawPositionChartCursor(width, height);
    }

    private void DrawPositionChartGrid(
        double width,
        double height)
    {
        var plotLeft = ChartPaddingLeft;
        var plotTop = ChartPaddingTop;
        var plotRight = width - ChartPaddingRight;
        var plotBottom = height - ChartPaddingBottom;
        var gridBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85));

        for (var index = 0; index <= 3; index++)
        {
            var y = plotTop + ((plotBottom - plotTop) * index / 3);
            PositionChartCanvas.Children.Add(new Line
            {
                X1 = plotLeft,
                X2 = plotRight,
                Y1 = y,
                Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1
            });
        }

        var positionLabel = new TextBlock
        {
            Text = "pos",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        PositionChartCanvas.Children.Add(positionLabel);
        Canvas.SetLeft(positionLabel, 4);
        Canvas.SetTop(positionLabel, 2);

        var timeLabel = new TextBlock
        {
            Text = "time",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        PositionChartCanvas.Children.Add(timeLabel);
        Canvas.SetLeft(timeLabel, plotRight - 28);
        Canvas.SetTop(timeLabel, plotBottom + 4);
    }

    private void DrawPositionSeries(
        Func<CartesianRobotPose, double> selectValue,
        double minimum,
        double maximum,
        Color color,
        double width,
        double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var points = new PointCollection();
        foreach (var pose in snapshot.Poses)
        {
            points.Add(ToChartPoint(
                pose.Time,
                selectValue(pose),
                minimum,
                maximum,
                width,
                height));
        }

        PositionChartCanvas.Children.Add(new Polyline
        {
            Points = points,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2
        });
    }

    private void DrawPositionChartCursor(
        double width,
        double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var sceneFrame = snapshot.SceneFrames[currentFrameIndex];
        var cursorX = ToChartX(sceneFrame.Time, width);
        PositionChartCanvas.Children.Add(new Line
        {
            X1 = cursorX,
            X2 = cursorX,
            Y1 = ChartPaddingTop,
            Y2 = height - ChartPaddingBottom,
            Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            StrokeThickness = 1.5
        });
    }

    private Point ToChartPoint(
        TimeSpan time,
        double value,
        double minimum,
        double maximum,
        double width,
        double height)
    {
        var normalizedValue = maximum <= minimum
            ? 0
            : Math.Clamp((value - minimum) / (maximum - minimum), 0, 1);

        return new Point(
            ToChartX(time, width),
            ChartPaddingTop + ((1 - normalizedValue) * (height - ChartPaddingTop - ChartPaddingBottom)));
    }

    private double ToChartX(
        TimeSpan time,
        double width)
    {
        if (snapshot is null || snapshot.TotalDuration <= TimeSpan.Zero)
        {
            return ChartPaddingLeft;
        }

        var normalizedTime = Math.Clamp(
            time.TotalSeconds / snapshot.TotalDuration.TotalSeconds,
            0,
            1);
        return ChartPaddingLeft + (normalizedTime * (width - ChartPaddingLeft - ChartPaddingRight));
    }

    private void UpdateVelocityChart()
    {
        VelocityChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.Poses.Count == 0)
        {
            return;
        }

        var width = VelocityChartCanvas.ActualWidth;
        var height = VelocityChartCanvas.ActualHeight;
        if (width <= ChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        var samples = CreateVelocitySamples(snapshot);
        var maximumVelocity = Math.Max(1, samples.Count == 0 ? 0 : samples.Max(sample => sample.VelocityMillimetersPerSecond));

        DrawVelocityChartGrid(width, height, maximumVelocity);
        if (samples.Count > 0)
        {
            DrawVelocitySeries(samples, maximumVelocity, width, height);
        }

        DrawVelocityChartCursor(width, height);
    }

    private static IReadOnlyList<VelocitySample> CreateVelocitySamples(CartesianPlaybackSnapshot snapshot)
        => snapshot.Frames
            .Select(frame => new VelocitySample(
                frame.Time,
                frame.VelocityMillimetersPerSecond))
            .ToArray();

    private void DrawVelocityChartGrid(
        double width,
        double height,
        double maximumVelocity)
    {
        var plotLeft = ChartPaddingLeft;
        var plotTop = ChartPaddingTop;
        var plotRight = width - ChartPaddingRight;
        var plotBottom = height - ChartPaddingBottom;
        var gridBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85));

        for (var index = 0; index <= 2; index++)
        {
            var y = plotTop + ((plotBottom - plotTop) * index / 2);
            VelocityChartCanvas.Children.Add(new Line
            {
                X1 = plotLeft,
                X2 = plotRight,
                Y1 = y,
                Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1
            });
        }

        var velocityLabel = new TextBlock
        {
            Text = $"{maximumVelocity:0.#}",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        VelocityChartCanvas.Children.Add(velocityLabel);
        Canvas.SetLeft(velocityLabel, 4);
        Canvas.SetTop(velocityLabel, 2);

        var zeroLabel = new TextBlock
        {
            Text = "0",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        VelocityChartCanvas.Children.Add(zeroLabel);
        Canvas.SetLeft(zeroLabel, 12);
        Canvas.SetTop(zeroLabel, plotBottom - 10);

        var timeLabel = new TextBlock
        {
            Text = "time",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        VelocityChartCanvas.Children.Add(timeLabel);
        Canvas.SetLeft(timeLabel, plotRight - 28);
        Canvas.SetTop(timeLabel, plotBottom + 4);
    }

    private void DrawVelocitySeries(
        IReadOnlyList<VelocitySample> samples,
        double maximumVelocity,
        double width,
        double height)
    {
        var points = new PointCollection();
        foreach (var sample in samples)
        {
            points.Add(ToVelocityChartPoint(sample, maximumVelocity, width, height));
        }

        VelocityChartCanvas.Children.Add(new Polyline
        {
            Points = points,
            Stroke = new SolidColorBrush(Color.FromRgb(250, 204, 21)),
            StrokeThickness = 2
        });
    }

    private void DrawVelocityChartCursor(
        double width,
        double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var sceneFrame = snapshot.SceneFrames[currentFrameIndex];
        var cursorX = ToChartX(sceneFrame.Time, width);
        VelocityChartCanvas.Children.Add(new Line
        {
            X1 = cursorX,
            X2 = cursorX,
            Y1 = ChartPaddingTop,
            Y2 = height - ChartPaddingBottom,
            Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            StrokeThickness = 1.5
        });
    }

    private Point ToVelocityChartPoint(
        VelocitySample sample,
        double maximumVelocity,
        double width,
        double height)
    {
        var normalizedVelocity = Math.Clamp(
            sample.VelocityMillimetersPerSecond / maximumVelocity,
            0,
            1);

        return new Point(
            ToChartX(sample.Time, width),
            ChartPaddingTop + ((1 - normalizedVelocity) * (height - ChartPaddingTop - ChartPaddingBottom)));
    }

    private void UpdateVelocityComparisonChart()
    {
        VelocityComparisonChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.SceneFrames.Count == 0)
        {
            return;
        }

        var width = VelocityComparisonChartCanvas.ActualWidth;
        var height = VelocityComparisonChartCanvas.ActualHeight;
        if (width <= ChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        var effectiveSamples = CreateVelocitySamples(snapshot)
            .Select(sample => new ScalarSample(sample.Time, sample.VelocityMillimetersPerSecond))
            .ToArray();
        var requestedSamples = CreateRequestedVelocitySamples(snapshot);
        var maximumVelocity = Math.Max(
            1,
            Math.Max(
                effectiveSamples.Length == 0 ? 0 : effectiveSamples.Max(sample => sample.Value),
                requestedSamples.Count == 0 ? 0 : requestedSamples.Max(sample => sample.Value)));

        DrawScalarChartGrid(
            VelocityComparisonChartCanvas,
            width,
            height,
            maximumVelocity,
            "mm/s");
        DrawScalarSeries(
            VelocityComparisonChartCanvas,
            requestedSamples,
            maximumVelocity,
            Color.FromRgb(56, 189, 248),
            width,
            height);
        DrawScalarSeries(
            VelocityComparisonChartCanvas,
            effectiveSamples,
            maximumVelocity,
            Color.FromRgb(250, 204, 21),
            width,
            height);
        DrawScalarChartCursor(VelocityComparisonChartCanvas, width, height);
    }

    private void UpdateAccelerationChart()
    {
        AccelerationChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.Frames.Count == 0)
        {
            return;
        }

        var width = AccelerationChartCanvas.ActualWidth;
        var height = AccelerationChartCanvas.ActualHeight;
        if (width <= ChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        var samples = snapshot.Frames
            .Select(frame => new ScalarSample(
                frame.Time,
                frame.AccelerationMillimetersPerSecondSquared))
            .ToArray();
        var maximumMagnitude = Math.Max(
            1,
            samples.Max(sample => Math.Abs(sample.Value)));

        DrawSignedScalarChartGrid(
            AccelerationChartCanvas,
            width,
            height,
            maximumMagnitude,
            "mm/s^2");
        DrawSignedScalarSeries(
            AccelerationChartCanvas,
            samples,
            maximumMagnitude,
            width,
            height);
        DrawScalarChartCursor(AccelerationChartCanvas, width, height);
    }

    private static void DrawSignedScalarChartGrid(
        Canvas canvas,
        double width,
        double height,
        double maximumMagnitude,
        string unit)
    {
        var plotLeft = ChartPaddingLeft;
        var plotTop = ChartPaddingTop;
        var plotRight = width - ChartPaddingRight;
        var plotBottom = height - ChartPaddingBottom;
        var plotCenter = (plotTop + plotBottom) / 2;
        var gridBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85));

        foreach (var y in new[] { plotTop, plotCenter, plotBottom })
        {
            canvas.Children.Add(new Line
            {
                X1 = plotLeft,
                X2 = plotRight,
                Y1 = y,
                Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1
            });
        }

        AddChartLabel(canvas, $"+{maximumMagnitude:0.#} {unit}", left: 4, top: 2);
        AddChartLabel(canvas, "0", left: 12, top: plotCenter - 8);
        AddChartLabel(canvas, $"-{maximumMagnitude:0.#}", left: 4, top: plotBottom - 12);
        AddChartLabel(canvas, "time", left: plotRight - 28, top: plotBottom + 4);
    }

    private static void AddChartLabel(
        Canvas canvas,
        string text,
        double left,
        double top)
    {
        var label = new TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };

        canvas.Children.Add(label);
        Canvas.SetLeft(label, left);
        Canvas.SetTop(label, top);
    }

    private void DrawSignedScalarSeries(
        Canvas canvas,
        IReadOnlyList<ScalarSample> samples,
        double maximumMagnitude,
        double width,
        double height)
    {
        for (var index = 1; index < samples.Count; index++)
        {
            var previous = samples[index - 1];
            var current = samples[index];
            var color = current.Value < 0
                ? Color.FromRgb(248, 113, 113)
                : Color.FromRgb(34, 197, 94);

            canvas.Children.Add(new Line
            {
                X1 = ToChartX(previous.Time, width),
                Y1 = ToSignedScalarChartY(previous.Value, maximumMagnitude, height),
                X2 = ToChartX(current.Time, width),
                Y2 = ToSignedScalarChartY(current.Value, maximumMagnitude, height),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 2
            });
        }
    }

    private static double ToSignedScalarChartY(
        double value,
        double maximumMagnitude,
        double height)
    {
        var normalizedValue = Math.Clamp(value / maximumMagnitude, -1, 1);
        var plotTop = ChartPaddingTop;
        var plotBottom = height - ChartPaddingBottom;
        var plotCenter = (plotTop + plotBottom) / 2;
        var halfHeight = (plotBottom - plotTop) / 2;

        return plotCenter - (normalizedValue * halfHeight);
    }

    private IReadOnlyList<ScalarSample> CreateRequestedVelocitySamples(CartesianPlaybackSnapshot snapshot)
    {
        var samples = new List<ScalarSample>();
        foreach (var frame in snapshot.SceneFrames)
        {
            var requestedVelocity = frame.State == RobotState.Moving
                ? frame.RequestedVelocityMillimetersPerSecond ?? 0
                : 0;

            samples.Add(new ScalarSample(frame.Time, requestedVelocity));
        }

        return samples;
    }

    private void UpdateDistanceChart()
    {
        DistanceChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.Poses.Count == 0)
        {
            return;
        }

        var width = DistanceChartCanvas.ActualWidth;
        var height = DistanceChartCanvas.ActualHeight;
        if (width <= ChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        var samples = CreateDistanceSamples(snapshot);
        var maximumDistance = Math.Max(1, samples.Count == 0 ? 0 : samples.Max(sample => sample.Value));

        DrawScalarChartGrid(
            DistanceChartCanvas,
            width,
            height,
            maximumDistance,
            "mm");
        DrawScalarSeries(
            DistanceChartCanvas,
            samples,
            maximumDistance,
            Color.FromRgb(45, 212, 191),
            width,
            height);
        DrawScalarChartCursor(DistanceChartCanvas, width, height);
    }

    private static IReadOnlyList<ScalarSample> CreateDistanceSamples(CartesianPlaybackSnapshot snapshot)
    {
        var samples = new List<ScalarSample>
        {
            new(snapshot.Poses[0].Time, 0)
        };
        var totalDistance = 0d;

        for (var index = 1; index < snapshot.Poses.Count; index++)
        {
            var previous = snapshot.Poses[index - 1];
            var current = snapshot.Poses[index];
            totalDistance += CalculateDistance(previous.ToolCenterPoint, current.ToolCenterPoint);
            samples.Add(new ScalarSample(current.Time, totalDistance));
        }

        return samples;
    }

    private void DrawScalarChartGrid(
        Canvas canvas,
        double width,
        double height,
        double maximumValue,
        string unit)
    {
        var plotLeft = ChartPaddingLeft;
        var plotTop = ChartPaddingTop;
        var plotRight = width - ChartPaddingRight;
        var plotBottom = height - ChartPaddingBottom;
        var gridBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85));

        for (var index = 0; index <= 2; index++)
        {
            var y = plotTop + ((plotBottom - plotTop) * index / 2);
            canvas.Children.Add(new Line
            {
                X1 = plotLeft,
                X2 = plotRight,
                Y1 = y,
                Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1
            });
        }

        var maximumLabel = new TextBlock
        {
            Text = $"{maximumValue:0.#} {unit}",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        canvas.Children.Add(maximumLabel);
        Canvas.SetLeft(maximumLabel, 4);
        Canvas.SetTop(maximumLabel, 2);

        var zeroLabel = new TextBlock
        {
            Text = "0",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        canvas.Children.Add(zeroLabel);
        Canvas.SetLeft(zeroLabel, 12);
        Canvas.SetTop(zeroLabel, plotBottom - 10);

        var timeLabel = new TextBlock
        {
            Text = "time",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11
        };
        canvas.Children.Add(timeLabel);
        Canvas.SetLeft(timeLabel, plotRight - 28);
        Canvas.SetTop(timeLabel, plotBottom + 4);
    }

    private void DrawScalarSeries(
        Canvas canvas,
        IReadOnlyList<ScalarSample> samples,
        double maximumValue,
        Color color,
        double width,
        double height)
    {
        if (samples.Count == 0)
        {
            return;
        }

        var points = new PointCollection();
        foreach (var sample in samples)
        {
            points.Add(ToScalarChartPoint(sample, maximumValue, width, height));
        }

        canvas.Children.Add(new Polyline
        {
            Points = points,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2
        });
    }

    private void DrawScalarChartCursor(
        Canvas canvas,
        double width,
        double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var sceneFrame = snapshot.SceneFrames[currentFrameIndex];
        var cursorX = ToChartX(sceneFrame.Time, width);
        canvas.Children.Add(new Line
        {
            X1 = cursorX,
            X2 = cursorX,
            Y1 = ChartPaddingTop,
            Y2 = height - ChartPaddingBottom,
            Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            StrokeThickness = 1.5
        });
    }

    private Point ToScalarChartPoint(
        ScalarSample sample,
        double maximumValue,
        double width,
        double height)
    {
        var normalizedValue = maximumValue <= 0
            ? 0
            : Math.Clamp(sample.Value / maximumValue, 0, 1);

        return new Point(
            ToChartX(sample.Time, width),
            ChartPaddingTop + ((1 - normalizedValue) * (height - ChartPaddingTop - ChartPaddingBottom)));
    }

    private void UpdateStateChart()
    {
        StateChartCanvas.Children.Clear();

        if (snapshot is null || snapshot.SceneFrames.Count == 0)
        {
            return;
        }

        var width = StateChartCanvas.ActualWidth;
        var height = StateChartCanvas.ActualHeight;
        if (width <= StateChartPaddingLeft + ChartPaddingRight ||
            height <= ChartPaddingTop + ChartPaddingBottom)
        {
            return;
        }

        DrawStateChartRows(width, height);
        DrawStateChartSegments(width, height);
        DrawStateChartCursor(width, height);
    }

    private void DrawStateChartRows(double width, double height)
    {
        var states = Enum.GetValues<RobotState>();
        var rowHeight = GetStateChartRowHeight(height, states.Length);
        var plotRight = width - ChartPaddingRight;
        var rowBrush = new SolidColorBrush(Color.FromRgb(30, 41, 59));
        var labelBrush = new SolidColorBrush(Color.FromRgb(148, 163, 184));

        for (var index = 0; index < states.Length; index++)
        {
            var y = ChartPaddingTop + (index * (rowHeight + StateChartRowGap));
            var rowBackground = new Rectangle
            {
                Width = plotRight - StateChartPaddingLeft,
                Height = rowHeight,
                Fill = rowBrush
            };
            StateChartCanvas.Children.Add(rowBackground);
            Canvas.SetLeft(rowBackground, StateChartPaddingLeft);
            Canvas.SetTop(rowBackground, y);

            var label = new TextBlock
            {
                Text = states[index].ToString(),
                Foreground = labelBrush,
                FontSize = 11
            };
            StateChartCanvas.Children.Add(label);
            Canvas.SetLeft(label, 6);
            Canvas.SetTop(label, y + Math.Max(0, (rowHeight - 14) / 2));
        }
    }

    private void DrawStateChartSegments(double width, double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var states = Enum.GetValues<RobotState>();
        var rowHeight = GetStateChartRowHeight(height, states.Length);
        var segments = CreateStateSegments(snapshot);

        foreach (var segment in segments)
        {
            var stateIndex = Array.IndexOf(states, segment.State);
            if (stateIndex < 0)
            {
                continue;
            }

            var x = ToStateChartX(segment.Start, width);
            var segmentWidth = Math.Max(
                2,
                ToStateChartX(segment.End, width) - x);
            var y = ChartPaddingTop + (stateIndex * (rowHeight + StateChartRowGap));

            var segmentRectangle = new Rectangle
            {
                Width = segmentWidth,
                Height = rowHeight,
                Fill = new SolidColorBrush(GetStateColor(segment.State))
            };
            StateChartCanvas.Children.Add(segmentRectangle);
            Canvas.SetLeft(segmentRectangle, x);
            Canvas.SetTop(segmentRectangle, y);
        }
    }

    private static IReadOnlyList<StateSegment> CreateStateSegments(CartesianPlaybackSnapshot snapshot)
    {
        var segments = new List<StateSegment>();
        var startTime = snapshot.SceneFrames[0].Time;
        var currentState = snapshot.SceneFrames[0].State;

        for (var index = 1; index < snapshot.SceneFrames.Count; index++)
        {
            var frame = snapshot.SceneFrames[index];
            if (frame.State == currentState)
            {
                continue;
            }

            segments.Add(new StateSegment(currentState, startTime, frame.Time));
            currentState = frame.State;
            startTime = frame.Time;
        }

        segments.Add(new StateSegment(currentState, startTime, snapshot.TotalDuration));
        return segments;
    }

    private void DrawStateChartCursor(double width, double height)
    {
        if (snapshot is null)
        {
            return;
        }

        var sceneFrame = snapshot.SceneFrames[currentFrameIndex];
        var cursorX = ToStateChartX(sceneFrame.Time, width);
        StateChartCanvas.Children.Add(new Line
        {
            X1 = cursorX,
            X2 = cursorX,
            Y1 = ChartPaddingTop,
            Y2 = height - ChartPaddingBottom,
            Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            StrokeThickness = 1.5
        });
    }

    private double ToStateChartX(
        TimeSpan time,
        double width)
    {
        if (snapshot is null || snapshot.TotalDuration <= TimeSpan.Zero)
        {
            return StateChartPaddingLeft;
        }

        var normalizedTime = Math.Clamp(
            time.TotalSeconds / snapshot.TotalDuration.TotalSeconds,
            0,
            1);
        return StateChartPaddingLeft + (normalizedTime * (width - StateChartPaddingLeft - ChartPaddingRight));
    }

    private static double GetStateChartRowHeight(
        double height,
        int rowCount) =>
        Math.Max(
            8,
            (height - ChartPaddingTop - ChartPaddingBottom - (StateChartRowGap * (rowCount - 1))) / rowCount);

    private static Color GetStateColor(RobotState state) => state switch
    {
        RobotState.Idle => Color.FromRgb(148, 163, 184),
        RobotState.Homing => Color.FromRgb(96, 165, 250),
        RobotState.Moving => Color.FromRgb(34, 197, 94),
        RobotState.Waiting => Color.FromRgb(250, 204, 21),
        RobotState.Completed => Color.FromRgb(45, 212, 191),
        RobotState.Faulted => Color.FromRgb(248, 113, 113),
        _ => Colors.White
    };

    private void UpdateMovementExplanation(CartesianSceneFrame sceneFrame)
    {
        if (snapshot is not null)
        {
            MovementExplanationText.Text = movementExplanationBuilder.Create(
                profile,
                snapshot,
                currentFrameIndex);
        }
    }

    private double GetSelectedPlaybackSpeed()
    {
        if (PlaybackSpeedComboBox.SelectedItem is ComboBoxItem { Tag: string tag } &&
            double.TryParse(tag, NumberStyles.Float, CultureInfo.InvariantCulture, out var speed) &&
            speed > 0)
        {
            return speed;
        }

        return 1;
    }
}
