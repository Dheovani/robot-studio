using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation;

public static class CircularFootprintCollisionDetector
{
    private const double IntersectionTolerance = 0.000_000_001;

    public static PlanarPathCollision? FindFirstCollision(
        DifferentialDrivePose start,
        DifferentialDrivePose end,
        double radiusMillimeters,
        PlanarSimulationEnvironment environment)
    {
        if (!double.IsFinite(radiusMillimeters) || radiusMillimeters <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radiusMillimeters),
                "Collision radius must be a finite number greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(environment);

        PlanarPathCollision? firstCollision = null;

        foreach (var obstacle in environment.Obstacles)
        {
            var fraction = FindEntryFraction(start, end, radiusMillimeters, obstacle);
            if (fraction is null ||
                firstCollision is not null && fraction.Value >= firstCollision.TrajectoryFraction)
            {
                continue;
            }

            var pose = Interpolate(start, end, fraction.Value);
            firstCollision = new PlanarPathCollision(
                obstacle,
                pose,
                Math.Clamp(pose.X, obstacle.MinimumXMillimeters, obstacle.MaximumXMillimeters),
                Math.Clamp(pose.Y, obstacle.MinimumYMillimeters, obstacle.MaximumYMillimeters),
                fraction.Value);
        }

        return firstCollision;
    }

    private static double? FindEntryFraction(
        DifferentialDrivePose start,
        DifferentialDrivePose end,
        double radius,
        PlanarObstacle obstacle)
    {
        if (DistanceSquaredToObstacle(start.X, start.Y, obstacle) <= (radius * radius) + IntersectionTolerance)
        {
            return 0;
        }

        var candidates = new List<double>(8);
        AddVerticalSideCandidate(obstacle.MinimumXMillimeters - radius, start, end, obstacle, candidates);
        AddVerticalSideCandidate(obstacle.MaximumXMillimeters + radius, start, end, obstacle, candidates);
        AddHorizontalSideCandidate(obstacle.MinimumYMillimeters - radius, start, end, obstacle, candidates);
        AddHorizontalSideCandidate(obstacle.MaximumYMillimeters + radius, start, end, obstacle, candidates);
        AddCircleCandidate(obstacle.MinimumXMillimeters, obstacle.MinimumYMillimeters, radius, start, end, candidates);
        AddCircleCandidate(obstacle.MinimumXMillimeters, obstacle.MaximumYMillimeters, radius, start, end, candidates);
        AddCircleCandidate(obstacle.MaximumXMillimeters, obstacle.MinimumYMillimeters, radius, start, end, candidates);
        AddCircleCandidate(obstacle.MaximumXMillimeters, obstacle.MaximumYMillimeters, radius, start, end, candidates);

        return candidates.Count == 0 ? null : candidates.Min();
    }

    private static void AddVerticalSideCandidate(
        double x,
        DifferentialDrivePose start,
        DifferentialDrivePose end,
        PlanarObstacle obstacle,
        List<double> candidates)
    {
        var deltaX = end.X - start.X;
        if (Math.Abs(deltaX) <= IntersectionTolerance)
        {
            return;
        }

        var fraction = (x - start.X) / deltaX;
        var y = start.Y + ((end.Y - start.Y) * fraction);
        if (IsOnSegment(fraction) &&
            y >= obstacle.MinimumYMillimeters && y <= obstacle.MaximumYMillimeters)
        {
            candidates.Add(Math.Clamp(fraction, 0, 1));
        }
    }

    private static void AddHorizontalSideCandidate(
        double y,
        DifferentialDrivePose start,
        DifferentialDrivePose end,
        PlanarObstacle obstacle,
        List<double> candidates)
    {
        var deltaY = end.Y - start.Y;
        if (Math.Abs(deltaY) <= IntersectionTolerance)
        {
            return;
        }

        var fraction = (y - start.Y) / deltaY;
        var x = start.X + ((end.X - start.X) * fraction);
        if (IsOnSegment(fraction) &&
            x >= obstacle.MinimumXMillimeters && x <= obstacle.MaximumXMillimeters)
        {
            candidates.Add(Math.Clamp(fraction, 0, 1));
        }
    }

    private static void AddCircleCandidate(
        double centerX,
        double centerY,
        double radius,
        DifferentialDrivePose start,
        DifferentialDrivePose end,
        List<double> candidates)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        var a = (deltaX * deltaX) + (deltaY * deltaY);
        if (a <= IntersectionTolerance)
        {
            return;
        }

        var offsetX = start.X - centerX;
        var offsetY = start.Y - centerY;
        var b = 2 * ((offsetX * deltaX) + (offsetY * deltaY));
        var c = (offsetX * offsetX) + (offsetY * offsetY) - (radius * radius);
        var discriminant = (b * b) - (4 * a * c);
        if (discriminant < -IntersectionTolerance)
        {
            return;
        }

        var squareRoot = Math.Sqrt(Math.Max(0, discriminant));
        var first = (-b - squareRoot) / (2 * a);
        if (IsOnSegment(first))
        {
            candidates.Add(Math.Clamp(first, 0, 1));
        }
    }

    private static double DistanceSquaredToObstacle(
        double x,
        double y,
        PlanarObstacle obstacle)
    {
        var closestX = Math.Clamp(x, obstacle.MinimumXMillimeters, obstacle.MaximumXMillimeters);
        var closestY = Math.Clamp(y, obstacle.MinimumYMillimeters, obstacle.MaximumYMillimeters);
        var deltaX = x - closestX;
        var deltaY = y - closestY;
        return (deltaX * deltaX) + (deltaY * deltaY);
    }

    private static bool IsOnSegment(double fraction) =>
        fraction >= -IntersectionTolerance && fraction <= 1 + IntersectionTolerance;

    private static DifferentialDrivePose Interpolate(
        DifferentialDrivePose start,
        DifferentialDrivePose end,
        double fraction) =>
        new(
            start.X + ((end.X - start.X) * fraction),
            start.Y + ((end.Y - start.Y) * fraction),
            start.HeadingDegrees);
}
