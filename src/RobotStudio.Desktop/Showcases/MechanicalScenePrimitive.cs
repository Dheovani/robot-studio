using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal enum MechanicalMaterialRole
{
    Frame,
    DarkMetal,
    Steel,
    Accent,
    Platform,
    Motor,
    Transmission,
    Tool
}

internal abstract record MechanicalScenePrimitive(
    RobotPartId PartId,
    MechanicalMaterialRole MaterialRole);

internal sealed record MechanicalBoxPrimitive(
    RobotPartId PartId,
    Vector3 Center,
    Vector3 Size,
    MechanicalMaterialRole MaterialRole)
    : MechanicalScenePrimitive(PartId, MaterialRole);

internal sealed record MechanicalCylinderPrimitive(
    RobotPartId PartId,
    Vector3 Start,
    Vector3 End,
    float Radius,
    MechanicalMaterialRole MaterialRole)
    : MechanicalScenePrimitive(PartId, MaterialRole);
