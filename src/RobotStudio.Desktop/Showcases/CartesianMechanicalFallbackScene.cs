using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class CartesianMechanicalFallbackScene
{
    public static IReadOnlyList<MechanicalScenePrimitive> Create() =>
    [
        Box("base", new(0, 0, 0.25f), new(9.5f, 8, 0.5f), MechanicalMaterialRole.DarkMetal),
        Box("base", new(-4.25f, -3.5f, 0.05f), new(0.65f, 0.65f, 0.3f), MechanicalMaterialRole.DarkMetal),
        Box("base", new(4.25f, -3.5f, 0.05f), new(0.65f, 0.65f, 0.3f), MechanicalMaterialRole.DarkMetal),
        Box("controller", new(3.55f, -3.2f, 0.95f), new(1.7f, 1.1f, 1.25f), MechanicalMaterialRole.Accent),

        Box("left-y-rail", new(-2.45f, -0.45f, 0.78f), new(0.22f, 5.8f, 0.22f), MechanicalMaterialRole.Steel),
        Box("right-y-rail", new(2.45f, -0.45f, 0.78f), new(0.22f, 5.8f, 0.22f), MechanicalMaterialRole.Steel),
        Cylinder("y-motor", new(0, -3.65f, 0.78f), new(0, -3.05f, 0.78f), 0.42f, MechanicalMaterialRole.Motor),
        Box("y-belt", new(0, -0.45f, 0.82f), new(0.12f, 5.7f, 0.1f), MechanicalMaterialRole.Transmission),
        Box("y-bed-carriage", new(0, -0.8f, 1.02f), new(6.7f, 5.5f, 0.35f), MechanicalMaterialRole.Frame),
        Box("build-plate", new(0, -0.8f, 1.27f), new(6.3f, 5.1f, 0.16f), MechanicalMaterialRole.Platform),

        Box("left-frame-column", new(-4.05f, 2.65f, 4.35f), new(0.5f, 0.55f, 7.2f), MechanicalMaterialRole.Frame),
        Box("right-frame-column", new(4.05f, 2.65f, 4.35f), new(0.5f, 0.55f, 7.2f), MechanicalMaterialRole.Frame),
        Box("top-frame-beam", new(0, 2.65f, 7.95f), new(8.6f, 0.55f, 0.5f), MechanicalMaterialRole.Frame),
        Cylinder("left-z-guide", new(-3.7f, 2.4f, 1.05f), new(-3.7f, 2.4f, 7.55f), 0.11f, MechanicalMaterialRole.Steel),
        Cylinder("right-z-guide", new(3.7f, 2.4f, 1.05f), new(3.7f, 2.4f, 7.55f), 0.11f, MechanicalMaterialRole.Steel),
        Cylinder("left-z-screw", new(-3.45f, 2.8f, 1.1f), new(-3.45f, 2.8f, 7.55f), 0.09f, MechanicalMaterialRole.Steel),
        Cylinder("right-z-screw", new(3.45f, 2.8f, 1.1f), new(3.45f, 2.8f, 7.55f), 0.09f, MechanicalMaterialRole.Steel),
        Cylinder("left-z-motor", new(-3.45f, 2.8f, 0.65f), new(-3.45f, 2.8f, 1.15f), 0.38f, MechanicalMaterialRole.Motor),
        Cylinder("right-z-motor", new(3.45f, 2.8f, 0.65f), new(3.45f, 2.8f, 1.15f), 0.38f, MechanicalMaterialRole.Motor),

        Box("z-gantry", new(0, 2.5f, 5.4f), new(8.2f, 0.62f, 0.62f), MechanicalMaterialRole.Accent),
        Box("z-gantry", new(-3.7f, 2.5f, 5.4f), new(0.75f, 0.9f, 0.9f), MechanicalMaterialRole.Frame),
        Box("z-gantry", new(3.7f, 2.5f, 5.4f), new(0.75f, 0.9f, 0.9f), MechanicalMaterialRole.Frame),
        Box("x-rail", new(0, 2.15f, 5.4f), new(7.25f, 0.18f, 0.22f), MechanicalMaterialRole.Steel),
        Box("x-belt", new(0, 2.02f, 5.65f), new(7.1f, 0.1f, 0.1f), MechanicalMaterialRole.Transmission),
        Cylinder("x-motor", new(-4.2f, 2.5f, 5.4f), new(-3.7f, 2.5f, 5.4f), 0.4f, MechanicalMaterialRole.Motor),
        Box("x-tool-carriage", new(-1.6f, 1.92f, 5.35f), new(0.9f, 0.75f, 1.05f), MechanicalMaterialRole.Accent),
        Box("tool", new(-1.6f, 1.65f, 4.65f), new(0.62f, 0.62f, 0.5f), MechanicalMaterialRole.DarkMetal),
        Cylinder("tool", new(-1.6f, 1.65f, 4.45f), new(-1.6f, 1.65f, 3.9f), 0.16f, MechanicalMaterialRole.Tool)
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
