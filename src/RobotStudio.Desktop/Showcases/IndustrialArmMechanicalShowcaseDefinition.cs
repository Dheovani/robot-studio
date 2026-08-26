using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class IndustrialArmMechanicalShowcaseDefinition
{
    private static readonly RobotPartId BaseId = new("base");
    private static readonly RobotPartId J1Id = new("j1-turntable");
    private static readonly RobotPartId J2Id = new("j2-shoulder");
    private static readonly RobotPartId UpperArmId = new("upper-arm");
    private static readonly RobotPartId J3Id = new("j3-elbow");
    private static readonly RobotPartId ForearmId = new("forearm");
    private static readonly RobotPartId J4Id = new("j4-wrist-roll");
    private static readonly RobotPartId WristRollHousingId = new("wrist-roll-housing");
    private static readonly RobotPartId J5Id = new("j5-wrist-bend");
    private static readonly RobotPartId WristBendHousingId = new("wrist-bend-housing");
    private static readonly RobotPartId J6Id = new("j6-tool-roll");

    public static MechanicalShowcasePresentation CreatePresentation()
    {
        var showcase = Create();
        return new MechanicalShowcasePresentation(
            showcase.Model.Id,
            "6-DOF Industrial Arm",
            "Six-axis serial manipulator with enclosed joint drives, a three-axis wrist, and a parallel gripper",
            "IndustrialArmMechanical",
            showcase,
            new RobotPartId("tool"),
            IndustrialArmMechanicalTeachingViewCatalog.Options,
            IndustrialArmMechanicalTeachingViewCatalog.MotionAxes,
            IndustrialArmMechanicalTeachingViewCatalog.ExplodedOffsets,
            IndustrialArmMechanicalFallbackScene.Create(),
            CreateJointPivots());
    }

    public static MechanicalShowcaseDefinition Create()
    {
        var parts = new List<RobotPartDefinition>
        {
            Part(BaseId, "Floor mounting base", RobotPartKind.Base, null,
                "Anchors the manipulator and distributes dynamic loads into its foundation.",
                "Remains fixed while every serial joint moves above it."),
            Part("controller", "Base controller enclosure", RobotPartKind.Controller, BaseId,
                "Houses local drive electronics and routes power and communication into the rotating column.",
                "Remains fixed beside the base rotation assembly."),
            Part(J1Id, "J1 base turntable", RobotPartKind.Joint, BaseId,
                "Provides the first axis and turns the complete arm around vertical Z.",
                "Rotates every downstream link around the pedestal."),
            Part("j1-motor", "J1 drive motor", RobotPartKind.Motor, J1Id,
                "Produces base rotation torque.",
                "Its output drives the turntable through the base reduction."),
            Part(J2Id, "J2 shoulder joint", RobotPartKind.Joint, J1Id,
                "Raises the load-bearing upper arm against gravity.",
                "Pitches the complete arm chain around a horizontal shaft."),
            Part("j2-motor", "J2 shoulder motor", RobotPartKind.Motor, J2Id,
                "Supplies the high torque required by the shoulder axis.",
                "Rotates with J1 while driving J2 through a reduction stage."),
            Part("j2-reduction", "J2 reduction stage", RobotPartKind.Transmission, J2Id,
                "Increases available shoulder torque and supports the arm load.",
                "Transfers motor rotation concentrically to the upper arm."),
            Part(UpperArmId, "Load-bearing upper arm", RobotPartKind.Link, J2Id,
                "Maintains the structural relationship between shoulder and elbow.",
                "Pitches with J2 and carries every downstream axis."),
            Part("upper-arm-cover", "Upper-arm service cover", RobotPartKind.Structure, UpperArmId,
                "Protects internal structure, cabling, and service routing.",
                "Moves rigidly with the upper arm."),
            Part("upper-service-cable", "Upper-arm service cable", RobotPartKind.Structure, UpperArmId,
                "Routes power, feedback, and tool services from the shoulder to the elbow.",
                "Follows the upper arm while allowing relative elbow movement."),
            Part(J3Id, "J3 elbow joint", RobotPartKind.Joint, UpperArmId,
                "Changes the reach and height of the wrist relative to the shoulder.",
                "Pitches the forearm around the elbow shaft."),
            Part("j3-motor", "J3 elbow motor", RobotPartKind.Motor, J3Id,
                "Produces elbow torque while travelling with the upper arm.",
                "Drives forearm pitch through the elbow reduction."),
            Part(ForearmId, "Forearm link", RobotPartKind.Link, J3Id,
                "Carries the compact wrist from the elbow to the orientation axes.",
                "Pitches with J3 and provides the rotation axis for J4."),
            Part("forearm-cover", "Forearm service cover", RobotPartKind.Structure, ForearmId,
                "Protects wrist-drive transmission and tool-service routing.",
                "Moves rigidly with the forearm."),
            Part("forearm-service-cable", "Forearm service cable", RobotPartKind.Structure, ForearmId,
                "Continues power and signal routing from the elbow toward the wrist.",
                "Follows the forearm independently from the upper-arm cable segment."),
            Part(J4Id, "J4 forearm roll", RobotPartKind.Joint, ForearmId,
                "Begins the orientation wrist by rotating around the forearm centerline.",
                "Rolls J5, J6, and the tool around the longitudinal axis."),
            Part(WristRollHousingId, "J4 wrist housing", RobotPartKind.Structure, J4Id,
                "Supports the first wrist bearing and carries the bend assembly.",
                "Rotates with J4."),
            Part(J5Id, "J5 wrist bend", RobotPartKind.Joint, WristRollHousingId,
                "Tilts the tool axis independently of the main arm posture.",
                "Bends J6 and the tool around a transverse axis."),
            Part(WristBendHousingId, "J5 wrist housing", RobotPartKind.Structure, J5Id,
                "Connects wrist bend to the final tool-roll bearing.",
                "Moves with J5 and carries J6."),
            Part(J6Id, "J6 tool roll", RobotPartKind.Joint, WristBendHousingId,
                "Provides the final orientation axis at the tool flange.",
                "Rotates the end effector around its own centerline."),
            Part("tool", "Parallel industrial gripper", RobotPartKind.Tool, J6Id,
                "Provides two opposing fingers for a recognizable pick-and-place operation.",
                "Follows all six joints while retaining its fixed demonstration opening.")
        };

        var model = new RobotVisualModelDefinition(
            "industrial-arm-mechanical",
            "Six-Axis Industrial Serial Manipulator",
            BaseId,
            parts);

        var coordinatedPick = new MechanicalDemonstrationDefinition(
            "coordinated-pick-tour",
            "Coordinated pick tour",
            "Coordinates the positioning axes and wrist axes through approach, pick, transfer, and return poses.",
            TimeSpan.FromSeconds(14),
            [
                JointFrame(0, 0, 0, 0, 0, 0, 0),
                JointFrame(2.5, -30, -18, 36, 15, 18, 0),
                JointFrame(5, -30, 12, 58, 15, -26, 45),
                JointFrame(7.5, 35, -22, 42, -30, 20, 90),
                JointFrame(10.5, 35, 10, 64, -30, -28, 135),
                JointFrame(12.5, 0, -12, 28, 0, 12, 180),
                JointFrame(14, 0, 0, 0, 0, 0, 0)
            ]);

        var wristOrientation = new MechanicalDemonstrationDefinition(
            "wrist-orientation-tour",
            "Wrist orientation tour",
            "Holds the positioning arm steady while J4, J5, and J6 demonstrate roll, bend, and tool rotation.",
            TimeSpan.FromSeconds(12),
            [
                JointFrame(0, 0, -12, 30, 0, 0, 0),
                JointFrame(2, 0, -12, 30, 90, 0, 0),
                JointFrame(4, 0, -12, 30, 0, 0, 0),
                JointFrame(6, 0, -12, 30, 0, 45, 0),
                JointFrame(8, 0, -12, 30, 0, 0, 0),
                JointFrame(10, 0, -12, 30, 0, 0, 150),
                JointFrame(12, 0, -12, 30, 0, 0, 0)
            ]);

        var assemblySequence = new MechanicalDemonstrationDefinition(
            "assembly-sequence",
            "Assembly sequence",
            "Joins the pedestal, shoulder, structural links, elbow, three-axis wrist, and gripper in serial order.",
            TimeSpan.FromSeconds(16),
            [
                AssemblyFrame(0),
                AssemblyFrame(2, "controller", "j1-turntable"),
                AssemblyFrame(4, "controller", "j1-turntable", "j2-shoulder"),
                AssemblyFrame(7, "controller", "j1-turntable", "j2-shoulder", "upper-arm"),
                AssemblyFrame(9, "controller", "j1-turntable", "j2-shoulder", "upper-arm", "j3-elbow", "forearm"),
                AssemblyFrame(12, "controller", "j1-turntable", "j2-shoulder", "upper-arm", "j3-elbow", "forearm", "j4-wrist-roll", "j5-wrist-bend"),
                AssemblyFrame(14, "controller", "j1-turntable", "j2-shoulder", "upper-arm", "j3-elbow", "forearm", "j4-wrist-roll", "j5-wrist-bend", "j6-tool-roll"),
                AssemblyFrame(16, "controller", "j1-turntable", "j2-shoulder", "upper-arm", "j3-elbow", "forearm", "j4-wrist-roll", "j5-wrist-bend", "j6-tool-roll", "tool")
            ]);

        return new MechanicalShowcaseDefinition(model, [coordinatedPick, wristOrientation, assemblySequence]);
    }

    private static IReadOnlyList<MechanicalRevoluteJointPivot> CreateJointPivots() =>
    [
        new(J1Id, new Vector3(0, 0, 100)),
        new(J2Id, new Vector3(0, 0, 220)),
        new(J3Id, new Vector3(140, 0, 520)),
        new(J4Id, new Vector3(400, 0, 520)),
        new(J5Id, new Vector3(465, 0, 520)),
        new(J6Id, new Vector3(515, 0, 520))
    ];

    private static MechanicalKeyframe JointFrame(
        double seconds,
        float j1,
        float j2,
        float j3,
        float j4,
        float j5,
        float j6) =>
        new(
            TimeSpan.FromSeconds(seconds),
            [
                Pose(J1Id, AroundZ(j1)),
                Pose(J2Id, AroundY(j2)),
                Pose(J3Id, AroundY(j3)),
                Pose(J4Id, AroundX(j4)),
                Pose(J5Id, AroundY(j5)),
                Pose(J6Id, AroundX(j6))
            ]);

    private static RobotComponentPose Pose(RobotPartId partId, Quaternion rotation) =>
        new(partId, Vector3.Zero, rotation, Vector3.One);

    private static Quaternion AroundX(float degrees) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitX, degrees * MathF.PI / 180);

    private static Quaternion AroundY(float degrees) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitY, degrees * MathF.PI / 180);

    private static Quaternion AroundZ(float degrees) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitZ, degrees * MathF.PI / 180);

    private static MechanicalKeyframe AssemblyFrame(double seconds, params string[] joinedPartIds)
    {
        var joined = joinedPartIds.ToHashSet(StringComparer.Ordinal);
        return new MechanicalKeyframe(
            TimeSpan.FromSeconds(seconds),
            IndustrialArmMechanicalTeachingViewCatalog.ExplodedOffsets.Select(offset =>
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
