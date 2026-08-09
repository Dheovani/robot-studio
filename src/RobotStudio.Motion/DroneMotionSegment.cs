using RobotStudio.Domain.Aerial;

namespace RobotStudio.Motion;

public sealed record DroneMotionSegment(
    DronePose Start,
    DronePose End,
    TrapezoidalMotionProfile? TranslationProfile,
    TrapezoidalMotionProfile? AttitudeProfile,
    TrapezoidalMotionProfile? YawProfile)
{
    public TimeSpan Duration
    {
        get
        {
            var translationDuration = TranslationProfile?.TotalDuration ?? TimeSpan.Zero;
            var attitudeDuration = AttitudeProfile?.TotalDuration ?? TimeSpan.Zero;
            var yawDuration = YawProfile?.TotalDuration ?? TimeSpan.Zero;
            return new[] { translationDuration, attitudeDuration, yawDuration }.Max();
        }
    }

    public double LinearVelocityMillimetersPerSecond =>
        TranslationProfile is null ? 0 : TranslationProfile.Distance / Duration.TotalSeconds;

    public double YawVelocityDegreesPerSecond =>
        YawProfile is null ? 0 : YawProfile.Distance / Duration.TotalSeconds;

    public double AttitudeVelocityDegreesPerSecond =>
        AttitudeProfile is null ? 0 : AttitudeProfile.Distance / Duration.TotalSeconds;
}
