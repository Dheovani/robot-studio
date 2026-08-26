using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RobotStudio.Desktop.Rendering;

internal sealed class WpfCanvasScenePresenter(Canvas canvas)
{
    private readonly Canvas canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));

    public void Present(CanvasScene2D scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        canvas.Children.Clear();
        foreach (var primitive in scene.Primitives)
        {
            AddPrimitive(primitive);
        }
    }

    private void AddPrimitive(CanvasPrimitive2D primitive)
    {
        switch (primitive)
        {
            case CanvasLine2D line:
                canvas.Children.Add(new Line
                {
                    X1 = line.Start.X,
                    Y1 = line.Start.Y,
                    X2 = line.End.X,
                    Y2 = line.End.Y,
                    Stroke = new SolidColorBrush(line.Color),
                    StrokeThickness = line.Thickness
                });
                break;

            case CanvasRectangle2D rectangle:
                var rectangleShape = new Rectangle
                {
                    Width = rectangle.Bounds.Width,
                    Height = rectangle.Bounds.Height,
                    Fill = new SolidColorBrush(rectangle.Fill),
                    Stroke = new SolidColorBrush(rectangle.Stroke),
                    StrokeThickness = rectangle.StrokeThickness,
                    RadiusX = rectangle.CornerRadius,
                    RadiusY = rectangle.CornerRadius
                };
                canvas.Children.Add(rectangleShape);
                Canvas.SetLeft(rectangleShape, rectangle.Bounds.X);
                Canvas.SetTop(rectangleShape, rectangle.Bounds.Y);
                break;

            case CanvasEllipse2D ellipse:
                var ellipseShape = new Ellipse
                {
                    Width = ellipse.Bounds.Width,
                    Height = ellipse.Bounds.Height,
                    Fill = new SolidColorBrush(ellipse.Fill),
                    Stroke = ellipse.Stroke is Color stroke
                        ? new SolidColorBrush(stroke)
                        : null,
                    StrokeThickness = ellipse.StrokeThickness
                };
                canvas.Children.Add(ellipseShape);
                Canvas.SetLeft(ellipseShape, ellipse.Bounds.X);
                Canvas.SetTop(ellipseShape, ellipse.Bounds.Y);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported 2D canvas primitive '{primitive.GetType().Name}'.");
        }
    }
}
