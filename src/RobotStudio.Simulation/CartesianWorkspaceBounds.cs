using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Simulation;

public sealed record CartesianWorkspaceBounds(
    VisualVector3 Minimum,
    VisualVector3 Maximum)
{
    public static CartesianWorkspaceBounds FromProfile(CartesianRobotProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new CartesianWorkspaceBounds(
            new VisualVector3(
                profile.XAxis.MinimumMillimeters,
                profile.YAxis.MinimumMillimeters,
                profile.ZAxis.MinimumMillimeters),
            new VisualVector3(
                profile.XAxis.MaximumMillimeters,
                profile.YAxis.MaximumMillimeters,
                profile.ZAxis.MaximumMillimeters));
    }

    public VisualVector3 Size => new(
        Maximum.XMillimeters - Minimum.XMillimeters,
        Maximum.YMillimeters - Minimum.YMillimeters,
        Maximum.ZMillimeters - Minimum.ZMillimeters);

    public VisualVector3 Center => new(
        (Minimum.XMillimeters + Maximum.XMillimeters) / 2,
        (Minimum.YMillimeters + Maximum.YMillimeters) / 2,
        (Minimum.ZMillimeters + Maximum.ZMillimeters) / 2);

    public bool Contains(VisualVector3 position) =>
        position.XMillimeters >= Minimum.XMillimeters &&
        position.XMillimeters <= Maximum.XMillimeters &&
        position.YMillimeters >= Minimum.YMillimeters &&
        position.YMillimeters <= Maximum.YMillimeters &&
        position.ZMillimeters >= Minimum.ZMillimeters &&
        position.ZMillimeters <= Maximum.ZMillimeters;
}
