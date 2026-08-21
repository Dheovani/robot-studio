using System.Globalization;
using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Desktop.Profiles;

public sealed record CartesianProfileInput(
    string XMinimum,
    string XMaximum,
    string XMaximumVelocity,
    string XMaximumAcceleration,
    string YMinimum,
    string YMaximum,
    string YMaximumVelocity,
    string YMaximumAcceleration,
    string ZMinimum,
    string ZMaximum,
    string ZMaximumVelocity,
    string ZMaximumAcceleration)
{
    public static CartesianProfileInput FromProfile(CartesianRobotProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new CartesianProfileInput(
            Format(profile.XAxis.MinimumMillimeters),
            Format(profile.XAxis.MaximumMillimeters),
            Format(profile.XAxis.MaximumVelocityMillimetersPerSecond),
            Format(profile.XAxis.MaximumAccelerationMillimetersPerSecondSquared),
            Format(profile.YAxis.MinimumMillimeters),
            Format(profile.YAxis.MaximumMillimeters),
            Format(profile.YAxis.MaximumVelocityMillimetersPerSecond),
            Format(profile.YAxis.MaximumAccelerationMillimetersPerSecondSquared),
            Format(profile.ZAxis.MinimumMillimeters),
            Format(profile.ZAxis.MaximumMillimeters),
            Format(profile.ZAxis.MaximumVelocityMillimetersPerSecond),
            Format(profile.ZAxis.MaximumAccelerationMillimetersPerSecondSquared));
    }

    public CartesianRobotProfile CreateProfile() =>
        CartesianRobotProfile.CreateCartesian(
            CreateAxis(AxisId.X, XMinimum, XMaximum, XMaximumVelocity, XMaximumAcceleration),
            CreateAxis(AxisId.Y, YMinimum, YMaximum, YMaximumVelocity, YMaximumAcceleration),
            CreateAxis(AxisId.Z, ZMinimum, ZMaximum, ZMaximumVelocity, ZMaximumAcceleration));

    private static Axis CreateAxis(
        AxisId id,
        string minimum,
        string maximum,
        string maximumVelocity,
        string maximumAcceleration) =>
        new(
            id,
            ParseFinite(minimum, $"{id} minimum"),
            ParseFinite(maximum, $"{id} maximum"),
            ParseFinite(maximumVelocity, $"{id} maximum velocity"),
            ParseFinite(maximumAcceleration, $"{id} maximum acceleration"));

    private static double ParseFinite(string text, string fieldName)
    {
        if (!double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var value) ||
            !double.IsFinite(value))
        {
            throw new ArgumentException(
                $"{fieldName} must be a finite number using '.' as the decimal separator.");
        }

        return value;
    }

    private static string Format(double value) =>
        value.ToString("0.###", CultureInfo.InvariantCulture);
}
