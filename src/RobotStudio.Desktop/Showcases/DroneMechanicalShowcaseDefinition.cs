using System.Numerics;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Showcases;

internal static class DroneMechanicalShowcaseDefinition
{
    private static readonly RobotPartId AirframeId = new("airframe");

    public static MechanicalShowcasePresentation CreatePresentation()
    {
        var showcase = Create();
        return new MechanicalShowcasePresentation(
            showcase.Model.Id,
            "Drone",
            "Technical quadcopter with visible propellers, propulsion units, avionics, battery, camera, and landing gear",
            "DroneMechanical",
            showcase,
            new RobotPartId("propeller-front-left"),
            DroneMechanicalTeachingViewCatalog.Options,
            DroneMechanicalTeachingViewCatalog.MotionAxes,
            DroneMechanicalTeachingViewCatalog.ExplodedOffsets,
            DroneMechanicalFallbackScene.Create(),
            CreatePropellerPivots());
    }

    public static MechanicalShowcaseDefinition Create()
    {
        var parts = new List<RobotPartDefinition>
        {
            Part(AirframeId, "Central airframe", RobotPartKind.Base, null,
                "Provides the rigid reference structure for propulsion, avionics, and payload components.",
                "Translates and rotates as one aircraft body through roll, pitch, and yaw."),
            Part("shell", "Protective body shell", RobotPartKind.Structure, AirframeId,
                "Protects electronics and gives the vehicle a clear forward-facing silhouette.",
                "Moves rigidly with the central airframe."),
            Part("battery", "Lithium battery pack", RobotPartKind.PowerSource, AirframeId,
                "Supplies electrical energy to avionics and all four propulsion units.",
                "Remains secured beneath the center of gravity during flight."),
            Part("flight-controller", "Flight controller", RobotPartKind.Controller, AirframeId,
                "Combines pilot commands and sensor feedback into motor-speed corrections.",
                "Remains fixed to the vibration-isolated central electronics stack."),
            Part("imu", "Inertial measurement unit", RobotPartKind.Sensor, new RobotPartId("flight-controller"),
                "Measures angular rate and acceleration for attitude estimation.",
                "Moves with the controller while sensing roll, pitch, and yaw dynamics."),
            Part("camera", "Forward camera", RobotPartKind.Sensor, AirframeId,
                "Provides a recognizable forward direction and a simple visual payload.",
                "Follows aircraft attitude without an independent gimbal axis."),
            Part("landing-gear", "Landing gear", RobotPartKind.Structure, AirframeId,
                "Keeps the body and camera clear of the ground.",
                "Moves rigidly with the aircraft and contacts the ground only during landing.")
        };

        foreach (var rotor in Rotors)
        {
            var armId = new RobotPartId(rotor.ArmId);
            var motorId = new RobotPartId(rotor.MotorId);
            parts.Add(Part(armId, rotor.ArmName, RobotPartKind.Structure, AirframeId,
                "Transfers motor thrust and reaction torque into the central airframe.",
                "Moves rigidly with the body frame."));
            parts.Add(Part(motorId, rotor.MotorName, RobotPartKind.Motor, armId,
                "Converts electrical power into controlled propeller speed.",
                "Its rotor turns while the stator housing remains fixed to the arm."));
            parts.Add(Part(rotor.PropellerId, rotor.PropellerName, RobotPartKind.Propeller, motorId,
                "Accelerates air to produce lift and attitude-control torque.",
                rotor.Clockwise
                    ? "Rotates clockwise as viewed from above."
                    : "Rotates counterclockwise as viewed from above."));
        }

        var model = new RobotVisualModelDefinition(
            "drone-mechanical",
            "X-Configuration Technical Quadcopter",
            AirframeId,
            parts);

        var flightTour = new MechanicalDemonstrationDefinition(
            "flight-and-attitude-tour",
            "Flight and attitude tour",
            "Takes off, translates, banks, changes heading, and lands while all four propellers remain visible and rotating.",
            TimeSpan.FromSeconds(12),
            [
                FlightFrame(0, Vector3.Zero, 0, 0, 0, 0),
                FlightFrame(1.5, new Vector3(0, 0, 60), 0, 0, 0, 150),
                FlightFrame(3, new Vector3(35, 0, 105), 0, -8, 0, 300),
                FlightFrame(4.5, new Vector3(85, 30, 135), 8, -10, 25, 450),
                FlightFrame(6, new Vector3(85, 80, 150), 8, 0, 60, 600),
                FlightFrame(7.5, new Vector3(30, 110, 125), -8, 10, 100, 750),
                FlightFrame(9, new Vector3(-25, 50, 95), -5, 6, 145, 900),
                FlightFrame(10.5, new Vector3(0, 0, 55), 0, 0, 180, 1050),
                FlightFrame(12, Vector3.Zero, 0, 0, 0, 1200)
            ]);
        var motorPairs = new MechanicalDemonstrationDefinition(
            "motor-pair-inspection",
            "Counter-rotating motor pairs",
            "Runs one diagonal pair and then the other to explain how opposite rotation directions balance reaction torque.",
            TimeSpan.FromSeconds(10),
            [
                MotorPairFrame(0, 0, 0),
                MotorPairFrame(1, 120, 0),
                MotorPairFrame(2, 240, 0),
                MotorPairFrame(3, 360, 0),
                MotorPairFrame(4, 360, 120),
                MotorPairFrame(5, 360, 240),
                MotorPairFrame(6, 360, 360),
                MotorPairFrame(8, 600, 600),
                MotorPairFrame(10, 840, 840)
            ]);
        var assemblySequence = new MechanicalDemonstrationDefinition(
            "assembly-sequence",
            "Assembly sequence",
            "Joins the avionics, power pack, landing gear, four arms, propulsion units, camera, and protective shell.",
            TimeSpan.FromSeconds(14),
            [
                AssemblyFrame(0),
                AssemblyFrame(2, "landing-gear"),
                AssemblyFrame(4, "landing-gear", "battery"),
                AssemblyFrame(6, "landing-gear", "battery", "flight-controller"),
                AssemblyFrame(9, "landing-gear", "battery", "flight-controller",
                    "arm-front-left", "arm-front-right", "arm-rear-left", "arm-rear-right"),
                AssemblyFrame(11, "landing-gear", "battery", "flight-controller",
                    "arm-front-left", "arm-front-right", "arm-rear-left", "arm-rear-right", "camera"),
                AssemblyFrame(14, "landing-gear", "battery", "flight-controller",
                    "arm-front-left", "arm-front-right", "arm-rear-left", "arm-rear-right", "camera", "shell")
            ]);

        return new MechanicalShowcaseDefinition(model, [flightTour, motorPairs, assemblySequence]);
    }

