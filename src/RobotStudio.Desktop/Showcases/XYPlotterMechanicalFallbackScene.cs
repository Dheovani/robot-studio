using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class XYPlotterMechanicalFallbackScene
{
    public static IReadOnlyList<MechanicalScenePrimitive> Create() =>
    [
        Box("base", new(0, 0, 0.25f), new(10, 8, 0.5f), MechanicalMaterialRole.DarkMetal),
        Box("controller", new(3.9f, -3.25f, 0.85f), new(1.65f, 1.15f, 1.1f), MechanicalMaterialRole.Accent),
        Box("paper-bed", new(0, -0.25f, 0.72f), new(7.4f, 5.7f, 0.18f), MechanicalMaterialRole.Platform),
        Box("paper-bed", new(0, -0.25f, 0.84f), new(6.8f, 5.1f, 0.05f), MechanicalMaterialRole.Frame),

        Box("left-y-rail", new(-4.1f, -0.15f, 1.05f), new(0.28f, 6.5f, 0.28f), MechanicalMaterialRole.Steel),
        Box("right-y-rail", new(4.1f, -0.15f, 1.05f), new(0.28f, 6.5f, 0.28f), MechanicalMaterialRole.Steel),
        Cylinder("y-motor", new(-4.1f, -3.65f, 1.05f), new(-4.1f, -3.05f, 1.05f), 0.4f, MechanicalMaterialRole.Motor),
        Box("left-y-belt", new(-3.75f, -0.15f, 1.12f), new(0.1f, 6.3f, 0.1f), MechanicalMaterialRole.Transmission),
        Box("right-y-belt", new(3.75f, -0.15f, 1.12f), new(0.1f, 6.3f, 0.1f), MechanicalMaterialRole.Transmission),

        Box("y-gantry", new(0, -1.5f, 2.15f), new(8.8f, 0.72f, 0.72f), MechanicalMaterialRole.Accent),
        Box("y-gantry", new(-4.05f, -1.5f, 1.55f), new(0.75f, 1.05f, 1.4f), MechanicalMaterialRole.Frame),
        Box("y-gantry", new(4.05f, -1.5f, 1.55f), new(0.75f, 1.05f, 1.4f), MechanicalMaterialRole.Frame),
        Box("x-rail", new(0, -1.86f, 2.15f), new(7.6f, 0.2f, 0.22f), MechanicalMaterialRole.Steel),
        Box("x-belt", new(0, -1.98f, 2.42f), new(7.5f, 0.1f, 0.1f), MechanicalMaterialRole.Transmission),
        Cylinder("x-motor", new(-4.45f, -1.5f, 2.15f), new(-3.95f, -1.5f, 2.15f), 0.4f, MechanicalMaterialRole.Motor),

        Box("x-carriage", new(-1.8f, -1.9f, 2.05f), new(0.95f, 0.8f, 1.05f), MechanicalMaterialRole.Accent),
        Box("pen-lift", new(-1.8f, -2.15f, 1.45f), new(0.62f, 0.62f, 0.55f), MechanicalMaterialRole.DarkMetal),
        Cylinder("pen", new(-1.8f, -2.15f, 1.35f), new(-1.8f, -2.15f, 0.82f), 0.11f, MechanicalMaterialRole.Tool)
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
