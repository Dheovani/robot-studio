using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class IndustrialArmMechanicalFallbackScene
{
    public static IReadOnlyList<MechanicalScenePrimitive> Create() =>
    [
        Box("base", new(0, 0, 0.12f), new(3.2f, 2.8f, 0.24f), MechanicalMaterialRole.DarkMetal),
        Cylinder("base", new(0, 0, 0.2f), new(0, 0, 0.72f), 1.35f, MechanicalMaterialRole.DarkMetal),
        Box("controller", new(-1.35f, 0, 0.85f), new(0.7f, 1.45f, 1.15f), MechanicalMaterialRole.Accent),
        Cylinder("j1-turntable", new(0, 0, 0.62f), new(0, 0, 1.45f), 1.08f, MechanicalMaterialRole.Frame),
        Box("j1-turntable", new(0, 0, 1.72f), new(1.7f, 1.65f, 1.15f), MechanicalMaterialRole.Platform),
        Cylinder("j2-shoulder", new(0, -0.78f, 2.2f), new(0, 0.78f, 2.2f), 0.8f, MechanicalMaterialRole.Steel),
        Cylinder("j2-motor", new(0, -1.08f, 2.2f), new(0, -0.55f, 2.2f), 0.66f, MechanicalMaterialRole.Motor),
        Cylinder("j2-reduction", new(0, -0.58f, 2.2f), new(0, 0.05f, 2.2f), 0.52f, MechanicalMaterialRole.Transmission),
        Cylinder("upper-arm", new(0, 0, 2.2f), new(0.45f, 0, 3.65f), 0.68f, MechanicalMaterialRole.Platform),
        Cylinder("upper-arm", new(0.45f, 0, 3.65f), new(1.4f, 0, 5.2f), 0.68f, MechanicalMaterialRole.Platform),
        Cylinder("j3-elbow", new(1.4f, -0.72f, 5.2f), new(1.4f, 0.72f, 5.2f), 0.72f, MechanicalMaterialRole.Steel),
        Cylinder("j3-motor", new(1.4f, -1f, 5.2f), new(1.4f, -0.52f, 5.2f), 0.58f, MechanicalMaterialRole.Motor),
        Cylinder("forearm", new(1.4f, 0, 5.2f), new(4f, 0, 5.2f), 0.58f, MechanicalMaterialRole.Platform),
        Cylinder("j4-wrist-roll", new(3.82f, 0, 5.2f), new(4.28f, 0, 5.2f), 0.55f, MechanicalMaterialRole.Steel),
        Cylinder("wrist-roll-housing", new(4.08f, 0, 5.2f), new(4.65f, 0, 5.2f), 0.48f, MechanicalMaterialRole.Frame),
        Cylinder("j5-wrist-bend", new(4.65f, -0.5f, 5.2f), new(4.65f, 0.5f, 5.2f), 0.5f, MechanicalMaterialRole.Steel),
        Cylinder("wrist-bend-housing", new(4.65f, 0, 5.2f), new(5.15f, 0, 5.2f), 0.4f, MechanicalMaterialRole.Frame),
        Cylinder("j6-tool-roll", new(5.02f, 0, 5.2f), new(5.42f, 0, 5.2f), 0.38f, MechanicalMaterialRole.Steel),
        Box("tool", new(5.62f, 0, 5.2f), new(0.48f, 1.05f, 0.88f), MechanicalMaterialRole.Tool),
        Box("tool", new(5.95f, -0.66f, 5.2f), new(0.95f, 0.18f, 0.26f), MechanicalMaterialRole.Steel),
        Box("tool", new(5.95f, 0.66f, 5.2f), new(0.95f, 0.18f, 0.26f), MechanicalMaterialRole.Steel)
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
