namespace RobotStudio.Simulation;

public sealed class CartesianSimulationEnvironment
{
    public static CartesianSimulationEnvironment Empty { get; } = new([]);

    public CartesianSimulationEnvironment(IEnumerable<CartesianObstacle> obstacles)
    {
        ArgumentNullException.ThrowIfNull(obstacles);

        var obstacleArray = obstacles.ToArray();
        if (obstacleArray.Any(obstacle => obstacle is null))
        {
            throw new ArgumentException("Environment obstacles cannot contain null entries.", nameof(obstacles));
        }

        var duplicateId = obstacleArray
            .GroupBy(obstacle => obstacle.Id, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicateId is not null)
        {
            throw new ArgumentException($"Obstacle ID '{duplicateId}' must be unique.", nameof(obstacles));
        }

        Obstacles = Array.AsReadOnly(obstacleArray);
    }

    public IReadOnlyList<CartesianObstacle> Obstacles { get; }
}
