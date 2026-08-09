using RobotStudio.Motion;

namespace RobotStudio.Simulation;

internal static class MotionProfileTimelineSampler
{
    public static double CalculateProgress(
        TrapezoidalMotionProfile? profile,
        TimeSpan segmentStart,
        TimeSpan segmentEnd,
        TimeSpan current)
    {
        if (profile is not null)
        {
            return profile.SampleAt(current - segmentStart).Progress;
        }

        return (current - segmentStart).TotalSeconds /
            (segmentEnd - segmentStart).TotalSeconds;
    }
}
