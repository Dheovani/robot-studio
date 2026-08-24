using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Scripting.Tests;

public sealed class DeltaGCodeCommandMapperTests
{
    [Fact]
    public void Compile_WhenPoseIsReachable_ShouldCreateLinearToolCommand()
    {
        var compilation = CreateParser().Compile(
            "G21\nG90\nG1 X0 Y-45 Z-60 F4200",
            new RobotScriptParseContext(new DeltaActuatorPosition(0, 0, 0)));

        var move = Assert.IsType<DeltaLinearMoveCommand>(
            Assert.Single(compilation.Commands.Commands));
        Assert.Equal(new DeltaToolPose(0, -45, -60), move.TargetToolPose);
        Assert.Equal(70, move.RequestedToolVelocityMillimetersPerSecond);
    }

    [Fact]
    public void Compile_WhenRelativeMoveFollowsHome_ShouldUseHomeToolPose()
    {
        var commands = CreateParser().Parse(
            "G28\nG91\nG1 Y-20 Z-30 F3000");

        Assert.IsType<HomeCommand>(commands.Commands[0]);
        var move = Assert.IsType<DeltaLinearMoveCommand>(commands.Commands[1]);
        Assert.Equal(new DeltaToolPose(0, -20, -30), move.TargetToolPose);
    }

    [Fact]
    public void Compile_WhenMoveContainsOrientation_ShouldRejectUnsupportedWords()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            CreateParser().Compile("G1 X0 Y-45 Z-60 A10"));

        Assert.Contains("does not support A, B, or C", exception.Message);
    }

    [Fact]
    public void Compile_WhenInitialPositionIsNotDeltaActuators_ShouldRejectContext()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateParser().Compile(
                "G28",
                new RobotScriptParseContext(new CartesianPosition(0, 0, 0))));

        Assert.Contains(nameof(DeltaActuatorPosition), exception.Message);
    }

    [Fact]
    public void Write_WhenSequenceContainsDeltaToolMove_ShouldRoundTripPoseAndFeed()
    {
        var sequence = new RobotCommandSequence(
        [
            new DeltaLinearMoveCommand(
                new DeltaToolPose(0, -45, -60),
                requestedToolVelocityMillimetersPerSecond: 70)
        ]);

        var gCode = GCodeWriter.Write(sequence);
        var result = CreateParser().Parse(
            gCode,
            new RobotScriptParseContext(new DeltaActuatorPosition(0, 0, 0)));

        Assert.Equal("G21\r\nG90\r\nG1 X0 Y-45 Z-60 F4200", gCode);
        Assert.IsType<DeltaLinearMoveCommand>(Assert.Single(result.Commands));
    }

    private static GCodeParser CreateParser() =>
        new(new DeltaGCodeCommandMapper(CreateProfile()));

    private static DeltaRobotProfile CreateProfile() =>
        new(
            baseRadiusMillimeters: 140,
            toolZOffsetMillimeters: 0,
            movingComponentCollisionRadiusMillimeters: 14,
            actuatorA: new DeltaActuator(DeltaActuatorId.A, 0, 180, 120, 240),
            actuatorB: new DeltaActuator(DeltaActuatorId.B, 0, 180, 100, 200),
            actuatorC: new DeltaActuator(DeltaActuatorId.C, 0, 180, 90, 180));
}
