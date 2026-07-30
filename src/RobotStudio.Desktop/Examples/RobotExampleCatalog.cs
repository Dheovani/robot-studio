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
            RobotViewerKind.SimpleArmThreeDimensional,
            "Articulated arm joint motion",
            "Moves base, shoulder, and elbow joints through two poses.",
            """
            HOME
            ARM BASE=45 SHOULDER=30 ELBOW=-20 SPEED=80
            ARM BASE=90 SHOULDER=-40 ELBOW=70 SPEED=70
            WAIT 500
            """)
    ];

    public static IReadOnlyList<RobotExample> All => Examples;

    public static RobotExample GetDefaultFor(RobotViewerKind viewerKind) =>
        Examples.Single(example => example.ViewerKind == viewerKind);
}