    private static IReadOnlyList<MechanicalRevoluteJointPivot> CreatePropellerPivots() =>
        Rotors.Select(rotor => new MechanicalRevoluteJointPivot(
            new RobotPartId(rotor.PropellerId),
            new Vector3(rotor.Center * 100, 82))).ToArray();

    private static MechanicalKeyframe FlightFrame(
        double seconds,
        Vector3 translation,
        float rollDegrees,
        float pitchDegrees,
        float yawDegrees,
        float propellerDegrees) =>
        new(
            TimeSpan.FromSeconds(seconds),
            CreateFlightPoses(translation, rollDegrees, pitchDegrees, yawDegrees, propellerDegrees));

    private static IReadOnlyList<RobotComponentPose> CreateFlightPoses(
        Vector3 translation,
        float rollDegrees,
        float pitchDegrees,
        float yawDegrees,
        float propellerDegrees)
    {
        var poses = new List<RobotComponentPose>
        {
            new(
                AirframeId,
                translation,
                AroundZ(yawDegrees) * AroundY(pitchDegrees) * AroundX(rollDegrees),
                Vector3.One)
        };
        poses.AddRange(Rotors.Select(rotor => PropellerPose(
            rotor,
            rotor.Clockwise ? -propellerDegrees : propellerDegrees)));
        return poses;
    }

    private static MechanicalKeyframe MotorPairFrame(
        double seconds,
        float firstPairDegrees,
        float secondPairDegrees)
    {
        var poses = new List<RobotComponentPose>
        {
            RobotComponentPose.Identity(AirframeId)
        };
        poses.AddRange(Rotors.Select(rotor =>
        {
            var belongsToFirstPair = rotor.Id is "front-left" or "rear-right";
            var angle = belongsToFirstPair ? firstPairDegrees : secondPairDegrees;
            return PropellerPose(rotor, rotor.Clockwise ? -angle : angle);
        }));
        return new MechanicalKeyframe(TimeSpan.FromSeconds(seconds), poses);
    }

    private static RobotComponentPose PropellerPose(Rotor rotor, float degrees) =>
        new(new RobotPartId(rotor.PropellerId), Vector3.Zero, AroundZ(degrees), Vector3.One);

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
            DroneMechanicalTeachingViewCatalog.ExplodedOffsets.Select(offset =>
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

    private static IReadOnlyList<Rotor> Rotors { get; } =
    [
        new("front-left", "arm-front-left", "Front-left arm", "motor-front-left", "Front-left motor", "propeller-front-left", "Front-left propeller", new Vector2(-2.55f, -2.55f), false),
        new("front-right", "arm-front-right", "Front-right arm", "motor-front-right", "Front-right motor", "propeller-front-right", "Front-right propeller", new Vector2(2.55f, -2.55f), true),
        new("rear-left", "arm-rear-left", "Rear-left arm", "motor-rear-left", "Rear-left motor", "propeller-rear-left", "Rear-left propeller", new Vector2(-2.55f, 2.55f), true),
        new("rear-right", "arm-rear-right", "Rear-right arm", "motor-rear-right", "Rear-right motor", "propeller-rear-right", "Rear-right propeller", new Vector2(2.55f, 2.55f), false)
    ];

    private sealed record Rotor(
        string Id,
        string ArmId,
        string ArmName,
        string MotorId,
        string MotorName,
        string PropellerId,
        string PropellerName,
        Vector2 Center,
        bool Clockwise);
}
