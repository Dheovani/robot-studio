using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation;

public static class ScaraJointInterpolation
{
    public static ScaraJointPosition Interpolate(
        ScaraJointPosition start,
        ScaraJointPosition end,
        double progress)
    {
        var clampedProgress = Math.Clamp(progress, 0, 1);
        return new ScaraJointPosition(
            start.ShoulderDegrees + ((end.ShoulderDegrees - start.ShoulderDegrees) * clampedProgress),
            start.ElbowDegrees + ((end.ElbowDegrees - start.ElbowDegrees) * clampedProgress));
    }
}
