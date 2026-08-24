using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Scripting.Tests;

public sealed class IndustrialArmGCodeCommandMapperTests
{
    [Fact]
    public void Compile_WhenFullPoseIsReachable_ShouldCreateLinearToolCommand()
    {
        var compilation = CreateParser().Compile(
            "G21\nG90\nG1 X300 Y0 Z180 A20 B10 C0 F4200",
            new RobotScriptParseContext(IndustrialArmJointPosition.Home));

        var move = Assert.IsType<IndustrialArmLinearMoveCommand>(
            Assert.Single(compilation.Commands.Commands));
        Assert.Equal(new IndustrialArmToolPose(300, 0, 180, 20, 10, 0), move.TargetToolPose);
        Assert.Equal(70, move.RequestedToolVelocityMillimetersPerSecond);
    }

    [Fact]
    public void Compile_WhenRelativeMoveFollowsHome_ShouldUseHomeToolPose()
    {
        var commands = CreateParser().Parse(
            "G28\nG91\nG1 X-100 Z80 A20 B10 F3000");

        Assert.IsType<HomeCommand>(commands.Commands[0]);
        var move = Assert.IsType<IndustrialArmLinearMoveCommand>(commands.Commands[1]);
        Assert.Equal(new IndustrialArmToolPose(300, 0, 180, 20, 10, 0), move.TargetToolPose);
    }

    [Fact]
    public void Compile_WhenYawConflictsWithPosition_ShouldRejectPose()
    {
        var exception = Assert.Throws<InvalidRobotCommandException>(() =>
            CreateParser().Compile("G1 X300 Y0 Z180 A0 B0 C20 F3000"));

        Assert.Contains("couples TCP yaw C", exception.Message);
    }

    [Fact]
    public void Compile_WhenInitialPositionIsNotIndustrialArmJoints_ShouldRejectContext()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateParser().Compile(
                "G28",
                new RobotScriptParseContext(new CartesianPosition(0, 0, 0))));

        Assert.Contains(nameof(IndustrialArmJointPosition), exception.Message);
    }

    [Fact]
    public void Write_WhenSequenceContainsIndustrialArmToolMove_ShouldRoundTripPoseAndFeed()
    {
        var sequence = new RobotCommandSequence(
        [
            new IndustrialArmLinearMoveCommand(
                new IndustrialArmToolPose(300, 0, 180, 20, 10, 0),
                requestedToolVelocityMillimetersPerSecond: 70)
        ]);

        var gCode = GCodeWriter.Write(sequence);
        var result = CreateParser().Parse(
            gCode,
            new RobotScriptParseContext(IndustrialArmJointPosition.Home));

        Assert.Equal("G21\r\nG90\r\nG1 X300 Y0 Z180 A20 B10 C0 F4200", gCode);
        Assert.IsType<IndustrialArmLinearMoveCommand>(Assert.Single(result.Commands));
    }

    private static GCodeParser CreateParser() =>
        new(new IndustrialArmGCodeCommandMapper(CreateProfile()));

    private static IndustrialArmRobotProfile CreateProfile() =>
        new(
            100,
            180,
            140,
            80,
            12,
            [
                new(IndustrialArmJointId.J1Base, -180, 180, 120, 240),
                new(IndustrialArmJointId.J2Shoulder, -120, 120, 100, 200),
                new(IndustrialArmJointId.J3Elbow, -150, 150, 90, 180),
                new(IndustrialArmJointId.J4WristRoll, -180, 180, 160, 320),
                new(IndustrialArmJointId.J5WristPitch, -120, 120, 110, 220),
                new(IndustrialArmJointId.J6ToolRoll, -360, 360, 200, 400)
            ]);
}
