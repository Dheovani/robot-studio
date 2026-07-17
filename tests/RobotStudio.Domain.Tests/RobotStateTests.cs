using RobotStudio.Domain;

namespace RobotStudio.Domain.Tests;

public sealed class RobotStateTests
{
    [Fact]
    public void RobotState_ShouldExposeExecutionStates()
    {
        var states = Enum.GetNames<RobotState>();

        Assert.Contains(nameof(RobotState.Idle), states);
        Assert.Contains(nameof(RobotState.Moving), states);
        Assert.Contains(nameof(RobotState.Homing), states);
        Assert.Contains(nameof(RobotState.Waiting), states);
        Assert.Contains(nameof(RobotState.Completed), states);
        Assert.Contains(nameof(RobotState.Faulted), states);
    }
}
