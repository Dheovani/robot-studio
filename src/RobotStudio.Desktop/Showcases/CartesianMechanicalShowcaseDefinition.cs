using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class CartesianMechanicalShowcaseDefinition
{
    public static MechanicalShowcaseDefinition Create()
    {
        var baseId = new RobotPartId("base");
        var movingBedId = new RobotPartId("y-bed-carriage");
        var zGantryId = new RobotPartId("z-gantry");
        var toolCarriageId = new RobotPartId("x-tool-carriage");

        var model = new RobotVisualModelDefinition(
            "cartesian-intro-mechanical",
            "Desktop Cartesian Machine",
            baseId,
            [
                Part(baseId, "Machine base", RobotPartKind.Base, null,
                    "Provides the rigid reference shared by all three Cartesian axes.",
                    "Remains stationary while supporting the frame and moving bed."),
                Part("controller", "Machine controller", RobotPartKind.Controller, baseId,
                    "Coordinates the three axis motors as one Cartesian mechanism.",
                    "Its enclosure remains fixed at the front of the machine."),
                Part("left-frame-column", "Left frame column", RobotPartKind.Structure, baseId,
                    "Supports the left side of the vertical gantry.",
                    "Remains fixed while the Z gantry moves along it."),
                Part("right-frame-column", "Right frame column", RobotPartKind.Structure, baseId,
                    "Supports the right side of the vertical gantry.",
                    "Remains fixed while the Z gantry moves along it."),
                Part("top-frame-beam", "Top frame beam", RobotPartKind.Structure, baseId,
                    "Keeps both vertical columns aligned and mechanically rigid.",
                    "Remains fixed across the top of the machine."),
                Part("left-y-rail", "Left Y guide rail", RobotPartKind.Rail, baseId,
                    "Guides one side of the work platform along the depth direction.",
                    "Constrains the moving bed to the logical Y axis."),
                Part("right-y-rail", "Right Y guide rail", RobotPartKind.Rail, baseId,
                    "Guides the opposite side of the work platform.",
                    "Works in parallel with the left guide while remaining fixed."),
                Part("y-motor", "Y stepper motor", RobotPartKind.Motor, baseId,
                    "Supplies torque to the moving-bed transmission.",
                    "Its shaft drives the Y belt while the housing remains fixed."),
                Part("y-belt", "Y timing belt", RobotPartKind.Transmission, baseId,
                    "Transfers motor rotation into linear movement of the bed.",
                    "Runs along Y and pulls the bed carriage in either direction."),
                Part(movingBedId, "Y bed carriage", RobotPartKind.Carriage, baseId,
                    "Supports the working platform on the two Y rails.",
                    "Translates independently along the logical Y axis."),
                Part("build-plate", "Work platform", RobotPartKind.Structure, movingBedId,
                    "Provides a recognizable work surface for the Cartesian machine.",
                    "Moves with the Y bed carriage but adds no new degree of freedom."),
                Part("left-z-guide", "Left Z guide", RobotPartKind.Rail, baseId,
                    "Guides the left side of the horizontal gantry vertically.",
                    "Constrains the gantry to the logical Z axis."),
                Part("right-z-guide", "Right Z guide", RobotPartKind.Rail, baseId,
                    "Guides the right side of the horizontal gantry vertically.",
                    "Keeps both sides of the Z movement aligned."),
                Part("left-z-screw", "Left Z lead screw", RobotPartKind.Transmission, baseId,
                    "Converts motor rotation into vertical movement on the left side.",
                    "Rotates in synchronization with the right lead screw."),
                Part("right-z-screw", "Right Z lead screw", RobotPartKind.Transmission, baseId,
                    "Converts motor rotation into vertical movement on the right side.",
                    "Rotates in synchronization with the left lead screw."),
                Part("left-z-motor", "Left Z stepper motor", RobotPartKind.Motor, baseId,
                    "Drives the left lead screw.",
                    "Cooperates with the right motor as part of one logical Z axis."),
                Part("right-z-motor", "Right Z stepper motor", RobotPartKind.Motor, baseId,
                    "Drives the right lead screw.",
                    "Cooperates with the left motor as part of one logical Z axis."),
                Part(zGantryId, "Synchronized Z gantry", RobotPartKind.Carriage, baseId,
                    "Carries the complete X-axis assembly between both columns.",
                    "Both ends translate together along the single logical Z axis."),
                Part("x-rail", "X linear rail", RobotPartKind.Rail, zGantryId,
                    "Guides the tool carriage across the horizontal gantry.",
                    "Moves vertically with Z while constraining local X motion."),
                Part("x-motor", "X stepper motor", RobotPartKind.Motor, zGantryId,
                    "Drives the horizontal tool-carriage transmission.",
                    "Travels with Z while its shaft drives the X belt."),
                Part("x-belt", "X timing belt", RobotPartKind.Transmission, zGantryId,
                    "Transfers motor rotation to the tool carriage.",
                    "Travels with the Z gantry and pulls the carriage along X."),
                Part(toolCarriageId, "X tool carriage", RobotPartKind.Carriage, zGantryId,
                    "Carries the end effector on the horizontal rail.",
                    "Inherits Z movement and translates locally along X."),
                Part("tool", "Generic process tool", RobotPartKind.Tool, toolCarriageId,
                    "Marks where a practical machine would mount an extruder, probe, or another process tool.",
                    "Follows the combined X and Z mechanism above the Y-moving work surface.")
            ]);

        var coordinatedAxisTour = new MechanicalDemonstrationDefinition(
            "coordinated-axis-tour",
            "Practical axis tour",
            "Moves the bed, tool carriage, and synchronized gantry before returning every axis home.",
            TimeSpan.FromSeconds(8),
            [
                Frame(0, toolCarriageId, 0, movingBedId, 0, zGantryId, 0),
                Frame(2, toolCarriageId, 0, movingBedId, 160, zGantryId, 0),
                Frame(4, toolCarriageId, 260, movingBedId, 160, zGantryId, 0),
                Frame(6, toolCarriageId, 260, movingBedId, 160, zGantryId, -120),
                Frame(8, toolCarriageId, 0, movingBedId, 0, zGantryId, 0)
            ]);

        var individualAxisInspection = new MechanicalDemonstrationDefinition(
            "individual-axis-inspection",
            "Individual axis inspection",
            "Moves and returns Y, X, and Z separately so each mechanical relationship can be inspected.",
            TimeSpan.FromSeconds(12),
            [
                Frame(0, toolCarriageId, 0, movingBedId, 0, zGantryId, 0),
                Frame(2, toolCarriageId, 0, movingBedId, 160, zGantryId, 0),
                Frame(4, toolCarriageId, 0, movingBedId, 0, zGantryId, 0),
                Frame(6, toolCarriageId, 260, movingBedId, 0, zGantryId, 0),
                Frame(8, toolCarriageId, 0, movingBedId, 0, zGantryId, 0),
                Frame(10, toolCarriageId, 0, movingBedId, 0, zGantryId, -120),
                Frame(12, toolCarriageId, 0, movingBedId, 0, zGantryId, 0)
            ]);

        return new MechanicalShowcaseDefinition(model, [coordinatedAxisTour, individualAxisInspection]);
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
