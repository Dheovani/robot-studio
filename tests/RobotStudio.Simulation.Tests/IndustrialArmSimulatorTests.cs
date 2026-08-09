using RobotStudio.Domain;
using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Simulation.Tests;

public sealed class IndustrialArmSimulatorTests
{
    [Fact]
    public void Execute_WhenSequenceContainsMoveWaitAndHome_ShouldCompleteAtHome()
    {
        var context = IndustrialArmSimulationContext.Create(CreateProfile(), IndustrialArmJointPosition.Home);
        var sequence = new RobotCommandSequence(
        [
            new IndustrialArmMoveJointsCommand(new IndustrialArmJointPosition(45, 30, -20, 60, 10, 90)),
            new WaitCommand(TimeSpan.FromMilliseconds(500)),
            new HomeCommand()
        ]);

        var result = new IndustrialArmSimulator().Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(IndustrialArmJointPosition.Home, result.FinalContext.CurrentJoints);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.True(result.FinalContext.ElapsedTime > TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void Sample_WhenArmMoves_ShouldInterpolateEveryJointAndPreserveSource()
    {
        var source = new RobotCommandSource(2, "ARM6 J1=60 J2=30 J3=-20 J4=40 J5=10 J6=90");
        var result = new IndustrialArmSimulator().Execute(
            IndustrialArmSimulationContext.Create(CreateProfile(), IndustrialArmJointPosition.Home),
            new RobotCommandSequence(
            [
                new IndustrialArmMoveJointsCommand(
                    new IndustrialArmJointPosition(60, 30, -20, 40, 10, 90),
                    source: source)
            ]));

        var snapshot = new IndustrialArmPlaybackSampler().Sample(result, TimeSpan.FromMilliseconds(100));

        Assert.True(snapshot.FrameCount > 2);
        Assert.Contains(snapshot.Frames, frame => frame.Joints.J6Degrees is > 0 and < 90);
        Assert.Contains(snapshot.Frames, frame => frame.CommandSource?.LineNumber == 2);
        Assert.NotNull(result.Timeline[1].MotionProfile);
        var acceleratingFrame = Assert.Single(snapshot.Frames, frame => frame.Time == TimeSpan.FromMilliseconds(100));
        Assert.True(
            acceleratingFrame.Joints.J6Degrees <
            90 * (acceleratingFrame.Time.TotalSeconds / result.FinalContext.ElapsedTime.TotalSeconds));
    }

    [Fact]
    public void Execute_WhenSecondMoveExceedsJointLimit_ShouldFaultAndPreserveLastValidJoints()
    {
        var validTarget = new IndustrialArmJointPosition(30, 20, -15, 40, 10, 60);
        var sequence = new RobotCommandSequence(
        [
            new IndustrialArmMoveJointsCommand(validTarget),
            new IndustrialArmMoveJointsCommand(new IndustrialArmJointPosition(181, 0, 0, 0, 0, 0))
        ]);

        var result = new IndustrialArmSimulator().Execute(
            IndustrialArmSimulationContext.Create(CreateProfile(), IndustrialArmJointPosition.Home),
            sequence);

        Assert.False(result.Succeeded);
        Assert.Equal(RobotState.Faulted, result.FinalContext.State);
        Assert.Equal(validTarget, result.FinalContext.CurrentJoints);
        Assert.NotNull(result.Failure);
    }

    [Fact]
    public void Sample_WhenIntervalIsNotPositive_ShouldThrow()
    {
        var result = new IndustrialArmSimulator().Execute(
            IndustrialArmSimulationContext.Create(CreateProfile(), IndustrialArmJointPosition.Home),
            new RobotCommandSequence([new HomeCommand()]));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IndustrialArmPlaybackSampler().Sample(result, TimeSpan.Zero));
    }

    private static IndustrialArmRobotProfile CreateProfile() =>
        new(
            100,
            180,
            140,
            80,
            [
                new(IndustrialArmJointId.J1Base, -180, 180, 120, 240),
                new(IndustrialArmJointId.J2Shoulder, -120, 120, 100, 200),
                new(IndustrialArmJointId.J3Elbow, -150, 150, 90, 180),
                new(IndustrialArmJointId.J4WristRoll, -180, 180, 160, 320),
                new(IndustrialArmJointId.J5WristPitch, -120, 120, 110, 220),
                new(IndustrialArmJointId.J6ToolRoll, -360, 360, 200, 400)
            ]);
}
