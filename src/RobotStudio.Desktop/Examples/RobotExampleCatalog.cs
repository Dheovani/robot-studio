using RobotStudio.Desktop.Robots;

namespace RobotStudio.Desktop.Examples;

public static class RobotExampleCatalog
{
    private static readonly IReadOnlyList<RobotExample> Examples =
    [
        new(
            RobotViewerKind.CartesianThreeDimensional,
            "Cartesian diagonal move",
            "Homes the robot, moves the TCP through X/Y/Z, then waits.",
            """
            HOME
            MOVE X=120 Y=80 Z=40 SPEED=90
            WAIT 500
            """,
            """
            G21
            G90
            G28
            G1 X120 Y80 Z40 F5400
            G4 P500
            """),

        new(
            RobotViewerKind.CartesianThreeDimensional,
            "Cartesian two-axis move",
            "Keeps Z steady while moving the TCP through X and Y.",
            """
            HOME
            MOVE X=80 Y=40 Z=0 SPEED=70
            MOVE X=180 Y=120 Z=0 SPEED=80
            WAIT 300
            """,
            """
            G21
            G90
            G28
            G1 X80 Y40 Z0 F4200
            G91
            G1 X100 Y80 F4800
            G4 P300
            """),

        new(
            RobotViewerKind.CartesianThreeDimensional,
            "Axis limit validation (invalid)",
            "Intentionally requests X=320 mm beyond the 300 mm limit. Validate it to inspect the axis-specific error; simulation is expected to be rejected.",
            """
            HOME
            MOVE X=320 Y=80 Z=40 SPEED=90
            """,
            """
            G21
            G90
            G28
            G1 X320 Y80 Z40 F5400
            """,
            RobotExampleExpectedResult.ValidationError),

        new(
            RobotViewerKind.CartesianThreeDimensional,
            "Requested vs effective speed",
            "Moves X, Y, and Z separately. Requested speeds above the Y and Z limits are capped, making the comparison visible in the charts and explanation panel.",
            """
            HOME
            MOVE X=120 Y=0 Z=0 SPEED=90
            MOVE X=120 Y=130 Z=0 SPEED=140
            MOVE X=120 Y=130 Z=100 SPEED=140
            WAIT 500
            """,
            """
            G21
            G90
            G28
            G1 X120 Y0 Z0 F5400
            G1 X120 Y130 Z0 F8400
            G1 X120 Y130 Z100 F8400
            G4 P500
            """),

        new(
            RobotViewerKind.CartesianThreeDimensional,
            "Jog, wait, and home sequence",
            "Mirrors small X+/Y+/Z+ jog actions, pauses between phases, and returns HOME to teach command sequencing and state transitions.",
            """
            HOME
            WAIT 300
            MOVE X=10 Y=0 Z=0 SPEED=40
            MOVE X=20 Y=0 Z=0 SPEED=40
            MOVE X=20 Y=10 Z=0 SPEED=40
            MOVE X=20 Y=10 Z=10 SPEED=40
            WAIT 700
            HOME
            """,
            """
            G21
            G90
            G28
            G4 P300
            G91
            G1 X10 F2400
            G1 X10 F2400
            G1 Y10 F2400
            G1 Z10 F2400
            G4 P700
            G28
            """),

        new(
            RobotViewerKind.XYPlotterTwoDimensional,
            "Planar plotting path",
            "Moves through two planar points before waiting.",
            """
            HOME
            MOVE X=160 Y=90 Z=0 SPEED=90
            MOVE X=40 Y=140 Z=0 SPEED=70
            WAIT 500
            """),

        new(
            RobotViewerKind.XYPlotterTwoDimensional,
            "Rectangular plotter path",
            "Draws a simple rectangle using only X/Y movement.",
            """
            HOME
            MOVE X=60 Y=40 Z=0 SPEED=80
            MOVE X=220 Y=40 Z=0 SPEED=80
            MOVE X=220 Y=140 Z=0 SPEED=80
            MOVE X=60 Y=140 Z=0 SPEED=80
            MOVE X=60 Y=40 Z=0 SPEED=80
            WAIT 300
            """),

        new(
            RobotViewerKind.DifferentialDriveTwoDimensional,
            "Mobile navigation path",
            "Drives a mobile robot through two poses with different headings.",
            """
            HOME
            DRIVE X=160 Y=80 HEADING=45 LIN=120 ANG=90
            DRIVE X=300 Y=220 HEADING=135 LIN=100 ANG=80
            WAIT 500
            """),

        new(
            RobotViewerKind.DifferentialDriveTwoDimensional,
            "Mobile square route",
            "Shows how a differential drive robot changes heading at each corner.",
            """
            HOME
            DRIVE X=120 Y=60 HEADING=0 LIN=110 ANG=90
            DRIVE X=260 Y=60 HEADING=90 LIN=110 ANG=90
            DRIVE X=260 Y=200 HEADING=180 LIN=110 ANG=90
            DRIVE X=120 Y=200 HEADING=-90 LIN=110 ANG=90
            WAIT 500
            """),

        new(
            RobotViewerKind.ScaraThreeDimensional,
            "SCARA joint motion",
            "Moves shoulder and elbow joints through two reachable poses.",
            """
            HOME
            SCARA SHOULDER=45 ELBOW=30 SPEED=80
            SCARA SHOULDER=80 ELBOW=-40 SPEED=70
            WAIT 500
            """),

        new(
            RobotViewerKind.ScaraThreeDimensional,
            "SCARA elbow reversal",
            "Highlights how the elbow joint changes the same planar mechanism shape.",
            """
            HOME
            SCARA SHOULDER=35 ELBOW=80 SPEED=75
            SCARA SHOULDER=35 ELBOW=-80 SPEED=75
            SCARA SHOULDER=70 ELBOW=20 SPEED=70
            WAIT 400
            """),

        new(
            RobotViewerKind.SimpleArmThreeDimensional,
            "Articulated arm joint motion",
            "Moves base, shoulder, and elbow joints through two poses.",
            """
            HOME
            ARM BASE=45 SHOULDER=30 ELBOW=-20 SPEED=80
            ARM BASE=90 SHOULDER=-40 ELBOW=70 SPEED=70
            WAIT 500
            """),

        new(
            RobotViewerKind.SimpleArmThreeDimensional,
            "Arm reach and fold",
            "Shows the arm extending forward and then folding back with coordinated joints.",
            """
            HOME
            ARM BASE=20 SHOULDER=20 ELBOW=20 SPEED=70
            ARM BASE=65 SHOULDER=50 ELBOW=-60 SPEED=80
            ARM BASE=110 SHOULDER=-20 ELBOW=80 SPEED=65
            WAIT 400
            """),

        new(
            RobotViewerKind.DeltaThreeDimensional,
            "Delta coupled actuator move",
            "Moves the three parallel actuators together so the tool moves through a coupled workspace.",
            """
            HOME
            DELTA A=30 B=60 C=90 SPEED=80
            DELTA A=80 B=40 C=20 SPEED=70
            WAIT 500
            """),

        new(
            RobotViewerKind.DeltaThreeDimensional,
            "Delta vertical and lateral motion",
            "Contrasts equal actuator movement with uneven actuator movement.",
            """
            HOME
            DELTA A=50 B=50 C=50 SPEED=90
            DELTA A=30 B=90 C=50 SPEED=75
            DELTA A=80 B=35 C=95 SPEED=70
            WAIT 400
            """),

        new(
            RobotViewerKind.DroneThreeDimensional,
            "Drone waypoint flight",
            "Moves through two 3D waypoints while coordinating roll, pitch, and yaw.",
            """
            HOME
            DRONE X=120 Y=80 Z=80 ROLL=10 PITCH=-8 YAW=45 SPEED=120 ATTITUDE_SPEED=60 YAW_SPEED=90
            DRONE X=260 Y=180 Z=120 ROLL=-12 PITCH=6 YAW=135 SPEED=110 ATTITUDE_SPEED=55 YAW_SPEED=80
            WAIT 500
            """),

        new(
            RobotViewerKind.DroneThreeDimensional,
            "Drone climb and turn",
            "Shows vertical motion, lateral motion, and full attitude coordination.",
            """
            HOME
            DRONE X=80 Y=60 Z=140 ROLL=8 PITCH=-10 YAW=0 SPEED=100 ATTITUDE_SPEED=50 YAW_SPEED=90
            DRONE X=220 Y=60 Z=180 ROLL=-8 PITCH=5 YAW=90 SPEED=120 ATTITUDE_SPEED=60 YAW_SPEED=60
            DRONE X=220 Y=220 Z=90 ROLL=0 PITCH=0 YAW=180 SPEED=100 ATTITUDE_SPEED=75 YAW_SPEED=75
            WAIT 400
            """),

        new(
            RobotViewerKind.IndustrialArmThreeDimensional,
            "Industrial pick pose",
            "Coordinates all six joints and pauses at a didactic tooling pose.",
            """
            HOME
            ARM6 J1=35 J2=30 J3=-45 J4=60 J5=20 J6=90 SPEED=80
            WAIT 500
            ARM6 J1=-25 J2=45 J3=-30 J4=-45 J5=15 J6=180 SPEED=70
            WAIT 500
            HOME
            """),

        new(
            RobotViewerKind.IndustrialArmThreeDimensional,
            "Wrist orientation study",
            "Keeps the main arm near one region while changing wrist and tool orientation.",
            """
            HOME
            ARM6 J1=20 J2=35 J3=-55 J4=0 J5=30 J6=0 SPEED=75
            ARM6 J1=20 J2=35 J3=-55 J4=90 J5=-20 J6=120 SPEED=65
            ARM6 J1=20 J2=35 J3=-55 J4=-90 J5=20 J6=-120 SPEED=65
            WAIT 400
            """)
    ];

    public static IReadOnlyList<RobotExample> All => Examples;

    public static IReadOnlyList<RobotExample> GetFor(RobotViewerKind viewerKind) =>
        Examples
            .Where(example => example.ViewerKind == viewerKind)
            .ToArray();

    public static RobotExample GetDefaultFor(RobotViewerKind viewerKind) =>
        GetFor(viewerKind).First();
}
