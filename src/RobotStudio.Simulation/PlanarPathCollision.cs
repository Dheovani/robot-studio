using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation;

public sealed record PlanarPathCollision(
    PlanarObstacle Obstacle,
    DifferentialDrivePose RobotPose,
    double ContactXMillimeters,
    double ContactYMillimeters,
    double TrajectoryFraction);
