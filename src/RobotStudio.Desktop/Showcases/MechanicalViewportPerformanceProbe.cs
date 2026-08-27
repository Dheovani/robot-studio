namespace RobotStudio.Desktop.Showcases;

internal sealed record MechanicalViewportPerformanceProbe(
    IReadOnlyList<double> FrameIntervalsMilliseconds,
    IReadOnlyList<double> TransformUpdateMilliseconds,
    IReadOnlyList<double> HitTestMilliseconds,
    int SemanticHitCount);
