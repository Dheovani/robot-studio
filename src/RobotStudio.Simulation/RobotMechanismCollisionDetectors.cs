using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Simulation;

public static class SimpleArmLinkCollisionDetector
{
    public static SpatialCollision? FindFirstCollision(
        SimpleArmJointPosition start,
        SimpleArmJointPosition end,
        SimpleArmRobotProfile profile,
        SpatialSimulationEnvironment environment,
        double maximumJointStepDegrees = 1)
    {
        ValidateStep(maximumJointStepDegrees);
        var samples = Math.Max(1, (int)Math.Ceiling(start.MaximumJointDeltaTo(end) / maximumJointStepDegrees));
        for (var index = 0; index <= samples; index++)
        {
            var fraction = (double)index / samples;
            var joints = Interpolate(start, end, fraction);
            var points = GetPoints(profile, joints);
            var collision = FindLinkCollision(points, profile.LinkCollisionRadiusMillimeters, environment, fraction, "Link");
            if (collision is not null)
            {
                return collision;
            }
        }

        return null;
    }

    private static SpatialPoint[] GetPoints(SimpleArmRobotProfile profile, SimpleArmJointPosition joints)
    {
        var first = Degrees(joints.BaseDegrees);
        var second = first + Degrees(joints.ShoulderDegrees);
        var third = second + Degrees(joints.ElbowDegrees);
        var p0 = new SpatialPoint(0, 0, 0);
        var p1 = AddPlanar(p0, profile.FirstLinkLengthMillimeters, first);
        var p2 = AddPlanar(p1, profile.SecondLinkLengthMillimeters, second);
        var p3 = AddPlanar(p2, profile.ThirdLinkLengthMillimeters, third);
        return [p0, p1, p2, p3];
    }

    private static SimpleArmJointPosition Interpolate(SimpleArmJointPosition start, SimpleArmJointPosition end, double fraction) =>
        new(
            Lerp(start.BaseDegrees, end.BaseDegrees, fraction),
            Lerp(start.ShoulderDegrees, end.ShoulderDegrees, fraction),
            Lerp(start.ElbowDegrees, end.ElbowDegrees, fraction));

    internal static SpatialCollision? FindLinkCollision(
        IReadOnlyList<SpatialPoint> points,
        double radius,
        SpatialSimulationEnvironment environment,
        double fraction,
        string prefix)
    {
        for (var link = 0; link < points.Count - 1; link++)
        {
            foreach (var obstacle in environment.Obstacles)
            {
                if (SpatialEnvelopeCollisionDetector.LinkEnvelopeIntersects(points[link], points[link + 1], radius, obstacle))
                {
                    return new SpatialCollision(obstacle, $"{prefix}{link + 1}", points[link + 1], fraction);
                }
            }
        }

        return null;
    }

    internal static void ValidateStep(double step)
    {
        if (!double.IsFinite(step) || step <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(step), "Collision sampling step must be finite and greater than zero.");
        }
    }

    internal static double Lerp(double start, double end, double fraction) => start + ((end - start) * fraction);
    private static double Degrees(double value) => value * Math.PI / 180;
    private static SpatialPoint AddPlanar(SpatialPoint point, double length, double angle) =>
        new(point.X + (length * Math.Cos(angle)), point.Y + (length * Math.Sin(angle)), point.Z);
}

public static class IndustrialArmLinkCollisionDetector
{
    public static SpatialCollision? FindFirstCollision(
        IndustrialArmJointPosition start,
        IndustrialArmJointPosition end,
        IndustrialArmRobotProfile profile,
        SpatialSimulationEnvironment environment,
        double maximumJointStepDegrees = 1)
    {
        SimpleArmLinkCollisionDetector.ValidateStep(maximumJointStepDegrees);
        var samples = Math.Max(1, (int)Math.Ceiling(start.MaximumJointDeltaTo(end) / maximumJointStepDegrees));
        for (var index = 0; index <= samples; index++)
        {
            var fraction = (double)index / samples;
            var joints = Interpolate(start, end, fraction);
            var collision = SimpleArmLinkCollisionDetector.FindLinkCollision(
                GetPoints(profile, joints),
                profile.LinkCollisionRadiusMillimeters,
                environment,
                fraction,
                "IndustrialLink");
            if (collision is not null)
            {
                return collision;
            }
        }

        return null;
    }

    private static SpatialPoint[] GetPoints(IndustrialArmRobotProfile profile, IndustrialArmJointPosition joints)
    {
        var yaw = Degrees(joints.J1Degrees);
        var shoulder = Degrees(joints.J2Degrees);
        var elbow = shoulder + Degrees(joints.J3Degrees);
        var wrist = elbow + Degrees(joints.J5Degrees);
        var origin = new SpatialPoint(0, 0, 0);
        var shoulderPoint = new SpatialPoint(0, 0, profile.BaseHeightMillimeters);
        var elbowPoint = AddSpatial(shoulderPoint, profile.UpperArmLengthMillimeters, yaw, shoulder);
        var wristPoint = AddSpatial(elbowPoint, profile.ForearmLengthMillimeters, yaw, elbow);
        var toolPoint = AddSpatial(wristPoint, profile.WristLengthMillimeters, yaw, wrist);
        return [origin, shoulderPoint, elbowPoint, wristPoint, toolPoint];
    }

