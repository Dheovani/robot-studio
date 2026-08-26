using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class DeltaMechanicalShowcaseDefinition
{
    private const float SqrtThree = 1.7320508f;
    private const float CarriageRadiusMillimeters = 315;
    private const float PlatformRadiusMillimeters = 75;
    private const float CarriageZMillimeters = 465;
    private const float PlatformZMillimeters = 165;

    private static readonly RobotPartId BaseId = new("base");
    private static readonly RobotPartId PlatformId = new("platform");

    public static MechanicalShowcasePresentation CreatePresentation()
    {
        var showcase = Create();
        return new MechanicalShowcasePresentation(
            showcase.Model.Id,
            "Delta Robot",
            "Overhead linear Delta with three synchronized carriages, six parallel links, and a moving tool platform",
            "DeltaMechanical",
            showcase,
            new RobotPartId("platform"),
            DeltaMechanicalTeachingViewCatalog.Options,
            DeltaMechanicalTeachingViewCatalog.MotionAxes,
            DeltaMechanicalTeachingViewCatalog.ExplodedOffsets,
            DeltaMechanicalFallbackScene.Create(),
            parallelLinkConstraints: CreateParallelLinkConstraints());
    }

    public static MechanicalShowcaseDefinition Create()
    {
        var parts = new List<RobotPartDefinition>
        {
            Part(BaseId, "Overhead support frame", RobotPartKind.Base, null,
                "Supports the complete parallel mechanism above its work area.",
                "Remains fixed while actuator and link forces close through the frame."),
            Part("controller", "Motion controller", RobotPartKind.Controller, BaseId,
                "Synchronizes all three linear actuator positions.",
                "Remains fixed on the overhead frame."),
            Part(PlatformId, "Moving platform", RobotPartKind.Carriage, BaseId,
                "Combines the six link constraints into one translational end-effector platform.",
                "Moves in X, Y, and Z while remaining parallel to the fixed frame."),
            Part("tool", "Vacuum pick tool", RobotPartKind.Tool, PlatformId,
                "Represents a lightweight tool suited to fast pick-and-place work.",
                "Follows the moving platform without adding another controlled axis.")
        };

        foreach (var station in Stations)
        {
            var actuatorId = new RobotPartId($"actuator-{station.Id}");
            var carriageId = new RobotPartId($"carriage-{station.Id}");
            parts.Add(Part(actuatorId, $"Actuator {station.Label}", RobotPartKind.Actuator, BaseId,
                "Provides one independently commanded vertical coordinate.",
                "Guides its carriage along the fixed vertical rail."));
            parts.Add(Part($"motor-{station.Id}", $"Servo motor {station.Label}", RobotPartKind.Motor, actuatorId,
                "Drives the actuator lead screw or equivalent linear transmission.",
                "Rotates internally while the motor housing remains attached to the frame."));
            parts.Add(Part(carriageId, $"Carriage {station.Label}", RobotPartKind.Carriage, actuatorId,
                "Provides the moving attachment points for one pair of parallel links.",
                "Translates vertically according to its actuator coordinate."));
            parts.Add(Part($"link-{station.Id}-left", $"Link {station.Label} left", RobotPartKind.Link, BaseId,
                "Connects one side of the carriage to the moving platform.",
                "Changes orientation and effective span while remaining attached at both ends."));
            parts.Add(Part($"link-{station.Id}-right", $"Link {station.Label} right", RobotPartKind.Link, BaseId,
                "Forms a parallelogram pair that resists platform rotation.",
                "Moves with its paired link while preserving the platform orientation."));
        }

        var model = new RobotVisualModelDefinition(
            "delta-mechanical",
            "Three-Actuator Linear Delta Robot",
            BaseId,
            parts);

        var pickAndPlace = new MechanicalDemonstrationDefinition(
            "pick-and-place",
            "Coupled pick-and-place",
            "Coordinates all three actuators to lower, translate, and return the moving platform.",
            TimeSpan.FromSeconds(12),
            [
                ActuatorFrame(0, 0, 0, 0),
                ActuatorFrame(2.5, 35, 35, 35),
                ActuatorFrame(5, 65, 25, 45),
                ActuatorFrame(7.5, 25, 65, 45),
                ActuatorFrame(10, 45, 45, 45),
                ActuatorFrame(12, 0, 0, 0)
            ]);
        var individualActuators = new MechanicalDemonstrationDefinition(
            "individual-actuator-inspection",
            "Individual actuator inspection",
            "Moves A, B, and C separately to reveal how one actuator changes the complete platform pose.",
            TimeSpan.FromSeconds(15),
            [
                ActuatorFrame(0, 0, 0, 0),
                ActuatorFrame(2, 55, 0, 0),
                ActuatorFrame(4, 0, 0, 0),
                ActuatorFrame(6.5, 0, 55, 0),
                ActuatorFrame(8.5, 0, 0, 0),
                ActuatorFrame(11, 0, 0, 55),
                ActuatorFrame(13, 0, 0, 0),
                ActuatorFrame(15, 35, 35, 35)
            ]);
        var assemblySequence = new MechanicalDemonstrationDefinition(
            "assembly-sequence",
            "Assembly sequence",
            "Joins the controller, actuator towers, paired links, moving platform, and tool.",
            TimeSpan.FromSeconds(14),
            [
                AssemblyFrame(0),
                AssemblyFrame(2, "controller"),
                AssemblyFrame(5, "controller", "actuator-a", "actuator-b", "actuator-c"),
                AssemblyFrame(9, "controller", "actuator-a", "actuator-b", "actuator-c",
                    "link-a-left", "link-a-right", "link-b-left", "link-b-right", "link-c-left", "link-c-right"),
                AssemblyFrame(12, "controller", "actuator-a", "actuator-b", "actuator-c",
                    "link-a-left", "link-a-right", "link-b-left", "link-b-right", "link-c-left", "link-c-right", "platform"),
                AssemblyFrame(14, "controller", "actuator-a", "actuator-b", "actuator-c",
                    "link-a-left", "link-a-right", "link-b-left", "link-b-right", "link-c-left", "link-c-right", "platform", "tool")
            ]);

        return new MechanicalShowcaseDefinition(model, [pickAndPlace, individualActuators, assemblySequence]);
    }

    internal static IReadOnlyList<MechanicalParallelLinkConstraint> CreateParallelLinkConstraints()
    {
        var constraints = new List<MechanicalParallelLinkConstraint>();
        foreach (var station in Stations)
        {
            AddLinkConstraint(constraints, station, "left", -1);
            AddLinkConstraint(constraints, station, "right", 1);
        }

        return constraints;
    }

    private static void AddLinkConstraint(
        ICollection<MechanicalParallelLinkConstraint> constraints,
        Station station,
        string side,
        float direction)
    {
        var start = new Vector3(station.Radial * CarriageRadiusMillimeters, CarriageZMillimeters) +
                    new Vector3(station.Tangent * (22 * direction), 0);
        var end = new Vector3(station.Radial * PlatformRadiusMillimeters, PlatformZMillimeters) +
                  new Vector3(station.Tangent * (18 * direction), 0);
        constraints.Add(new MechanicalParallelLinkConstraint(
            new RobotPartId($"link-{station.Id}-{side}"),
            new RobotPartId($"carriage-{station.Id}"),
            PlatformId,
            start,
            end));
    }

    private static MechanicalKeyframe ActuatorFrame(
        double seconds,
        float actuatorA,
        float actuatorB,
        float actuatorC)
    {
        var x = (actuatorB - actuatorC) / SqrtThree;
        var y = actuatorA - ((actuatorB + actuatorC) / 2);
        var z = -(actuatorA + actuatorB + actuatorC) / 3;
        return new MechanicalKeyframe(
            TimeSpan.FromSeconds(seconds),
            [
                TranslationPose("carriage-a", new Vector3(0, 0, -actuatorA)),
                TranslationPose("carriage-b", new Vector3(0, 0, -actuatorB)),
                TranslationPose("carriage-c", new Vector3(0, 0, -actuatorC)),
                TranslationPose("platform", new Vector3(x, y, z))
            ]);
    }

    private static RobotComponentPose TranslationPose(string partId, Vector3 translation) =>
        new(new RobotPartId(partId), translation, Quaternion.Identity, Vector3.One);

    private static MechanicalKeyframe AssemblyFrame(double seconds, params string[] joinedPartIds)
    {
        var joined = joinedPartIds.ToHashSet(StringComparer.Ordinal);
        return new MechanicalKeyframe(
            TimeSpan.FromSeconds(seconds),
            DeltaMechanicalTeachingViewCatalog.ExplodedOffsets.Select(offset =>
            {
                var translation = joined.Contains(offset.PartId.Value)
                    ? -offset.TranslationMillimeters
                    : Vector3.Zero;
                return new RobotComponentPose(offset.PartId, translation, Quaternion.Identity, Vector3.One);
            }));
    }

    private static RobotPartDefinition Part(
        string id,
        string name,
        RobotPartKind kind,
        RobotPartId parentId,
        string function,
        string movement) =>
        Part(new RobotPartId(id), name, kind, parentId, function, movement);

    private static RobotPartDefinition Part(
        RobotPartId id,
        string name,
        RobotPartKind kind,
        RobotPartId? parentId,
        string function,
        string movement) =>
        new(id, name, kind, parentId, function, movement);

    private static IReadOnlyList<Station> Stations { get; } =
    [
        new("a", "A", new Vector2(0, 1), new Vector2(-1, 0)),
        new("b", "B", new Vector2(-0.8660254f, -0.5f), new Vector2(0.5f, -0.8660254f)),
        new("c", "C", new Vector2(0.8660254f, -0.5f), new Vector2(0.5f, 0.8660254f))
    ];

    private sealed record Station(string Id, string Label, Vector2 Radial, Vector2 Tangent);
}
