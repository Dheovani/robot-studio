namespace RobotStudio.Simulation;

public sealed record SpatialCollision(
    SpatialObstacle Obstacle,
    string ComponentId,
    SpatialPoint ComponentPosition,
    double TrajectoryFraction);
