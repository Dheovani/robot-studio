namespace RobotStudio.Simulation;

public sealed class CartesianRobotPoseMapper
{
    public CartesianRobotPose Map(RobotVisualState visualState)
    {
        ArgumentNullException.ThrowIfNull(visualState);

        var toolCenterPoint = visualState.Position;

        return new CartesianRobotPose(
            visualState.Time,
            visualState.State,
            Base: new VisualVector3(0, 0, 0),
            XAxisCarriage: new VisualVector3(toolCenterPoint.XMillimeters, 0, 0),
            YAxisCarriage: new VisualVector3(toolCenterPoint.XMillimeters, toolCenterPoint.YMillimeters, 0),
            ZAxisCarriage: toolCenterPoint,
            ToolCenterPoint: toolCenterPoint,
            visualState.CommandIndex,
            visualState.CommandName,
            visualState.CommandSource);
    }
}
