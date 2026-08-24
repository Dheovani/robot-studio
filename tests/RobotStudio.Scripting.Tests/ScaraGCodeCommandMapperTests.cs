using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting.Tests;

public sealed class ScaraGCodeCommandMapperTests
{
    [Fact]
    public void Compile_WhenLinearMoveIsReachable_ShouldCreateToolSpaceCommand()
    {
        var parser = CreateParser();

        var compilation = parser.Compile(
            "G21\nG90\nG1 X220 Y80 Z0 F4800",
            new RobotScriptParseContext(new ScaraJointPosition(0, 0)));

        var move = Assert.IsType<ScaraLinearMoveCommand>(
            Assert.Single(compilation.Commands.Commands));
        Assert.Equal(new ScaraToolPose(220, 80), move.TargetToolPose);
        Assert.Equal(80, move.RequestedToolVelocityMillimetersPerSecond);
        Assert.Equal(3, compilation.Statements.Count);
    }

    [Fact]
    public void Compile_WhenRelativeMoveFollowsHome_ShouldUseHomeToolPose()
    {
        var commands = CreateParser().Parse(
            "G28\nG91\nG1 X-20 Y20 F3000");

        Assert.IsType<HomeCommand>(commands.Commands[0]);
        var move = Assert.IsType<ScaraLinearMoveCommand>(commands.Commands[1]);
        Assert.Equal(new ScaraToolPose(280, 20), move.TargetToolPose);
    }

    [Fact]
    public void Compile_WhenMoveUsesNonzeroZ_ShouldRejectNonPlanarTarget()
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            CreateParser().Compile("G1 X220 Y80 Z10"));

        Assert.Contains("planar", exception.Message);
        Assert.Contains("Z0", exception.Message);
    }

    [Fact]
    public void Compile_WhenInitialPositionIsNotScaraJoints_ShouldRejectContext()
    {
        var context = new RobotScriptParseContext(new CartesianPosition(0, 0, 0));

        var exception = Assert.Throws<ArgumentException>(() =>
            CreateParser().Compile("G28", context));

        Assert.Contains(nameof(ScaraJointPosition), exception.Message);
    }

    [Fact]
    public void Write_WhenSequenceContainsScaraToolMove_ShouldPreserveToolCoordinatesAndFeed()
    {
        var sequence = new RobotCommandSequence(
        [
            new ScaraLinearMoveCommand(
                new ScaraToolPose(220, 80),
                requestedToolVelocityMillimetersPerSecond: 80)
        ]);

        var gCode = GCodeWriter.Write(sequence);
        var result = CreateParser().Parse(
            gCode,
            new RobotScriptParseContext(new ScaraJointPosition(0, 0)));

        Assert.Equal("G21\r\nG90\r\nG1 X220 Y80 F4800", gCode);
        var move = Assert.IsType<ScaraLinearMoveCommand>(
            Assert.Single(result.Commands));
        Assert.Equal(new ScaraToolPose(220, 80), move.TargetToolPose);
        Assert.Equal(80, move.RequestedToolVelocityMillimetersPerSecond);
    }

    private static GCodeParser CreateParser() =>
        new(new ScaraGCodeCommandMapper(CreateProfile()));

    private static ScaraRobotProfile CreateProfile() =>
        new(
            firstLinkLengthMillimeters: 180,
            secondLinkLengthMillimeters: 120,
            linkCollisionRadiusMillimeters: 12,
            shoulderJoint: new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120, 240),
            elbowJoint: new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100, 200));
}
