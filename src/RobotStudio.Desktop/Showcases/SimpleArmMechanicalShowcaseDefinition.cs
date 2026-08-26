using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class SimpleArmMechanicalShowcaseDefinition
{
    private static readonly RobotPartId BaseId = new("base");
    private static readonly RobotPartId TurntableId = new("turntable");
    private static readonly RobotPartId ShoulderJointId = new("shoulder-joint");
    private static readonly RobotPartId UpperArmId = new("upper-arm");
    private static readonly RobotPartId ElbowJointId = new("elbow-joint");

    private static readonly Vector3 ShoulderPivotMillimeters = new(0, 0, 220);
    private static readonly Vector3 ElbowPivotMillimeters = new(270, 0, 460);

    public static MechanicalShowcasePresentation CreatePresentation()
    {
        var showcase = Create();
        return new MechanicalShowcasePresentation(
            showcase.Model.Id,
            "Simple Articulated Arm",
            "Desktop serial arm with base yaw, shoulder pitch, elbow pitch, and a parallel gripper",
            "SimpleArmMechanical",
            showcase,
            new RobotPartId("tool"),
            SimpleArmMechanicalTeachingViewCatalog.Options,
            SimpleArmMechanicalTeachingViewCatalog.MotionAxes,
            SimpleArmMechanicalTeachingViewCatalog.ExplodedOffsets,
            SimpleArmMechanicalFallbackScene.Create(),
            [
                new MechanicalRevoluteJointPivot(ShoulderJointId, ShoulderPivotMillimeters),
                new MechanicalRevoluteJointPivot(ElbowJointId, ElbowPivotMillimeters)
            ]);
    }

    public static MechanicalShowcaseDefinition Create()
    {
        var forearmId = new RobotPartId("forearm");
        var wristId = new RobotPartId("wrist");
        var model = new RobotVisualModelDefinition(
            "simple-arm-mechanical",
            "Desktop Three-Joint Serial Arm",
            BaseId,
            [
                Part(BaseId, "Mounting base", RobotPartKind.Base, null,
                    "Anchors the arm and carries the control electronics and base drive.",
                    "Remains fixed while transmitting arm loads into the bench."),
                Part("controller", "Embedded controller", RobotPartKind.Controller, BaseId,
                    "Coordinates the three servo axes and tool demonstration.",
                    "Remains fixed inside the base enclosure."),
                Part("base-motor", "Base servo motor", RobotPartKind.Motor, BaseId,
                    "Produces rotation around the arm's vertical axis.",
                    "Turns the complete upper mechanism left or right."),
                Part(TurntableId, "Rotating turntable", RobotPartKind.Joint, BaseId,
                    "Supports the shoulder assembly on the base yaw bearing.",
                    "Rotates around vertical Z and carries every downstream component."),
                Part(ShoulderJointId, "Shoulder joint", RobotPartKind.Joint, TurntableId,
                    "Provides the first elevation axis for the serial arm.",
                    "Pitches the upper arm around a horizontal shaft."),
                Part("shoulder-motor", "Shoulder servo motor", RobotPartKind.Motor, ShoulderJointId,
                    "Produces torque required to raise the arm against gravity.",
                    "Rotates with the turntable while driving shoulder pitch."),
                Part("shoulder-transmission", "Shoulder reduction", RobotPartKind.Transmission, ShoulderJointId,
                    "Increases available shoulder torque through a compact reduction stage.",
                    "Transfers servo rotation to the upper-arm shaft."),
                Part(UpperArmId, "Upper structural arm", RobotPartKind.Link, ShoulderJointId,
                    "Maintains a fixed distance between shoulder and elbow axes.",
                    "Pitches with the shoulder and carries the elbow assembly."),
                Part("upper-arm-cover", "Upper-arm cover", RobotPartKind.Structure, UpperArmId,
                    "Protects the load-bearing upper link and its internal service routing.",
                    "Moves rigidly with the upper arm."),
                Part(ElbowJointId, "Elbow joint", RobotPartKind.Joint, UpperArmId,
                    "Provides relative forearm rotation at the end of the upper arm.",
                    "Pitches around a horizontal axis while inheriting base and shoulder motion."),
                Part("elbow-motor", "Elbow servo motor", RobotPartKind.Motor, ElbowJointId,
                    "Produces forearm rotation relative to the upper arm.",
                    "Travels with the shoulder assembly while driving the elbow."),
                Part("elbow-transmission", "Elbow reduction", RobotPartKind.Transmission, ElbowJointId,
                    "Transfers elbow servo torque to the forearm shaft.",
                    "Turns concentrically with the elbow output."),
                Part(forearmId, "Forearm link", RobotPartKind.Link, ElbowJointId,
                    "Carries the wrist from the elbow to the tool mounting point.",
                    "Pitches with the elbow and inherits upstream rotations."),
                Part("forearm-cover", "Forearm cover", RobotPartKind.Structure, forearmId,
                    "Protects the forearm structure and tool wiring.",
                    "Moves rigidly with the forearm."),
                Part(wristId, "Wrist coupling", RobotPartKind.Joint, forearmId,
                    "Provides a compact mechanical interface between forearm and tool.",
                    "Follows the serial chain while maintaining the curated tool orientation."),
                Part("tool", "Parallel gripper", RobotPartKind.Tool, wristId,
                    "Represents a simple end effector for transfer demonstrations.",
                    "Follows base, shoulder, and elbow movement as the chain endpoint.")
            ]);

        var reachAndTransfer = new MechanicalDemonstrationDefinition(
            "reach-and-transfer",
            "Reach and transfer",
            "Coordinates base yaw, shoulder elevation, and elbow bend between two work positions.",
            TimeSpan.FromSeconds(12),
            [
                JointFrame(0, 0, 0, 0),
                JointFrame(2.5, -35, -18, 42),
                JointFrame(5, -35, 22, -28),
                JointFrame(8, 42, -24, 52),
                JointFrame(10, 42, 18, -24),
                JointFrame(12, 0, 0, 0)
            ]);

        var individualJoints = new MechanicalDemonstrationDefinition(
            "individual-joint-inspection",
            "Individual joint inspection",
            "Moves base, shoulder, and elbow separately and returns each axis before the next phase.",
            TimeSpan.FromSeconds(12),
            [
                JointFrame(0, 0, 0, 0),
                JointFrame(2, 50, 0, 0),
                JointFrame(4, 0, 0, 0),
                JointFrame(6, 0, -35, 0),
                JointFrame(8, 0, 0, 0),
                JointFrame(10, 0, 0, 60),
                JointFrame(12, 0, 0, 0)
            ]);

        var assemblySequence = new MechanicalDemonstrationDefinition(
            "assembly-sequence",
            "Assembly sequence",
            "Joins the controller, base drive, shoulder, upper arm, elbow, forearm, wrist, and gripper.",
            TimeSpan.FromSeconds(13),
            [
                AssemblyFrame(0),
                AssemblyFrame(1.5, "controller"),
                AssemblyFrame(3, "controller", "base-motor", "turntable"),
                AssemblyFrame(5, "controller", "base-motor", "turntable", "shoulder-joint"),
                AssemblyFrame(7, "controller", "base-motor", "turntable", "shoulder-joint", "upper-arm"),
                AssemblyFrame(9, "controller", "base-motor", "turntable", "shoulder-joint", "upper-arm", "elbow-joint"),
                AssemblyFrame(11, "controller", "base-motor", "turntable", "shoulder-joint", "upper-arm", "elbow-joint", "forearm", "wrist"),
                AssemblyFrame(13, "controller", "base-motor", "turntable", "shoulder-joint", "upper-arm", "elbow-joint", "forearm", "wrist", "tool")
            ]);

        return new MechanicalShowcaseDefinition(model, [reachAndTransfer, individualJoints, assemblySequence]);
    }

    private static MechanicalKeyframe JointFrame(
        double seconds,
        float baseDegrees,
        float shoulderDegrees,
        float elbowDegrees) =>
        new(
            TimeSpan.FromSeconds(seconds),
            [
                Pose(TurntableId, AroundZ(baseDegrees)),
                Pose(ShoulderJointId, AroundY(shoulderDegrees)),
                Pose(ElbowJointId, AroundY(elbowDegrees))
            ]);

    private static RobotComponentPose Pose(RobotPartId partId, Quaternion rotation) =>
        new(partId, Vector3.Zero, rotation, Vector3.One);

    private static Quaternion AroundZ(float degrees) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitZ, degrees * MathF.PI / 180);

    private static Quaternion AroundY(float degrees) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitY, degrees * MathF.PI / 180);

    private static MechanicalKeyframe AssemblyFrame(double seconds, params string[] joinedPartIds)
    {
        var joined = joinedPartIds.ToHashSet(StringComparer.Ordinal);
        return new MechanicalKeyframe(
            TimeSpan.FromSeconds(seconds),
            SimpleArmMechanicalTeachingViewCatalog.ExplodedOffsets.Select(offset =>
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
}
