using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class SimpleArmMechanicalFallbackScene
{
    public static IReadOnlyList<MechanicalScenePrimitive> Create() =>
    [
        Cylinder("base", new(0, 0, 0), new(0, 0, 0.65f), 1.4f, MechanicalMaterialRole.DarkMetal),
        Box("controller", new(-0.72f, 0, 0.9f), new(0.8f, 1.25f, 1.1f), MechanicalMaterialRole.Accent),
        Cylinder("base-motor", new(0, 0, 0.45f), new(0, 0, 1.35f), 0.62f, MechanicalMaterialRole.Motor),
        Cylinder("turntable", new(0, 0, 0.62f), new(0, 0, 1.5f), 1.02f, MechanicalMaterialRole.Frame),
        Box("turntable", new(0, 0, 1.72f), new(1.35f, 1.55f, 1.1f), MechanicalMaterialRole.Platform),
        Cylinder("shoulder-joint", new(0, -0.72f, 2.2f), new(0, 0.72f, 2.2f), 0.72f, MechanicalMaterialRole.Steel),
        Cylinder("shoulder-motor", new(0, -1.05f, 2.2f), new(0, -0.55f, 2.2f), 0.62f, MechanicalMaterialRole.Motor),
        Cylinder("shoulder-transmission", new(0, -0.55f, 2.2f), new(0, 0.05f, 2.2f), 0.48f, MechanicalMaterialRole.Transmission),
        Cylinder("upper-arm", new(0, 0, 2.2f), new(2.7f, 0, 4.6f), 0.42f, MechanicalMaterialRole.Frame),
        Cylinder("upper-arm-cover", new(0, 0, 2.2f), new(2.7f, 0, 4.6f), 0.68f, MechanicalMaterialRole.Platform),
        Cylinder("elbow-joint", new(2.7f, -0.62f, 4.6f), new(2.7f, 0.62f, 4.6f), 0.68f, MechanicalMaterialRole.Steel),
        Cylinder("elbow-motor", new(2.7f, -0.92f, 4.6f), new(2.7f, -0.48f, 4.6f), 0.56f, MechanicalMaterialRole.Motor),
        Cylinder("elbow-transmission", new(2.7f, -0.5f, 4.6f), new(2.7f, 0.05f, 4.6f), 0.42f, MechanicalMaterialRole.Transmission),
        Cylinder("forearm", new(2.7f, 0, 4.6f), new(5.3f, 0, 3.4f), 0.36f, MechanicalMaterialRole.Frame),
        Cylinder("forearm-cover", new(2.7f, 0, 4.6f), new(5.3f, 0, 3.4f), 0.6f, MechanicalMaterialRole.Platform),
        Cylinder("wrist", new(5.3f, 0, 3.4f), new(5.75f, 0, 3.05f), 0.48f, MechanicalMaterialRole.Accent),
        Cylinder("tool", new(5.75f, 0, 3.05f), new(6.35f, 0, 2.65f), 0.28f, MechanicalMaterialRole.Tool)
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
