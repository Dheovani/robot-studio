using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class DeltaMechanicalFallbackScene
{
    private const float CarriageRadius = 3.15f;
    private const float PlatformRadius = 0.75f;
    private const float CarriageZ = 4.65f;
    private const float PlatformZ = 1.65f;

    public static IReadOnlyList<MechanicalScenePrimitive> Create()
    {
        var primitives = new List<MechanicalScenePrimitive>
        {
            Box("base", new(0, 0, 6.55f), new(1.1f, 7.4f, 0.42f), MechanicalMaterialRole.Frame),
            Box("base", new(-3.2f, -1.85f, 6.55f), new(1.1f, 7.4f, 0.42f), MechanicalMaterialRole.Frame),
            Box("base", new(3.2f, -1.85f, 6.55f), new(1.1f, 7.4f, 0.42f), MechanicalMaterialRole.Frame),
            Box("controller", new(0, -2.35f, 5.57f), new(2.2f, 1.1f, 1.35f), MechanicalMaterialRole.Accent),
            Box("controller", new(-0.75f, -1.92f, 6.3f), new(0.18f, 0.34f, 0.48f), MechanicalMaterialRole.Steel),
            Box("controller", new(0.75f, -1.92f, 6.3f), new(0.18f, 0.34f, 0.48f), MechanicalMaterialRole.Steel),
            Cylinder("platform", new(0, 0, 1.5f), new(0, 0, 1.8f), 0.95f, MechanicalMaterialRole.Platform),
            Cylinder("tool", new(0, 0, 0.65f), new(0, 0, 1.55f), 0.3f, MechanicalMaterialRole.Tool)
        };

        foreach (var station in Stations)
        {
            var railCenter = station.Radial * 3.2f;
            primitives.Add(Cylinder(
                $"actuator-{station.Id}",
                new Vector3(railCenter, 2.65f),
                new Vector3(railCenter, 6.2f),
                0.24f,
                MechanicalMaterialRole.Steel));
            primitives.Add(Box(
                $"motor-{station.Id}",
                new Vector3(railCenter, 6.12f),
                new Vector3(0.82f, 0.82f, 0.72f),
                MechanicalMaterialRole.Motor));
            primitives.Add(Box(
                $"carriage-{station.Id}",
                new Vector3(station.Radial * CarriageRadius, CarriageZ),
                new Vector3(0.88f, 0.72f, 0.58f),
                MechanicalMaterialRole.Accent));

            AddLink(primitives, station, "left", -1);
            AddLink(primitives, station, "right", 1);
        }

        return primitives;
    }

    private static void AddLink(
        ICollection<MechanicalScenePrimitive> primitives,
        Station station,
        string side,
        float direction)
    {
        var start = new Vector3(station.Radial * CarriageRadius, CarriageZ) +
                    new Vector3(station.Tangent * (0.22f * direction), 0);
        var end = new Vector3(station.Radial * PlatformRadius, PlatformZ) +
                  new Vector3(station.Tangent * (0.18f * direction), 0);
        primitives.Add(Cylinder(
            $"link-{station.Id}-{side}",
            start,
            end,
            0.085f,
            MechanicalMaterialRole.Frame));
    }

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

    private static IReadOnlyList<Station> Stations { get; } =
    [
        new("a", new Vector2(0, 1), new Vector2(-1, 0)),
        new("b", new Vector2(-0.8660254f, -0.5f), new Vector2(0.5f, -0.8660254f)),
        new("c", new Vector2(0.8660254f, -0.5f), new Vector2(0.5f, 0.8660254f))
    ];

    private sealed record Station(string Id, Vector2 Radial, Vector2 Tangent);
}
