using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class DifferentialDriveMechanicalShowcaseDefinition
{
    private static readonly RobotPartId BaseId = new("base");

    public static MechanicalShowcasePresentation CreatePresentation()
    {
        var showcase = Create();
        return new MechanicalShowcasePresentation(
            showcase.Model.Id,
            "Differential Drive Robot",
            "Round service robot with independent wheel drive and odometry sensors",
            "DifferentialDriveMechanical",
            showcase,
            new RobotPartId("front-sensor"),
            DifferentialDriveMechanicalTeachingViewCatalog.Options,
            DifferentialDriveMechanicalTeachingViewCatalog.MotionAxes,
            DifferentialDriveMechanicalTeachingViewCatalog.ExplodedOffsets,
            DifferentialDriveMechanicalFallbackScene.Create());
    }

    public static MechanicalShowcaseDefinition Create()
    {
        var leftMotorId = new RobotPartId("left-motor");
        var rightMotorId = new RobotPartId("right-motor");
        var upperShellId = new RobotPartId("upper-shell");
        var model = new RobotVisualModelDefinition(
            "differential-drive-mechanical",
            "Round Differential-Drive Service Robot",
            BaseId,
            [
                Part(BaseId, "Structural chassis", RobotPartKind.Base, null,
                    "Provides the rigid reference that connects both drive units and the support caster.",
                    "Translates and rotates as one body when the wheels produce motion."),
                Part("upper-shell", "Protective upper shell", RobotPartKind.Structure, BaseId,
                    "Provides the recognizable low-profile enclosure used by domestic service robots.",
                    "Remains fixed to the chassis during normal operation."),
                Part("controller", "Motion controller", RobotPartKind.Controller, BaseId,
                    "Calculates the left and right motor commands required for the requested path.",
                    "Remains fixed while independently commanding both drive channels."),
                Part("battery", "Battery pack", RobotPartKind.PowerSource, BaseId,
                    "Stores electrical energy for the controller, sensors, and motors.",
                    "Remains fixed near the chassis center to keep mass balanced."),
                Part(leftMotorId, "Left gearmotor", RobotPartKind.Motor, BaseId,
                    "Converts electrical power into torque for the left wheel.",
                    "Its speed may differ from the right motor to change robot heading."),
                Part("left-encoder", "Left wheel encoder", RobotPartKind.Sensor, leftMotorId,
                    "Measures left wheel rotation for velocity estimation and odometry.",
                    "Rotates with the motor shaft and reports incremental motion."),
                Part("left-wheel", "Left drive wheel", RobotPartKind.Wheel, leftMotorId,
                    "Transfers left motor torque to the floor through tire traction.",
                    "Rolls independently; equal wheel speeds drive straight and unequal speeds turn."),
                Part(rightMotorId, "Right gearmotor", RobotPartKind.Motor, BaseId,
                    "Converts electrical power into torque for the right wheel.",
                    "Its speed may differ from the left motor to change robot heading."),
                Part("right-encoder", "Right wheel encoder", RobotPartKind.Sensor, rightMotorId,
                    "Measures right wheel rotation for velocity estimation and odometry.",
                    "Rotates with the motor shaft and reports incremental motion."),
                Part("right-wheel", "Right drive wheel", RobotPartKind.Wheel, rightMotorId,
                    "Transfers right motor torque to the floor through tire traction.",
                    "Rolls independently and forms the second side of the differential drive."),
                Part("caster", "Support caster", RobotPartKind.Wheel, BaseId,
                    "Supports the third contact point without constraining planar steering.",
                    "Swivels passively to follow the direction imposed by the drive wheels."),
                Part("front-sensor", "Front range sensor", RobotPartKind.Sensor, upperShellId,
                    "Measures distance to obstacles in front of the service robot.",
                    "Moves with the chassis and observes along the robot forward direction."),
                Part("bumper", "Front bumper", RobotPartKind.Structure, BaseId,
                    "Forms a protective perimeter around the mobile chassis.",
                    "Moves with the chassis and can provide a future contact-sensing surface.")
            ]);

        var driveTour = new MechanicalDemonstrationDefinition(
            "drive-and-turn-tour",
            "Drive and turn tour",
            "Follows a square route to connect forward travel, heading changes, and the robot body frame.",
            TimeSpan.FromSeconds(16),
            [
                DriveFrame(0, 0, 0, 0),
                DriveFrame(2.5, 300, 0, 0),
                DriveFrame(4, 300, 0, 90),
                DriveFrame(6.5, 300, 300, 90),
                DriveFrame(8, 300, 300, 180),
                DriveFrame(10.5, 0, 300, 180),
                DriveFrame(12, 0, 300, 270),
                DriveFrame(14.5, 0, 0, 270),
                DriveFrame(16, 0, 0, 360)
            ]);

        var turningComparison = new MechanicalDemonstrationDefinition(
            "turning-comparison",
            "Turning comparison",
            "Compares left and right in-place rotation produced by opposite wheel velocities.",
            TimeSpan.FromSeconds(8),
            [
                DriveFrame(0, 0, 0, 0),
                DriveFrame(2, 0, 0, 90),
                DriveFrame(4, 0, 0, 0),
                DriveFrame(6, 0, 0, -90),
                DriveFrame(8, 0, 0, 0)
            ]);

        var assemblySequence = new MechanicalDemonstrationDefinition(
            "assembly-sequence",
            "Assembly sequence",
            "Joins the bumper, caster, battery, controller, drive units, and protective shell to the chassis.",
            TimeSpan.FromSeconds(11),
            [
                AssemblyFrame(0),
                AssemblyFrame(1, "bumper"),
                AssemblyFrame(2.5, "bumper", "caster"),
                AssemblyFrame(4, "bumper", "caster", "battery"),
                AssemblyFrame(5.5, "bumper", "caster", "battery", "controller"),
                AssemblyFrame(7.5, "bumper", "caster", "battery", "controller", "left-motor", "right-motor"),
                AssemblyFrame(9.5, "bumper", "caster", "battery", "controller", "left-motor", "right-motor", "upper-shell"),
                AssemblyFrame(11, "bumper", "caster", "battery", "controller", "left-motor", "right-motor", "upper-shell")
            ]);

        return new MechanicalShowcaseDefinition(model, [driveTour, turningComparison, assemblySequence]);
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

    private static MechanicalKeyframe DriveFrame(
        double seconds,
        float xMillimeters,
        float yMillimeters,
        float headingDegrees) =>
        new(
            TimeSpan.FromSeconds(seconds),
            [
                new RobotComponentPose(
                    BaseId,
                    new Vector3(xMillimeters, yMillimeters, 0),
                    Quaternion.CreateFromAxisAngle(Vector3.UnitZ, headingDegrees * MathF.PI / 180),
                    Vector3.One)
            ]);

    private static MechanicalKeyframe AssemblyFrame(double seconds, params string[] joinedPartIds)
    {
        var joined = joinedPartIds.ToHashSet(StringComparer.Ordinal);
        return new MechanicalKeyframe(
            TimeSpan.FromSeconds(seconds),
            DifferentialDriveMechanicalTeachingViewCatalog.ExplodedOffsets.Select(offset =>
            {
                var translation = joined.Contains(offset.PartId.Value)
                    ? -offset.TranslationMillimeters
                    : Vector3.Zero;
                return new RobotComponentPose(offset.PartId, translation, Quaternion.Identity, Vector3.One);
            }));
    }
}
