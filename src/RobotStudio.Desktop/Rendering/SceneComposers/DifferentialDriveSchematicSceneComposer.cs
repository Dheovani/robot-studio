using System.Windows;
using System.Windows.Media;
using RobotStudio.Domain.Mobile;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop.Rendering.SceneComposers;

internal static class DifferentialDriveSchematicSceneComposer
{
    private const double Padding = 36;

    public static CanvasScene2D Compose(
        DifferentialDrivePlaybackSnapshot snapshot,
        int frameIndex,
        Size viewportSize,
        double zoomMultiplier)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (frameIndex < 0 || frameIndex >= snapshot.FrameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frameIndex));
        }

        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return new CanvasScene2D([]);
        }

        if (!double.IsFinite(zoomMultiplier) || zoomMultiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(zoomMultiplier));
        }

        var mapping = CreateMapping(snapshot.Profile, viewportSize, zoomMultiplier);
        var primitives = new List<CanvasPrimitive2D>();
        AddWorkspace(primitives, snapshot.Profile, mapping);
        AddPath(primitives, snapshot, frameIndex, mapping);
        AddRobot(primitives, snapshot.Frames[frameIndex].Pose, mapping);
        return new CanvasScene2D(primitives);
    }

    private static void AddWorkspace(
        ICollection<CanvasPrimitive2D> primitives,
        DifferentialDriveProfile profile,
        CoordinateMapping mapping)
    {
        var topLeft = mapping.Map(profile.MinimumXMillimeters, profile.MaximumYMillimeters);
        var bottomRight = mapping.Map(profile.MaximumXMillimeters, profile.MinimumYMillimeters);
        primitives.Add(new CanvasRectangle2D(
            new Rect(topLeft, bottomRight),
            Color.FromArgb(20, 59, 130, 246),
            Color.FromRgb(71, 85, 105),
            StrokeThickness: 2));

        for (var x = profile.MinimumXMillimeters; x <= profile.MaximumXMillimeters; x += 50)
        {
            primitives.Add(new CanvasLine2D(
                mapping.Map(x, profile.MinimumYMillimeters),
                mapping.Map(x, profile.MaximumYMillimeters),
                Color.FromRgb(30, 41, 59),
                Thickness: 1));
        }

        for (var y = profile.MinimumYMillimeters; y <= profile.MaximumYMillimeters; y += 50)
        {
            primitives.Add(new CanvasLine2D(
                mapping.Map(profile.MinimumXMillimeters, y),
                mapping.Map(profile.MaximumXMillimeters, y),
                Color.FromRgb(30, 41, 59),
                Thickness: 1));
        }
    }

    private static void AddPath(
        ICollection<CanvasPrimitive2D> primitives,
        DifferentialDrivePlaybackSnapshot snapshot,
        int frameIndex,
        CoordinateMapping mapping)
    {
        for (var index = 1; index <= frameIndex; index++)
        {
            var previous = snapshot.Frames[index - 1].Pose;
            var current = snapshot.Frames[index].Pose;
            primitives.Add(new CanvasLine2D(
                mapping.Map(previous.X, previous.Y),
                mapping.Map(current.X, current.Y),
                Color.FromRgb(45, 212, 191),
                Thickness: 3));
        }
    }

    private static void AddRobot(
        ICollection<CanvasPrimitive2D> primitives,
        DifferentialDrivePose pose,
        CoordinateMapping mapping)
    {
        const double bodyRadius = 18;
        const double headingLength = 34;
        var center = mapping.Map(pose.X, pose.Y);
        var headingRadians = pose.HeadingDegrees * Math.PI / 180;
        var headingEnd = new Point(
            center.X + (Math.Cos(headingRadians) * headingLength),
            center.Y - (Math.Sin(headingRadians) * headingLength));

        primitives.Add(new CanvasEllipse2D(
            new Rect(center.X - bodyRadius, center.Y - bodyRadius, bodyRadius * 2, bodyRadius * 2),
            Color.FromRgb(37, 99, 235),
            Color.FromRgb(147, 197, 253),
            StrokeThickness: 2));
        primitives.Add(new CanvasLine2D(
            center,
            headingEnd,
            Color.FromRgb(250, 204, 21),
            Thickness: 4));
        primitives.Add(new CanvasEllipse2D(
            new Rect(headingEnd.X - 4, headingEnd.Y - 4, 8, 8),
            Color.FromRgb(250, 204, 21)));
        AddWheel(primitives, center, xOffset: -18);
        AddWheel(primitives, center, xOffset: 10);
    }

    private static void AddWheel(
        ICollection<CanvasPrimitive2D> primitives,
        Point center,
        double xOffset) =>
        primitives.Add(new CanvasRectangle2D(
            new Rect(center.X + xOffset, center.Y - 14, 8, 28),
            Color.FromRgb(15, 23, 42),
            Color.FromRgb(203, 213, 225),
            StrokeThickness: 1,
            CornerRadius: 2));

    private static CoordinateMapping CreateMapping(
        DifferentialDriveProfile profile,
        Size viewportSize,
        double zoomMultiplier)
    {
        var workspaceWidth = profile.MaximumXMillimeters - profile.MinimumXMillimeters;
        var workspaceHeight = profile.MaximumYMillimeters - profile.MinimumYMillimeters;
        var scale = Math.Min(
            (viewportSize.Width - (Padding * 2)) / workspaceWidth,
            (viewportSize.Height - (Padding * 2)) / workspaceHeight) * zoomMultiplier;
        var contentWidth = workspaceWidth * scale;
        var contentHeight = workspaceHeight * scale;

        return new CoordinateMapping(
            profile,
            scale,
            OriginX: (viewportSize.Width - contentWidth) / 2,
            OriginY: (viewportSize.Height + contentHeight) / 2);
    }

    private sealed record CoordinateMapping(
        DifferentialDriveProfile Profile,
        double Scale,
        double OriginX,
        double OriginY)
    {
        public Point Map(double xMillimeters, double yMillimeters) =>
            new(
                OriginX + ((xMillimeters - Profile.MinimumXMillimeters) * Scale),
                OriginY - ((yMillimeters - Profile.MinimumYMillimeters) * Scale));
    }
}
