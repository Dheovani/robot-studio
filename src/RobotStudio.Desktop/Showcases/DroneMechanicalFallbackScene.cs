using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class DroneMechanicalFallbackScene
{
    public static IReadOnlyList<MechanicalScenePrimitive> Create()
    {
        var primitives = new List<MechanicalScenePrimitive>
        {
            Box("airframe", Vector3.Zero, new(2.35f, 1.45f, 0.28f), MechanicalMaterialRole.Frame),
            Box("battery", new(0, 0.18f, -0.22f), new(1.65f, 0.9f, 0.42f), MechanicalMaterialRole.Power),
            Box("flight-controller", new(0, 0, 0.22f), new(0.9f, 0.9f, 0.18f), MechanicalMaterialRole.Accent),
            Box("imu", new(0, 0, 0.38f), new(0.36f, 0.36f, 0.16f), MechanicalMaterialRole.Sensor),
            Box("camera", new(0, -1.05f, -0.68f), new(0.64f, 0.48f, 0.42f), MechanicalMaterialRole.Sensor),
            Cylinder("landing-gear", new(-0.92f, -0.52f, -0.38f), new(-0.92f, -0.52f, -0.86f), 0.08f, MechanicalMaterialRole.DarkMetal),
            Cylinder("landing-gear", new(0.92f, -0.52f, -0.38f), new(0.92f, -0.52f, -0.86f), 0.08f, MechanicalMaterialRole.DarkMetal),
            Cylinder("landing-gear", new(-0.92f, 0.52f, -0.38f), new(-0.92f, 0.52f, -0.86f), 0.08f, MechanicalMaterialRole.DarkMetal),
            Cylinder("landing-gear", new(0.92f, 0.52f, -0.38f), new(0.92f, 0.52f, -0.86f), 0.08f, MechanicalMaterialRole.DarkMetal)
        };

        AddRoundedBody(primitives, new(0, 0, -0.12f), 3.05f, 2.15f, 0.72f, 0.42f, MechanicalMaterialRole.DarkMetal);
        AddRoundedBody(primitives, new(0, -0.02f, 0.34f), 2.92f, 2.02f, 0.5f, 0.4f, MechanicalMaterialRole.Platform);
        primitives.Add(Box("shell", new(0, 0.1f, 0.62f), new(1.7f, 1.08f, 0.08f), MechanicalMaterialRole.Accent));

        foreach (var rotor in Rotors)
        {
            primitives.Add(Cylinder(
                rotor.ArmId,
                new(rotor.Center.X * 0.34f, rotor.Center.Y * 0.34f, 0.08f),
                new(rotor.Center.X, rotor.Center.Y, 0.2f),
                0.22f,
                MechanicalMaterialRole.Frame));
            primitives.Add(Cylinder(
                rotor.MotorId,
                new(rotor.Center.X, rotor.Center.Y, 0.18f),
                new(rotor.Center.X, rotor.Center.Y, 0.72f),
                0.38f,
                MechanicalMaterialRole.Motor));
            AddPropeller(primitives, rotor.PropellerId, rotor.Center);
        }

        return primitives;
    }

    private static void AddRoundedBody(
        ICollection<MechanicalScenePrimitive> primitives,
        Vector3 center,
        float width,
        float depth,
        float height,
        float cornerRadius,
        MechanicalMaterialRole materialRole)
    {
        primitives.Add(Box("shell", center, new(width - (cornerRadius * 2), depth, height), materialRole));
        primitives.Add(Box("shell", center, new(width, depth - (cornerRadius * 2), height), materialRole));
        foreach (var x in new[] { -1f, 1f })
        {
            foreach (var y in new[] { -1f, 1f })
            {
                var corner = center + new Vector3(
                    x * ((width / 2) - cornerRadius),
                    y * ((depth / 2) - cornerRadius),
                    0);
                primitives.Add(Cylinder(
                    "shell",
                    corner - new Vector3(0, 0, height / 2),
                    corner + new Vector3(0, 0, height / 2),
                    cornerRadius,
                    materialRole));
            }
        }
    }

    private static void AddPropeller(
        ICollection<MechanicalScenePrimitive> primitives,
        string partId,
        Vector2 center)
    {
        var bladeAngle = center.X * center.Y > 0 ? MathF.PI / 4 : -MathF.PI / 4;
        for (var blade = 0; blade < 2; blade++)
        {
            var angle = bladeAngle + (blade * MathF.PI);
            var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            primitives.Add(Box(
                partId,
                new Vector3(center + (direction * 0.78f), 0.82f),
                new(1.45f, 0.34f, 0.09f),
                MechanicalMaterialRole.Tool));
        }
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

    private static IReadOnlyList<Rotor> Rotors { get; } =
    [
        new("arm-front-left", "motor-front-left", "propeller-front-left", new Vector2(-2.55f, -2.55f)),
        new("arm-front-right", "motor-front-right", "propeller-front-right", new Vector2(2.55f, -2.55f)),
        new("arm-rear-left", "motor-rear-left", "propeller-rear-left", new Vector2(-2.55f, 2.55f)),
        new("arm-rear-right", "motor-rear-right", "propeller-rear-right", new Vector2(2.55f, 2.55f))
    ];

    private sealed record Rotor(string ArmId, string MotorId, string PropellerId, Vector2 Center);
}
