using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class DifferentialDriveMechanicalFallbackScene
{
    public static IReadOnlyList<MechanicalScenePrimitive> Create() =>
    [
        Cylinder("base", new(0, 0, 0.72f), new(0, 0, 1.25f), 3.3f, MechanicalMaterialRole.Frame),
        Cylinder("upper-shell", new(0, 0, 1.22f), new(0, 0, 2.05f), 3.02f, MechanicalMaterialRole.Accent),
        Cylinder("upper-shell", new(0, 0, 2.04f), new(0, 0, 2.18f), 2.76f, MechanicalMaterialRole.Platform),
        Box("controller", new(0.85f, 0, 1.65f), new(1.7f, 1.55f, 0.35f), MechanicalMaterialRole.Accent),
        Box("battery", new(-1.25f, 0, 1.55f), new(1.8f, 2.2f, 0.65f), MechanicalMaterialRole.Power),

        Cylinder("left-motor", new(0, -1.65f, 1.05f), new(0, -2.1f, 1.05f), 0.48f, MechanicalMaterialRole.Motor),
        Cylinder("left-encoder", new(0, -1.4f, 1.05f), new(0, -1.62f, 1.05f), 0.32f, MechanicalMaterialRole.Sensor),
        Cylinder("left-wheel", new(0, -2.1f, 1.05f), new(0, -2.8f, 1.05f), 1.05f, MechanicalMaterialRole.Transmission),
        Cylinder("right-motor", new(0, 1.65f, 1.05f), new(0, 2.1f, 1.05f), 0.48f, MechanicalMaterialRole.Motor),
        Cylinder("right-encoder", new(0, 1.4f, 1.05f), new(0, 1.62f, 1.05f), 0.32f, MechanicalMaterialRole.Sensor),
        Cylinder("right-wheel", new(0, 2.1f, 1.05f), new(0, 2.8f, 1.05f), 1.05f, MechanicalMaterialRole.Transmission),

        Cylinder("caster", new(2.15f, 0, 0.62f), new(2.15f, 0, 0.2f), 0.42f, MechanicalMaterialRole.Steel),
        Box("front-sensor", new(2.72f, 0, 1.55f), new(0.32f, 1.35f, 0.55f), MechanicalMaterialRole.Sensor),
        Cylinder("bumper", new(0, 0, 0.88f), new(0, 0, 1.28f), 3.46f, MechanicalMaterialRole.DarkMetal)
    ];

    private static MechanicalBoxPrimitive Box(
        string partId,
        Vector3 center,
        Vector3 size,
        MechanicalMaterialRole materialRole) =>
        new(new RobotPartId(partId), center, size, materialRole);

    private static MechanicalCylinderPrimitive Cylinder(
        string partId,
        Vector3 start,
        Vector3 end,
        float radius,
        MechanicalMaterialRole materialRole) =>
        new(new RobotPartId(partId), start, end, radius, materialRole);
}
