using System.Windows;
using System.Windows.Media;

namespace RobotStudio.Desktop.Rendering;

internal sealed class CanvasScene2D(IEnumerable<CanvasPrimitive2D> primitives)
{
    public IReadOnlyList<CanvasPrimitive2D> Primitives { get; } =
        primitives?.ToArray() ?? throw new ArgumentNullException(nameof(primitives));
}

internal abstract record CanvasPrimitive2D;

internal sealed record CanvasLine2D(
    Point Start,
    Point End,
    Color Color,
    double Thickness) : CanvasPrimitive2D;

internal sealed record CanvasRectangle2D(
    Rect Bounds,
    Color Fill,
    Color Stroke,
    double StrokeThickness,
    double CornerRadius = 0) : CanvasPrimitive2D;

internal sealed record CanvasEllipse2D(
    Rect Bounds,
    Color Fill,
    Color? Stroke = null,
    double StrokeThickness = 0) : CanvasPrimitive2D;
