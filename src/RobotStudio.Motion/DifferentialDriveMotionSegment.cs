using RobotStudio.Domain.Mobile;

namespace RobotStudio.Motion;

public sealed record DifferentialDriveMotionSegment(
    DifferentialDriveMotionKind Kind,
    DifferentialDrivePose Start,
    DifferentialDrivePose End,
    TrapezoidalMotionProfile Profile)
{
    public TimeSpan Duration => Profile.TotalDuration;

    public double LinearVelocityMillimetersPerSecond =>
        Kind == DifferentialDriveMotionKind.Translation ? Profile.PeakVelocity : 0;

    public double AngularVelocityDegreesPerSecond =>
        Kind == DifferentialDriveMotionKind.Rotation ? Profile.PeakVelocity : 0;
}
