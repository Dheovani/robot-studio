using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class XYPlotterMechanicalShowcaseDefinition
{
    private static readonly RobotPartId XCarriageId = new("x-carriage");
    private static readonly RobotPartId YGantryId = new("y-gantry");

    public static MechanicalShowcasePresentation CreatePresentation()
    {
        var showcase = Create();
        return new MechanicalShowcasePresentation(
            showcase.Model.Id,
            "XY Plotter",
            "Planar drawing mechanism and coordinated X/Y motion",
            "XYPlotterMechanical",
            showcase,
            new RobotPartId("pen"),
            XYPlotterMechanicalTeachingViewCatalog.Options,
            XYPlotterMechanicalTeachingViewCatalog.MotionAxes,
            XYPlotterMechanicalTeachingViewCatalog.ExplodedOffsets,
            XYPlotterMechanicalFallbackScene.Create());
    }

    public static MechanicalShowcaseDefinition Create()
    {
        var baseId = new RobotPartId("base");
        var penLiftId = new RobotPartId("pen-lift");
        var model = new RobotVisualModelDefinition(
            "xy-plotter-mechanical",
            "Two-Axis Pen Plotter",
            baseId,
            [
                Part(baseId, "Plotter base", RobotPartKind.Base, null,
                    "Provides a rigid planar reference for the drawing mechanism.",
                    "Remains stationary while supporting the paper and both linear axes."),
                Part("controller", "Motion controller", RobotPartKind.Controller, baseId,
                    "Coordinates X and Y stepper motion to convert commands into a drawn path.",
                    "Remains fixed beside the drawing bed."),
                Part("paper-bed", "Paper bed", RobotPartKind.Structure, baseId,
                    "Supports and aligns the drawing surface below the pen.",
                    "Remains fixed while the mechanism moves above it."),
                Part("left-y-rail", "Left Y rail", RobotPartKind.Rail, baseId,
                    "Guides the left side of the moving bridge along Y.",
                    "Remains fixed and constrains bridge travel to the Y direction."),
                Part("right-y-rail", "Right Y rail", RobotPartKind.Rail, baseId,
                    "Guides the right side of the moving bridge along Y.",
                    "Remains fixed and keeps the bridge parallel to the paper bed."),
                Part("y-motor", "Y-axis motor", RobotPartKind.Motor, baseId,
                    "Supplies torque for bridge movement along the drawing depth.",
                    "Rotates while the Y transmission converts rotation into linear travel."),
                Part("left-y-belt", "Left Y belt", RobotPartKind.Transmission, baseId,
                    "Transfers Y-axis motor motion to one side of the bridge.",
                    "Circulates along the left rail during Y travel."),
                Part("right-y-belt", "Right Y belt", RobotPartKind.Transmission, baseId,
                    "Keeps the opposite side of the bridge synchronized.",
                    "Circulates with the left belt to prevent bridge skew."),
                Part(YGantryId, "Moving Y bridge", RobotPartKind.Carriage, baseId,
                    "Carries the complete X mechanism across the drawing depth.",
                    "Translates along Y and carries every attached X-axis component."),
                Part("x-rail", "X-axis rail", RobotPartKind.Rail, YGantryId,
                    "Constrains the pen carriage to horizontal X travel.",
                    "Moves with the Y bridge while remaining fixed relative to that bridge."),
                Part("x-belt", "X-axis belt", RobotPartKind.Transmission, YGantryId,
                    "Transfers X motor rotation to the pen carriage.",
                    "Circulates across the moving bridge during X travel."),
                Part("x-motor", "X-axis motor", RobotPartKind.Motor, YGantryId,
                    "Supplies torque for pen movement across the drawing width.",
                    "Travels with the Y bridge and rotates to position the X carriage."),
                Part(XCarriageId, "Pen carriage", RobotPartKind.Carriage, YGantryId,
                    "Positions the pen at the requested X coordinate.",
                    "Translates along X while inheriting Y movement from the bridge."),
                Part(penLiftId, "Pen-lift mechanism", RobotPartKind.Actuator, XCarriageId,
                    "Raises or lowers the pen for drawing and repositioning.",
                    "Moves only through a short local vertical stroke; planar commands remain X/Y."),
                Part("pen", "Drawing pen", RobotPartKind.Tool, penLiftId,
                    "Marks the paper at the tool-center point.",
                    "Follows the combined X/Y carriage position and the local pen-lift stroke.")
            ]);

        var rectangularPath = new MechanicalDemonstrationDefinition(
            "rectangular-path-tour",
            "Rectangular path tour",
            "Coordinates X carriage and Y bridge motion to trace a rectangular drawing path.",
            TimeSpan.FromSeconds(10),
            [
                Frame(0, 0, 0),
                Frame(2, 250, 0),
                Frame(4, 250, 150),
                Frame(6, -150, 150),
                Frame(8, -150, 0),
                Frame(10, 0, 0)
            ]);

        var individualAxes = new MechanicalDemonstrationDefinition(
            "individual-axis-inspection",
            "Individual axis inspection",
            "Moves the Y bridge and X pen carriage separately, returning each axis before the next phase.",
            TimeSpan.FromSeconds(8),
            [
                Frame(0, 0, 0),
                Frame(2, 0, 160),
                Frame(4, 0, 0),
                Frame(6, 260, 0),
                Frame(8, 0, 0)
            ]);

        var assemblySequence = new MechanicalDemonstrationDefinition(
            "assembly-sequence",
            "Assembly sequence",
            "Joins the controller, paper bed, Y bridge, X carriage, and pen mechanism in order.",
            TimeSpan.FromSeconds(9),
            [
                AssemblyFrame(0),
                AssemblyFrame(1.5, "controller"),
                AssemblyFrame(3, "controller", "paper-bed"),
                AssemblyFrame(4.5, "controller", "paper-bed", "y-gantry"),
                AssemblyFrame(6, "controller", "paper-bed", "y-gantry", "x-carriage"),
                AssemblyFrame(7.5, "controller", "paper-bed", "y-gantry", "x-carriage", "pen-lift"),
                AssemblyFrame(9, "controller", "paper-bed", "y-gantry", "x-carriage", "pen-lift")
            ]);

        return new MechanicalShowcaseDefinition(model, [rectangularPath, individualAxes, assemblySequence]);
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

    private static MechanicalKeyframe Frame(double seconds, float xMillimeters, float yMillimeters) =>
        new(
            TimeSpan.FromSeconds(seconds),
            [
                Pose(XCarriageId, xMillimeters, 0),
                Pose(YGantryId, 0, yMillimeters)
            ]);

    private static RobotComponentPose Pose(RobotPartId partId, float xMillimeters, float yMillimeters) =>
        new(
            partId,
            new Vector3(xMillimeters, yMillimeters, 0),
            Quaternion.Identity,
            Vector3.One);

    private static MechanicalKeyframe AssemblyFrame(double seconds, params string[] joinedPartIds)
    {
        var joined = joinedPartIds.ToHashSet(StringComparer.Ordinal);
        return new MechanicalKeyframe(
            TimeSpan.FromSeconds(seconds),
            XYPlotterMechanicalTeachingViewCatalog.ExplodedOffsets.Select(offset =>
            {
                var translation = joined.Contains(offset.PartId.Value)
                    ? -offset.TranslationMillimeters
                    : Vector3.Zero;
                return new RobotComponentPose(offset.PartId, translation, Quaternion.Identity, Vector3.One);
            }));
    }
}
