namespace RobotStudio.Simulation;

public static class SpatialEnvelopeCollisionDetector
{
    private const double ParallelTolerance = 0.000_000_001;

    public static SpatialCollision? FindFirstSweptEnvelopeCollision(
        SpatialPoint start,
        SpatialPoint end,
        double radiusMillimeters,
        SpatialSimulationEnvironment environment,
        string componentId)
    {
        ValidateRadius(radiusMillimeters);
        ArgumentNullException.ThrowIfNull(environment);

        SpatialCollision? first = null;
        foreach (var obstacle in environment.Obstacles)
        {
            if (!TryFindEntryFraction(start, end, obstacle, radiusMillimeters, out var fraction) ||
                first is not null && fraction >= first.TrajectoryFraction)
            {
                continue;
            }

            first = new SpatialCollision(
                obstacle,
                componentId,
                SpatialPoint.Lerp(start, end, fraction),
                fraction);
        }

        return first;
    }

    public static bool LinkEnvelopeIntersects(
        SpatialPoint start,
        SpatialPoint end,
        double radiusMillimeters,
        SpatialObstacle obstacle)
    {
        ValidateRadius(radiusMillimeters);
        ArgumentNullException.ThrowIfNull(obstacle);
        return TryFindEntryFraction(start, end, obstacle, radiusMillimeters, out _);
    }

    private static bool TryFindEntryFraction(
        SpatialPoint start,
        SpatialPoint end,
        SpatialObstacle obstacle,
        double expansion,
        out double entryFraction)
    {
        var entry = 0d;
        var exit = 1d;
        if (!IntersectsAxis(start.X, end.X, obstacle.Minimum.X - expansion, obstacle.Maximum.X + expansion, ref entry, ref exit) ||
            !IntersectsAxis(start.Y, end.Y, obstacle.Minimum.Y - expansion, obstacle.Maximum.Y + expansion, ref entry, ref exit) ||
            !IntersectsAxis(start.Z, end.Z, obstacle.Minimum.Z - expansion, obstacle.Maximum.Z + expansion, ref entry, ref exit))
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

    private static void ValidateRadius(double radiusMillimeters)
    {
        if (!double.IsFinite(radiusMillimeters) || radiusMillimeters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radiusMillimeters), "Envelope radius must be finite and greater than zero.");
        }
    }
}
