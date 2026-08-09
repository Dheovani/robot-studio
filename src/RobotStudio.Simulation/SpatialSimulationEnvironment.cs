namespace RobotStudio.Simulation;

public sealed class SpatialSimulationEnvironment
{
    public static SpatialSimulationEnvironment Empty { get; } = new([]);

    public SpatialSimulationEnvironment(IEnumerable<SpatialObstacle> obstacles)
    {
        ArgumentNullException.ThrowIfNull(obstacles);
        var values = obstacles.ToArray();
        if (values.Any(obstacle => obstacle is null))
        {
            throw new ArgumentException("Environment obstacles cannot contain null entries.", nameof(obstacles));
        }

        var duplicate = values
            .GroupBy(obstacle => obstacle.Id, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;
        if (duplicate is not null)
        {
            throw new ArgumentException($"Obstacle ID '{duplicate}' must be unique.", nameof(obstacles));
        }

        Obstacles = Array.AsReadOnly(values);
    }

    public IReadOnlyList<SpatialObstacle> Obstacles { get; }
}
