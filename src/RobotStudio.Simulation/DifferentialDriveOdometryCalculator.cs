using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation;

public static class DifferentialDriveOdometryCalculator
{
    public static DifferentialDriveOdometry Advance(
        DifferentialDriveOdometry current,
        DifferentialDriveProfile profile,
        DifferentialDrivePose start,
        DifferentialDrivePose end)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var translation = start.DistanceTo(end);
        var headingChangeRadians = DegreesToRadians(GetSignedHeadingChange(start, end));
        var halfWheelBase = profile.WheelBaseMillimeters / 2;
        var leftTravel = translation - (headingChangeRadians * halfWheelBase);
        var rightTravel = translation + (headingChangeRadians * halfWheelBase);

        return new DifferentialDriveOdometry(
            current.LeftWheelTravelMillimeters + leftTravel,
            current.RightWheelTravelMillimeters + rightTravel,
            current.LeftWheelRotationDegrees + TravelToWheelRotationDegrees(leftTravel, profile.WheelRadiusMillimeters),
            current.RightWheelRotationDegrees + TravelToWheelRotationDegrees(rightTravel, profile.WheelRadiusMillimeters));
    }

    private static double GetSignedHeadingChange(
        DifferentialDrivePose start,
        DifferentialDrivePose end)
    {
        var delta = ((end.HeadingDegrees - start.HeadingDegrees + 180) % 360 + 360) % 360 - 180;
        return delta == -180 ? 180 : delta;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    private static double TravelToWheelRotationDegrees(double travel, double radius) =>
        travel / radius * 180 / Math.PI;
}
