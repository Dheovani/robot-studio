namespace RobotStudio.Simulation;

public sealed class CartesianViewportPlanner
{
    private const double MinimumWorkspaceSizeMillimeters = 1;
    private const double CameraDistanceMultiplier = 2.2;
    private const double MinimumNearClipMillimeters = 1;

    public CartesianViewportSnapshot Plan(CartesianWorkspaceBounds workspaceBounds)
    {
        ArgumentNullException.ThrowIfNull(workspaceBounds);

        var size = workspaceBounds.Size;
        var largestDimension = Math.Max(
            MinimumWorkspaceSizeMillimeters,
            Math.Max(
                size.XMillimeters,
                Math.Max(size.YMillimeters, size.ZMillimeters)));
        var cameraDistance = largestDimension * CameraDistanceMultiplier;
        var target = workspaceBounds.Center;

        return new CartesianViewportSnapshot(
            target,
            new VisualVector3(
                target.XMillimeters + cameraDistance,
                target.YMillimeters - cameraDistance,
                target.ZMillimeters + cameraDistance),
            Up: new VisualVector3(0, 0, 1),
            NearClipMillimeters: MinimumNearClipMillimeters,
            FarClipMillimeters: cameraDistance * 4);
    }
}