    private static IndustrialArmJointPosition Interpolate(
        IndustrialArmJointPosition start,
        IndustrialArmJointPosition end,
        double fraction) =>
        new(
            Lerp(start.J1Degrees, end.J1Degrees, fraction),
            Lerp(start.J2Degrees, end.J2Degrees, fraction),
            Lerp(start.J3Degrees, end.J3Degrees, fraction),
            Lerp(start.J4Degrees, end.J4Degrees, fraction),
            Lerp(start.J5Degrees, end.J5Degrees, fraction),
            Lerp(start.J6Degrees, end.J6Degrees, fraction));

    private static SpatialPoint AddSpatial(SpatialPoint point, double length, double yaw, double pitch)
    {
        var radial = length * Math.Cos(pitch);
        return new SpatialPoint(
            point.X + (radial * Math.Cos(yaw)),
            point.Y + (radial * Math.Sin(yaw)),
            point.Z + (length * Math.Sin(pitch)));
    }

    private static double Lerp(double start, double end, double fraction) =>
        SimpleArmLinkCollisionDetector.Lerp(start, end, fraction);
    private static double Degrees(double value) => value * Math.PI / 180;
}

public static class DeltaMechanismCollisionDetector
{
    private const double SqrtThree = 1.732_050_807_568_877_2;

    public static SpatialCollision? FindFirstCollision(
        DeltaActuatorPosition start,
        DeltaActuatorPosition end,
        DeltaRobotProfile profile,
        SpatialSimulationEnvironment environment,
        double maximumActuatorStepMillimeters = 2)
    {
        SimpleArmLinkCollisionDetector.ValidateStep(maximumActuatorStepMillimeters);
        var samples = Math.Max(1, (int)Math.Ceiling(start.MaximumActuatorDeltaTo(end) / maximumActuatorStepMillimeters));
        for (var index = 0; index <= samples; index++)
        {
            var fraction = (double)index / samples;
            var position = new DeltaActuatorPosition(
                SimpleArmLinkCollisionDetector.Lerp(start.AMillimeters, end.AMillimeters, fraction),
                SimpleArmLinkCollisionDetector.Lerp(start.BMillimeters, end.BMillimeters, fraction),
                SimpleArmLinkCollisionDetector.Lerp(start.CMillimeters, end.CMillimeters, fraction));
            var tool = ToolPoint(profile, position);
            var carriages = CarriagePoints(profile, position);

            var platform = SpatialEnvelopeCollisionDetector.FindFirstSweptEnvelopeCollision(
                tool,
                tool,
                profile.MovingComponentCollisionRadiusMillimeters,
                environment,
                "MovingPlatform");
            if (platform is not null)
            {
                return platform with { TrajectoryFraction = fraction };
            }

            foreach (var obstacle in environment.Obstacles)
            {
                for (var link = 0; link < carriages.Length; link++)
                {
                    if (SpatialEnvelopeCollisionDetector.LinkEnvelopeIntersects(
                        carriages[link], tool, profile.MovingComponentCollisionRadiusMillimeters, obstacle))
                    {
                        return new SpatialCollision(obstacle, $"ParallelLink{link + 1}", tool, fraction);
                    }
                }
            }
        }

        return null;
    }

    private static SpatialPoint ToolPoint(DeltaRobotProfile profile, DeltaActuatorPosition position)
    {
        var average = (position.AMillimeters + position.BMillimeters + position.CMillimeters) / 3;
        return new SpatialPoint(
            (position.BMillimeters - position.CMillimeters) / SqrtThree,
            position.AMillimeters - ((position.BMillimeters + position.CMillimeters) / 2),
            profile.ToolZOffsetMillimeters - average);
    }

    private static SpatialPoint[] CarriagePoints(DeltaRobotProfile profile, DeltaActuatorPosition position) =>
        [
            Carriage(profile, 90, position.AMillimeters),
            Carriage(profile, 210, position.BMillimeters),
            Carriage(profile, 330, position.CMillimeters)
        ];

    private static SpatialPoint Carriage(DeltaRobotProfile profile, double angleDegrees, double actuator)
    {
        var angle = angleDegrees * Math.PI / 180;
        return new SpatialPoint(
            profile.BaseRadiusMillimeters * Math.Cos(angle),
            profile.BaseRadiusMillimeters * Math.Sin(angle),
            profile.ToolZOffsetMillimeters - actuator);
    }
}
