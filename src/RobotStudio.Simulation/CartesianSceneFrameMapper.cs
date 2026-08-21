namespace RobotStudio.Simulation;

public sealed class CartesianSceneFrameMapper
{
    private const double RailThicknessMillimeters = 12;
    private const double CarriageSizeMillimeters = 24;
    private const double ToolSizeMillimeters = 16;

    public CartesianSceneFrame Map(
        CartesianWorkspaceBounds workspaceBounds,
        CartesianRobotPose pose)
    {
        ArgumentNullException.ThrowIfNull(workspaceBounds);
        ArgumentNullException.ThrowIfNull(pose);

        var workspaceSize = workspaceBounds.Size;
        var primitives = new[]
        {
            new CartesianScenePrimitive(
                "workspace",
                CartesianScenePrimitiveKind.Workspace,
                workspaceBounds.Center,
                workspaceSize),
            new CartesianScenePrimitive(
                "x-rail",
                CartesianScenePrimitiveKind.Rail,
                new VisualVector3(workspaceBounds.Center.XMillimeters, 0, 0),
                new VisualVector3(workspaceSize.XMillimeters, RailThicknessMillimeters, RailThicknessMillimeters)),
            new CartesianScenePrimitive(
                "x-carriage",
                CartesianScenePrimitiveKind.Carriage,
                pose.XAxisCarriage,
                CreateCarriageSize()),
            new CartesianScenePrimitive(
                "y-rail",
                CartesianScenePrimitiveKind.Rail,
                new VisualVector3(pose.XAxisCarriage.XMillimeters, workspaceBounds.Center.YMillimeters, 0),
                new VisualVector3(RailThicknessMillimeters, workspaceSize.YMillimeters, RailThicknessMillimeters)),
            new CartesianScenePrimitive(
                "y-carriage",
                CartesianScenePrimitiveKind.Carriage,
                pose.YAxisCarriage,
                CreateCarriageSize()),
            new CartesianScenePrimitive(
                "z-rail",
                CartesianScenePrimitiveKind.Rail,
                new VisualVector3(
                    pose.YAxisCarriage.XMillimeters,
                    pose.YAxisCarriage.YMillimeters,
                    workspaceBounds.Center.ZMillimeters),
                new VisualVector3(RailThicknessMillimeters, RailThicknessMillimeters, workspaceSize.ZMillimeters)),
            new CartesianScenePrimitive(
                "z-carriage",
                CartesianScenePrimitiveKind.Carriage,
                pose.ZAxisCarriage,
                CreateCarriageSize()),
            new CartesianScenePrimitive(
                "tool",
                CartesianScenePrimitiveKind.Tool,
                pose.ToolCenterPoint,
                new VisualVector3(ToolSizeMillimeters, ToolSizeMillimeters, ToolSizeMillimeters))
        };

        return new CartesianSceneFrame(
            pose.Time,
            pose.State,
            primitives,
            pose.CommandIndex,
            pose.CommandName,
            pose.CommandSource,
            pose.RequestedVelocityMillimetersPerSecond,
            pose.RequestedWaitDuration);
    }

    private static VisualVector3 CreateCarriageSize() =>
        new(CarriageSizeMillimeters, CarriageSizeMillimeters, CarriageSizeMillimeters);
}
