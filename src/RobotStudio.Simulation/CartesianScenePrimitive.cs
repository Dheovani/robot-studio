namespace RobotStudio.Simulation;

public sealed record CartesianScenePrimitive(
    string Id,
    CartesianScenePrimitiveKind Kind,
    VisualVector3 Center,
    VisualVector3 Size);
