using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class ScaraMechanicalShowcaseDefinition
{
    private const float ElbowPivotMillimeters = 325;

    private static readonly RobotPartId BaseId = new("base");
    private static readonly RobotPartId FirstLinkId = new("first-link");
    private static readonly RobotPartId ElbowJointId = new("elbow-joint");
    private static readonly RobotPartId ZActuatorId = new("z-actuator");

    public static MechanicalShowcasePresentation CreatePresentation()
    {
        var showcase = Create();
        return new MechanicalShowcasePresentation(
            showcase.Model.Id,
            "SCARA Robot",
            "Selective-compliance assembly arm with planar joints and a vertical tool axis",
            "ScaraMechanical",
            showcase,
            new RobotPartId("tool"),
            ScaraMechanicalTeachingViewCatalog.Options,
            ScaraMechanicalTeachingViewCatalog.MotionAxes,
            ScaraMechanicalTeachingViewCatalog.ExplodedOffsets,
            ScaraMechanicalFallbackScene.Create(),
            [new MechanicalRevoluteJointPivot(ElbowJointId, new Vector3(ElbowPivotMillimeters, 0, 0))]);
    }

    public static MechanicalShowcaseDefinition Create()
    {
        var firstLinkCoverId = new RobotPartId("first-link-cover");
        var secondLinkId = new RobotPartId("second-link");
        var model = new RobotVisualModelDefinition(
            "scara-mechanical",
            "Selective-Compliance Assembly Robot Arm",
            BaseId,
            [
                Part(BaseId, "Pedestal and mounting flange", RobotPartKind.Base, null,
                    "Provides a rigid vertical reference and floor mounting for the complete arm.",
                    "Remains fixed while transmitting reaction loads into the workcell."),
                Part("controller", "Embedded motion controller", RobotPartKind.Controller, BaseId,
                    "Synchronizes both rotary servos and the vertical actuator from robot commands.",
                    "Remains fixed inside the pedestal while calculating joint setpoints."),
                Part("shoulder-motor", "Shoulder servo motor", RobotPartKind.Motor, BaseId,
                    "Produces torque for the first planar revolute joint.",
                    "Rotates the first link and every downstream component around the pedestal axis."),
                Part("shoulder-transmission", "Shoulder reduction", RobotPartKind.Transmission, BaseId,
                    "Reduces motor speed and increases torque before it reaches the first link.",
                    "Turns concentrically with the shoulder output shaft."),
                Part(FirstLinkId, "First structural link", RobotPartKind.Link, BaseId,
                    "Carries the elbow assembly at a fixed distance from the shoulder axis.",
                    "Rotates in the horizontal plane around the shoulder joint."),
                Part(firstLinkCoverId, "First-link cover", RobotPartKind.Structure, FirstLinkId,
                    "Protects the first-link structure and routes service connections toward the elbow.",
                    "Moves rigidly with the first link."),
                Part(ElbowJointId, "Elbow joint", RobotPartKind.Joint, FirstLinkId,
                    "Provides the second planar degree of freedom at the end of the first link.",
                    "Rotates relative to the first link around its vertical axis."),
                Part("elbow-motor", "Elbow servo motor", RobotPartKind.Motor, ElbowJointId,
                    "Produces the relative rotation between the first and second links.",
                    "Turns with the elbow joint while the complete assembly follows shoulder motion."),
                Part(secondLinkId, "Second structural link", RobotPartKind.Link, ElbowJointId,
                    "Carries the vertical wrist from the elbow to the tool position.",
                    "Rotates with the elbow and inherits shoulder rotation."),
                Part("second-link-cover", "Second-link cover", RobotPartKind.Structure, secondLinkId,
                    "Protects the second-link structure and the wrist service routing.",
                    "Moves rigidly with the second link."),
                Part("z-motor", "Vertical-axis motor", RobotPartKind.Motor, secondLinkId,
                    "Drives the short vertical positioning stroke at the wrist.",
                    "Remains attached to the second link while turning the vertical transmission."),
                Part(ZActuatorId, "Vertical spindle", RobotPartKind.Actuator, secondLinkId,
                    "Raises and lowers the tool while the planar joints determine X/Y position.",
                    "Translates along the local vertical Z axis."),
                Part("tool", "Parallel gripper", RobotPartKind.Tool, ZActuatorId,
                    "Provides a simple end effector for pick-and-place demonstrations.",
                    "Follows both revolute joints and the vertical spindle stroke.")
            ]);

        var pickAndPlace = new MechanicalDemonstrationDefinition(
            "pick-and-place-cycle",
            "Pick-and-place cycle",
            "Moves between two planar stations and uses the vertical spindle to approach, pick, lift, and place.",
            TimeSpan.FromSeconds(12),
            [
                JointFrame(0, 0, 0, 0),
                JointFrame(2, 38, -72, 0),
                JointFrame(3, 38, -72, -95),
                JointFrame(4, 38, -72, 0),
                JointFrame(7, -32, 62, 0),
                JointFrame(8, -32, 62, -95),
                JointFrame(9, -32, 62, 0),
                JointFrame(12, 0, 0, 0)
            ]);

        var individualJoints = new MechanicalDemonstrationDefinition(
            "individual-joint-inspection",
            "Individual joint inspection",
            "Moves shoulder, elbow, and vertical spindle separately so their parent-child effects remain visible.",
            TimeSpan.FromSeconds(12),
            [
                JointFrame(0, 0, 0, 0),
                JointFrame(2, 42, 0, 0),
                JointFrame(4, 0, 0, 0),
                JointFrame(6, 0, -68, 0),
                JointFrame(8, 0, 0, 0),
                JointFrame(10, 0, 0, -110),
                JointFrame(12, 0, 0, 0)
            ]);

        var assemblySequence = new MechanicalDemonstrationDefinition(
            "assembly-sequence",
            "Assembly sequence",
            "Joins the controller, shoulder drive, links, elbow drive, vertical spindle, and gripper in order.",
            TimeSpan.FromSeconds(12),
            [
                AssemblyFrame(0),
                AssemblyFrame(1.5, "controller"),
                AssemblyFrame(3, "controller", "shoulder-motor", "shoulder-transmission"),
                AssemblyFrame(5, "controller", "shoulder-motor", "shoulder-transmission", "first-link"),
                AssemblyFrame(7, "controller", "shoulder-motor", "shoulder-transmission", "first-link", "elbow-joint"),
                AssemblyFrame(9, "controller", "shoulder-motor", "shoulder-transmission", "first-link", "elbow-joint", "second-link"),
                AssemblyFrame(10.5, "controller", "shoulder-motor", "shoulder-transmission", "first-link", "elbow-joint", "second-link", "z-actuator"),
                AssemblyFrame(12, "controller", "shoulder-motor", "shoulder-transmission", "first-link", "elbow-joint", "second-link", "z-actuator", "tool")
            ]);

        return new MechanicalShowcaseDefinition(model, [pickAndPlace, individualJoints, assemblySequence]);
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

    private static MechanicalKeyframe JointFrame(
        double seconds,
        float shoulderDegrees,
        float elbowDegrees,
        float zMillimeters)
    {
        var shoulderRotation = AroundZ(shoulderDegrees);
        var elbowRotation = AroundZ(elbowDegrees);

        return new MechanicalKeyframe(
            TimeSpan.FromSeconds(seconds),
            [
                new RobotComponentPose(FirstLinkId, Vector3.Zero, shoulderRotation, Vector3.One),
                new RobotComponentPose(ElbowJointId, Vector3.Zero, elbowRotation, Vector3.One),
                new RobotComponentPose(ZActuatorId, new Vector3(0, 0, zMillimeters), Quaternion.Identity, Vector3.One)
            ]);
    }

    private static Quaternion AroundZ(float degrees) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitZ, degrees * MathF.PI / 180);

    private static MechanicalKeyframe AssemblyFrame(double seconds, params string[] joinedPartIds)
    {
        var joined = joinedPartIds.ToHashSet(StringComparer.Ordinal);
        return new MechanicalKeyframe(
            TimeSpan.FromSeconds(seconds),
            ScaraMechanicalTeachingViewCatalog.ExplodedOffsets.Select(offset =>
            {
                var translation = joined.Contains(offset.PartId.Value)
                    ? -offset.TranslationMillimeters
                    : Vector3.Zero;
                return new RobotComponentPose(offset.PartId, translation, Quaternion.Identity, Vector3.One);
            }));
    }
}
