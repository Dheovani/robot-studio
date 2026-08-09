using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Simulation;

public sealed record CartesianObstacle
{
    public CartesianObstacle(
        string id,
        CartesianPosition minimum,
        CartesianPosition maximum)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Obstacle ID cannot be empty.", nameof(id));
        }

        if (!CoordinatesAreFinite(minimum) || !CoordinatesAreFinite(maximum))
        {
            throw new ArgumentException("Obstacle coordinates must be finite numbers.", nameof(maximum));
        }

        if (maximum.X <= minimum.X ||
            maximum.Y <= minimum.Y ||
            maximum.Z <= minimum.Z)
        {
            throw new ArgumentException(
                "Obstacle maximum coordinates must be greater than minimum coordinates on every axis.",
                nameof(maximum));
        }

        Id = id.Trim();
        Minimum = minimum;
        Maximum = maximum;
    }

    public string Id { get; }

    public CartesianPosition Minimum { get; }

    public CartesianPosition Maximum { get; }

    public bool Contains(CartesianPosition position) =>
        position.X >= Minimum.X && position.X <= Maximum.X &&
        position.Y >= Minimum.Y && position.Y <= Maximum.Y &&
        position.Z >= Minimum.Z && position.Z <= Maximum.Z;

    private static bool CoordinatesAreFinite(CartesianPosition position) =>
        double.IsFinite(position.X) &&
        double.IsFinite(position.Y) &&
        double.IsFinite(position.Z);
}
