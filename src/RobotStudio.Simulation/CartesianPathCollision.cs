using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Simulation;

public sealed record CartesianPathCollision(
    CartesianObstacle Obstacle,
    CartesianPosition Position,
    double TrajectoryFraction);
