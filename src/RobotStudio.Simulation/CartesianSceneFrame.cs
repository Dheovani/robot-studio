using RobotStudio.Domain;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation;

public sealed record CartesianSceneFrame(
    TimeSpan Time,
    RobotState State,
    IReadOnlyList<CartesianScenePrimitive> Primitives,
    int? CommandIndex,
    string? CommandName,
    RobotCommandSource? CommandSource)
{
    public int PrimitiveCount => Primitives.Count;
}
