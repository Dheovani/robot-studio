using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class CartesianMechanicalShowcaseDefinition
{
    public static MechanicalShowcaseDefinition Create()
    {
        var baseId = new RobotPartId("base");
        var xCarriageId = new RobotPartId("x-carriage");
        var yCarriageId = new RobotPartId("y-carriage");
        var zCarriageId = new RobotPartId("z-carriage");

        var model = new RobotVisualModelDefinition(
            "cartesian-intro-mechanical",
            "Cartesian Robot Mechanical Model",
            baseId,
            [
                Part(baseId, "Machine base", RobotPartKind.Base, null,
                    "Provides a rigid foundation for every linear assembly.",
                    "Remains fixed while reaction forces pass through it."),
                Part("controller", "Motion controller", RobotPartKind.Controller, baseId,
                    "Coordinates motor commands and monitors axis state.",
                    "Does not move with the mechanical axes."),
                Part("x-rail", "X linear rail", RobotPartKind.Rail, baseId,
                    "Guides the first carriage along the longest horizontal direction.",
                    "Constrains the X carriage to one linear degree of freedom."),
                Part("x-motor", "X servo motor", RobotPartKind.Motor, baseId,
                    "Produces torque for the X transmission.",
                    "Its shaft rotates while the motor housing remains fixed."),
                Part(xCarriageId, "X carriage", RobotPartKind.Carriage, baseId,
                    "Carries the complete Y and Z assemblies along the X rail.",
                    "Translates along the X axis."),
                Part("y-rail", "Y linear rail", RobotPartKind.Rail, xCarriageId,
                    "Guides the second carriage across the gantry.",
                    "Travels with X and constrains Y motion to one direction."),
                Part("y-motor", "Y servo motor", RobotPartKind.Motor, xCarriageId,
                    "Drives the Y carriage through its transmission.",
                    "Travels with X while its shaft drives Y motion."),
                Part(yCarriageId, "Y carriage", RobotPartKind.Carriage, xCarriageId,
                    "Carries the vertical assembly along the Y rail.",
                    "Inherits X movement and translates locally along Y."),
                Part("z-column", "Z column", RobotPartKind.Structure, yCarriageId,
                    "Supports and guides the vertical tool assembly.",
                    "Inherits X and Y movement."),
                Part("z-motor", "Z servo motor", RobotPartKind.Motor, yCarriageId,
                    "Raises and lowers the Z carriage.",
                    "Travels with X and Y while driving vertical movement."),
                Part(zCarriageId, "Z carriage", RobotPartKind.Carriage, yCarriageId,
                    "Positions the tool vertically.",
                    "Inherits X and Y movement and translates locally along Z."),
                Part("tool", "End effector", RobotPartKind.Tool, zCarriageId,
                    "Represents the component that interacts with a workpiece.",
                    "Follows the combined X, Y, and Z carriage motion.")
            ]);

        var demonstration = new MechanicalDemonstrationDefinition(
            "coordinated-axis-tour",
            "Coordinated axis tour",
            "Moves each nested carriage in sequence and returns the mechanism home.",
            TimeSpan.FromSeconds(8),
            [
                Frame(0, xCarriageId, 0, yCarriageId, 0, zCarriageId, 0),
                Frame(2, xCarriageId, 220, yCarriageId, 0, zCarriageId, 0),
                Frame(4, xCarriageId, 220, yCarriageId, 140, zCarriageId, 0),
                Frame(6, xCarriageId, 220, yCarriageId, 140, zCarriageId, -120),
                Frame(8, xCarriageId, 0, yCarriageId, 0, zCarriageId, 0)
            ]);

        return new MechanicalShowcaseDefinition(model, [demonstration]);
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

    private static MechanicalKeyframe Frame(
        double seconds,
        RobotPartId xPartId,
        float xMillimeters,
        RobotPartId yPartId,
        float yMillimeters,
        RobotPartId zPartId,
        float zMillimeters) =>
        new(
            TimeSpan.FromSeconds(seconds),
            [
                Pose(xPartId, xMillimeters, 0, 0),
                Pose(yPartId, 0, yMillimeters, 0),
                Pose(zPartId, 0, 0, zMillimeters)
            ]);

    private static RobotComponentPose Pose(
        RobotPartId partId,
        float xMillimeters,
        float yMillimeters,
        float zMillimeters) =>
        new(
            partId,
            new Vector3(xMillimeters, yMillimeters, zMillimeters),
            Quaternion.Identity,
            Vector3.One);
}
