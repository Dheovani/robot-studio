using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting.Tests;

public sealed class SimpleArmGCodeCommandMapperTests
{
    [Fact]
    public void Compile_WhenPoseIsReachable_ShouldCreateLinearToolCommand()
    {
        var compilation = CreateParser().Compile(
            "G21\nG90\nG1 X180 Y80 A20 F3600",
            new RobotScriptParseContext(new SimpleArmJointPosition(0, 0, 0)));

        var move = Assert.IsType<SimpleArmLinearMoveCommand>(
            Assert.Single(compilation.Commands.Commands));
        Assert.Equal(new SimpleArmToolPose(180, 80, 20), move.TargetToolPose);
        Assert.Equal(60, move.RequestedToolVelocityMillimetersPerSecond);
    }

    [Fact]
    public void Compile_WhenRelativeMoveFollowsHome_ShouldUseHomePoseAndOrientation()
    {
        var commands = CreateParser().Parse(
            "G28\nG91\nG1 X-30 Y40 A20 F3000");

        Assert.IsType<HomeCommand>(commands.Commands[0]);
        var move = Assert.IsType<SimpleArmLinearMoveCommand>(commands.Commands[1]);
        Assert.Equal(new SimpleArmToolPose(240, 40, 20), move.TargetToolPose);
    }

    [Theory]
    [InlineData("G1 X180 Y80 Z10 A20", "Z0")]
    [InlineData("G1 X180 Y80 A20 B10", "does not support B or C")]
    [InlineData("G1 X180 Y80 A20 C10", "does not support B or C")]
    public void Compile_WhenPoseUsesUnsupportedDimension_ShouldExplainRestriction(
        string script,
        string expectedMessage)
    {
        var exception = Assert.Throws<ScriptParseException>(() =>
            CreateParser().Compile(script));

        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public void Compile_WhenInitialPositionIsNotSimpleArmJoints_ShouldRejectContext()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateParser().Compile(
                "G28",
                new RobotScriptParseContext(new CartesianPosition(0, 0, 0))));

        Assert.Contains(nameof(SimpleArmJointPosition), exception.Message);
    }

    [Fact]
    public void Write_WhenSequenceContainsSimpleArmToolMove_ShouldRoundTripPoseAndFeed()
    {
        var sequence = new RobotCommandSequence(
        [
            new SimpleArmLinearMoveCommand(
                new SimpleArmToolPose(180, 80, 20),
                requestedToolVelocityMillimetersPerSecond: 60)
        ]);

        var gCode = GCodeWriter.Write(sequence);
        var result = CreateParser().Parse(
            gCode,
            new RobotScriptParseContext(new SimpleArmJointPosition(0, 0, 0)));

        Assert.Equal("G21\r\nG90\r\nG1 X180 Y80 A20 F3600", gCode);
        var move = Assert.IsType<SimpleArmLinearMoveCommand>(Assert.Single(result.Commands));
        Assert.Equal(new SimpleArmToolPose(180, 80, 20), move.TargetToolPose);
    }

    private static GCodeParser CreateParser() =>
        new(new SimpleArmGCodeCommandMapper(CreateProfile()));

    private static SimpleArmRobotProfile CreateProfile() =>
        new(
            firstLinkLengthMillimeters: 120,
            secondLinkLengthMillimeters: 90,
            thirdLinkLengthMillimeters: 60,
            linkCollisionRadiusMillimeters: 10,
            baseJoint: new SimpleArmJoint(SimpleArmJointId.Base, -180, 180, 100, 200),
            shoulderJoint: new SimpleArmJoint(SimpleArmJointId.Shoulder, -120, 120, 90, 180),
            elbowJoint: new SimpleArmJoint(SimpleArmJointId.Elbow, -150, 150, 80, 160));
}
