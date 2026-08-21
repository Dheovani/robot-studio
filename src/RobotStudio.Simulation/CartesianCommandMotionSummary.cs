using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Simulation;

public sealed record CartesianCommandMotionSummary(
    int CommandIndex,
    string CommandName,
    CartesianPosition StartPosition,
    CartesianPosition EndPosition,
    IReadOnlyList<AxisId> InvolvedAxes,
    double DistanceMillimeters,
    double VelocityLimitMillimetersPerSecond,
    double PeakVelocityMillimetersPerSecond,
    double AccelerationMillimetersPerSecondSquared,
    MotionProfileShape ProfileShape,
    TimeSpan AccelerationDuration,
    TimeSpan ConstantVelocityDuration,
    TimeSpan DecelerationDuration,
    TimeSpan TotalDuration,
    double? RequestedVelocityMillimetersPerSecond);
