using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public sealed record ScaraPathCollision(
    PlanarObstacle Obstacle,
    ScaraLinkId Link,
    ScaraJointPosition Joints,
    double TrajectoryFraction);
