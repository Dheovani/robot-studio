using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public static class ScaraLinkCollisionDetector
{
    public const double DefaultMaximumJointStepDegrees = 1;

    private const double IntersectionTolerance = 0.000_000_001;

    public static ScaraPathCollision? FindFirstCollision(
        ScaraJointPosition start,
        ScaraJointPosition end,
        ScaraRobotProfile profile,
        PlanarSimulationEnvironment environment,
        double maximumJointStepDegrees = DefaultMaximumJointStepDegrees)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(environment);

        if (!double.IsFinite(maximumJointStepDegrees) || maximumJointStepDegrees <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumJointStepDegrees),
                "Maximum collision sampling step must be a finite number greater than zero.");
        }

        profile.ValidatePosition(start);
        profile.ValidatePosition(end);

        var sampleCount = Math.Max(
            1,
            (int)Math.Ceiling(start.MaximumJointDeltaTo(end) / maximumJointStepDegrees));

        for (var index = 0; index <= sampleCount; index++)
        {
            var fraction = (double)index / sampleCount;
            var joints = ScaraJointInterpolation.Interpolate(start, end, fraction);
            var collision = FindConfigurationCollision(joints, profile, environment, fraction);
            if (collision is not null)
            {
                return collision;
            }
        }

        return null;
    }

    private static ScaraPathCollision? FindConfigurationCollision(
        ScaraJointPosition joints,
        ScaraRobotProfile profile,
        PlanarSimulationEnvironment environment,
        double trajectoryFraction)
    {
        var shoulderRadians = DegreesToRadians(joints.ShoulderDegrees);
        var secondLinkRadians = DegreesToRadians(joints.ShoulderDegrees + joints.ElbowDegrees);
        var elbowX = profile.FirstLinkLengthMillimeters * Math.Cos(shoulderRadians);
        var elbowY = profile.FirstLinkLengthMillimeters * Math.Sin(shoulderRadians);
        var toolX = elbowX + (profile.SecondLinkLengthMillimeters * Math.Cos(secondLinkRadians));
        var toolY = elbowY + (profile.SecondLinkLengthMillimeters * Math.Sin(secondLinkRadians));

        foreach (var obstacle in environment.Obstacles)
        {
            if (CapsuleIntersectsObstacle(
                startX: 0,
                startY: 0,
                endX: elbowX,
                endY: elbowY,
                profile.LinkCollisionRadiusMillimeters,
                obstacle))
            {
                return new ScaraPathCollision(
                    obstacle,
                    ScaraLinkId.FirstLink,
                    joints,
                    trajectoryFraction);
            }

            if (CapsuleIntersectsObstacle(
                elbowX,
                elbowY,
                toolX,
                toolY,
                profile.LinkCollisionRadiusMillimeters,
                obstacle))
            {
                return new ScaraPathCollision(
                    obstacle,
                    ScaraLinkId.SecondLink,
                    joints,
                    trajectoryFraction);
            }
        }

        return null;
    }

    private static bool CapsuleIntersectsObstacle(
        double startX,
        double startY,
        double endX,
        double endY,
        double radius,
        PlanarObstacle obstacle)
    {
        if (SegmentIntersectsObstacle(startX, startY, endX, endY, obstacle))
        {
            return true;
        }

        var radiusSquared = radius * radius;
        if (DistanceSquaredToObstacle(startX, startY, obstacle) <= radiusSquared + IntersectionTolerance ||
            DistanceSquaredToObstacle(endX, endY, obstacle) <= radiusSquared + IntersectionTolerance)
        {
            return true;
        }

        return
            DistanceSquaredToSegment(obstacle.MinimumXMillimeters, obstacle.MinimumYMillimeters, startX, startY, endX, endY) <= radiusSquared + IntersectionTolerance ||
            DistanceSquaredToSegment(obstacle.MinimumXMillimeters, obstacle.MaximumYMillimeters, startX, startY, endX, endY) <= radiusSquared + IntersectionTolerance ||
            DistanceSquaredToSegment(obstacle.MaximumXMillimeters, obstacle.MinimumYMillimeters, startX, startY, endX, endY) <= radiusSquared + IntersectionTolerance ||
            DistanceSquaredToSegment(obstacle.MaximumXMillimeters, obstacle.MaximumYMillimeters, startX, startY, endX, endY) <= radiusSquared + IntersectionTolerance;
    }

    private static bool SegmentIntersectsObstacle(
        double startX,
        double startY,
        double endX,
        double endY,
        PlanarObstacle obstacle)
    {
        var entry = 0d;
        var exit = 1d;

        return IntersectsAxis(startX, endX, obstacle.MinimumXMillimeters, obstacle.MaximumXMillimeters, ref entry, ref exit) &&
               IntersectsAxis(startY, endY, obstacle.MinimumYMillimeters, obstacle.MaximumYMillimeters, ref entry, ref exit);
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
        if (Math.Abs(displacement) <= IntersectionTolerance)
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

    private static double DistanceSquaredToSegment(
        double pointX,
        double pointY,
        double startX,
        double startY,
        double endX,
        double endY)
    {
        var deltaX = endX - startX;
        var deltaY = endY - startY;
        var lengthSquared = (deltaX * deltaX) + (deltaY * deltaY);
        if (lengthSquared <= IntersectionTolerance)
        {
            var pointDeltaX = pointX - startX;
            var pointDeltaY = pointY - startY;
            return (pointDeltaX * pointDeltaX) + (pointDeltaY * pointDeltaY);
        }

        var projection = Math.Clamp(
            (((pointX - startX) * deltaX) + ((pointY - startY) * deltaY)) / lengthSquared,
            0,
            1);
        var closestX = startX + (deltaX * projection);
        var closestY = startY + (deltaY * projection);
        var distanceX = pointX - closestX;
        var distanceY = pointY - closestY;
        return (distanceX * distanceX) + (distanceY * distanceY);
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180;
}
