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

    public static double CalculateSynchronizedProgress(
        TrapezoidalMotionProfile? profile,
        TimeSpan segmentStart,
        TimeSpan segmentEnd,
        TimeSpan current)
    {
        if (profile is null)
        {
            return 0;
        }

        var segmentDuration = segmentEnd - segmentStart;
        var elapsed = current - segmentStart;
        var normalizedTime = elapsed.TotalSeconds / segmentDuration.TotalSeconds;
        var profileTime = TimeSpan.FromTicks((long)(profile.TotalDuration.Ticks * normalizedTime));
        return profile.SampleAt(profileTime).Progress;
    }
}
