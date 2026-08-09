using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Simulation;

public static class CartesianPathCollisionDetector
{
    private const double ParallelTolerance = 0.000_000_001;

    public static CartesianPathCollision? FindFirstCollision(
        CartesianPosition start,
        CartesianPosition end,
        CartesianSimulationEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        CartesianPathCollision? firstCollision = null;

        foreach (var obstacle in environment.Obstacles)
        {
            if (!TryFindEntryFraction(start, end, obstacle, out var fraction))
            {
                continue;
            }

            if (firstCollision is not null && fraction >= firstCollision.TrajectoryFraction)
            {
                continue;
            }

            firstCollision = new CartesianPathCollision(
                obstacle,
                Interpolate(start, end, fraction),
                fraction);
        }

        return firstCollision;
    }

    private static bool TryFindEntryFraction(
        CartesianPosition start,
        CartesianPosition end,
        CartesianObstacle obstacle,
        out double entryFraction)
    {
        var entry = 0d;
        var exit = 1d;

        if (!IntersectsAxis(start.X, end.X, obstacle.Minimum.X, obstacle.Maximum.X, ref entry, ref exit) ||
            !IntersectsAxis(start.Y, end.Y, obstacle.Minimum.Y, obstacle.Maximum.Y, ref entry, ref exit) ||
            !IntersectsAxis(start.Z, end.Z, obstacle.Minimum.Z, obstacle.Maximum.Z, ref entry, ref exit))
        {
            entryFraction = default;
            return false;
        }

        entryFraction = entry;
        return true;
    }

    private static bool IntersectsAxis(
        double start,
        double end,
        double minimum,
        double maximum,
        ref double entry,
        ref double exit)
    {
        var displacement = end - start;
        if (Math.Abs(displacement) <= ParallelTolerance)
        {
            return start >= minimum && start <= maximum;
        }

        var first = (minimum - start) / displacement;
        var second = (maximum - start) / displacement;
        if (first > second)
        {
            (first, second) = (second, first);
        }

        entry = Math.Max(entry, first);
        exit = Math.Min(exit, second);

        return entry <= exit && exit >= 0 && entry <= 1;
    }

    private static CartesianPosition Interpolate(
        CartesianPosition start,
        CartesianPosition end,
        double fraction) =>
        new(
            start.X + ((end.X - start.X) * fraction),
            start.Y + ((end.Y - start.Y) * fraction),
            start.Z + ((end.Z - start.Z) * fraction));
}
