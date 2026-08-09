namespace RobotStudio.Simulation;

public sealed class CartesianVisualStateMapper
{
    public RobotVisualState Map(SimulationSample sample)
    {
        ArgumentNullException.ThrowIfNull(sample);

        return new RobotVisualState(
            sample.Time,
            sample.State,
            new VisualVector3(
                sample.Position.X,
                sample.Position.Y,
                sample.Position.Z),
            sample.CommandIndex,
            sample.CommandName,
            sample.CommandSource,
            sample.VelocityMillimetersPerSecond,
            sample.AccelerationMillimetersPerSecondSquared,
            sample.MotionProfilePhase);
    }
}
