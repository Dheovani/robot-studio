using System.Windows;
using System.Windows.Input;

namespace RobotStudio.Desktop.Rendering;

internal sealed class ViewportOrbitInteractionState
{
    private bool isDragging;
    private Point lastPosition;

    public void BeginDrag(
        FrameworkElement host,
        IInputElement coordinateElement,
        MouseButtonEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(coordinateElement);
        ArgumentNullException.ThrowIfNull(e);

        isDragging = true;
        lastPosition = e.GetPosition(coordinateElement);
        host.CaptureMouse();
        host.Cursor = Cursors.SizeAll;
        e.Handled = true;
    }

    public void EndDrag(
        FrameworkElement host,
        MouseButtonEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(e);

        isDragging = false;
        host.ReleaseMouseCapture();
        host.Cursor = Cursors.Hand;
        e.Handled = true;
    }

    public bool TryGetDragDelta(
        IInputElement coordinateElement,
        MouseEventArgs e,
        out double deltaX,
        out double deltaY)
    {
        ArgumentNullException.ThrowIfNull(coordinateElement);
        ArgumentNullException.ThrowIfNull(e);

        if (!isDragging)
        {
            deltaX = 0;
            deltaY = 0;
            return false;
        }

        var currentPosition = e.GetPosition(coordinateElement);
        deltaX = currentPosition.X - lastPosition.X;
        deltaY = currentPosition.Y - lastPosition.Y;
        lastPosition = currentPosition;

        return true;
    }
}
