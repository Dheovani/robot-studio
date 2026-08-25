using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class ScaraMechanicalFallbackScene
{
    public static IReadOnlyList<MechanicalScenePrimitive> Create() =>
    [
        Box("base", new(0, 0, 0.16f), new(2.4f, 2.15f, 0.32f), MechanicalMaterialRole.DarkMetal),
        Box("base", new(0, 0, 1.85f), new(1.78f, 1.62f, 3.38f), MechanicalMaterialRole.Platform),
        Cylinder("base", new(0, 0, 3.55f), new(0, 0, 4.28f), 1.02f, MechanicalMaterialRole.Platform),
        Box("controller", new(0, 0, 2.1f), new(1.35f, 1.2f, 1.25f), MechanicalMaterialRole.Accent),
        Cylinder("shoulder-motor", new(0, 0, 3.3f), new(0, 0, 4.15f), 0.62f, MechanicalMaterialRole.Motor),
        Cylinder("shoulder-transmission", new(0, 0, 4.05f), new(0, 0, 4.55f), 0.82f, MechanicalMaterialRole.Transmission),
        Box("first-link", new(1.62f, 0, 4.48f), new(3.25f, 0.55f, 0.34f), MechanicalMaterialRole.Frame),
        Box("first-link-cover", new(1.62f, 0, 4.72f), new(3.05f, 1.14f, 0.82f), MechanicalMaterialRole.Platform),
        Cylinder("elbow-joint", new(3.25f, 0, 4.15f), new(3.25f, 0, 5.05f), 0.72f, MechanicalMaterialRole.Accent),
        Cylinder("elbow-motor", new(3.25f, 0, 4.68f), new(3.25f, 0, 5.42f), 0.46f, MechanicalMaterialRole.Motor),
        Box("second-link", new(4.72f, 0, 4.42f), new(2.95f, 0.48f, 0.3f), MechanicalMaterialRole.Frame),
        Box("second-link-cover", new(4.62f, 0, 4.74f), new(2.55f, 1.18f, 0.9f), MechanicalMaterialRole.Platform),
        Box("second-link-cover", new(5.82f, 0, 5.02f), new(1.18f, 1.48f, 1.58f), MechanicalMaterialRole.Platform),
        Cylinder("z-motor", new(6.15f, 0, 4.55f), new(6.15f, 0, 5.45f), 0.48f, MechanicalMaterialRole.Motor),
        Cylinder("z-actuator", new(6.15f, 0, 2.35f), new(6.15f, 0, 5.15f), 0.34f, MechanicalMaterialRole.Accent),
        Cylinder("tool", new(6.15f, 0, 2.02f), new(6.15f, 0, 2.52f), 0.48f, MechanicalMaterialRole.Tool),
        Box("tool", new(6.15f, -0.34f, 1.75f), new(0.22f, 0.18f, 0.72f), MechanicalMaterialRole.Tool),
        Box("tool", new(6.15f, 0.34f, 1.75f), new(0.22f, 0.18f, 0.72f), MechanicalMaterialRole.Tool)
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
