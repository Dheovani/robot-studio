namespace RobotStudio.Simulation;

public readonly record struct SpatialPoint(double X, double Y, double Z)
{
    public static SpatialPoint Lerp(SpatialPoint start, SpatialPoint end, double fraction) =>
        new(
            start.X + ((end.X - start.X) * fraction),
            start.Y + ((end.Y - start.Y) * fraction),
            start.Z + ((end.Z - start.Z) * fraction));
}
