using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting.Tests;

public sealed class RobotScriptParserTests
{
    [Fact]
    public void Parse_WhenScriptIsValid_ShouldReturnCommandSequence()
    {
        var parser = new RobotScriptParser();

        var sequence = parser.Parse(
            """
            HOME
            MOVE X=10 Y=20 Z=5 SPEED=100
            WAIT 500
            """);

        Assert.IsType<HomeCommand>(sequence.Commands[0]);
        Assert.Equal(1, sequence.Commands[0].Source?.LineNumber);
        Assert.Equal("HOME", sequence.Commands[0].Source?.Text);

        var move = Assert.IsType<MoveToCommand>(sequence.Commands[1]);
        Assert.Equal(10, move.TargetPosition.X);
        Assert.Equal(20, move.TargetPosition.Y);
        Assert.Equal(5, move.TargetPosition.Z);
        Assert.Equal(100, move.RequestedVelocityMillimetersPerSecond);
        Assert.Equal(2, move.Source?.LineNumber);
        Assert.Equal("MOVE X=10 Y=20 Z=5 SPEED=100", move.Source?.Text);

        var wait = Assert.IsType<WaitCommand>(sequence.Commands[2]);
        Assert.Equal(TimeSpan.FromMilliseconds(500), wait.Duration);
        Assert.Equal(3, wait.Source?.LineNumber);
        Assert.Equal("WAIT 500", wait.Source?.Text);
    }

    [Fact]
    public void Parse_WhenMoveHasNoSpeed_ShouldReturnMoveCommandWithoutRequestedVelocity()
    {
        var parser = new RobotScriptParser();

        var sequence = parser.Parse("MOVE X=10 Y=20 Z=5");

        var move = Assert.IsType<MoveToCommand>(Assert.Single(sequence.Commands));
        Assert.Null(move.RequestedVelocityMillimetersPerSecond);
    }

    [Fact]
    public void Parse_WhenDriveIsValid_ShouldReturnDifferentialDriveMoveCommand()
    {
        var parser = new RobotScriptParser();

        var sequence = parser.Parse("DRIVE X=10 Y=20 HEADING=90 LIN=100 ANG=45");

        var drive = Assert.IsType<DifferentialDriveMoveCommand>(Assert.Single(sequence.Commands));
        Assert.Equal(10, drive.TargetPose.X);
        Assert.Equal(20, drive.TargetPose.Y);
        Assert.Equal(90, drive.TargetPose.HeadingDegrees);
        Assert.Equal(100, drive.RequestedLinearVelocityMillimetersPerSecond);
        Assert.Equal(45, drive.RequestedAngularVelocityDegreesPerSecond);
        Assert.Equal(1, drive.Source?.LineNumber);
        Assert.Equal("DRIVE X=10 Y=20 HEADING=90 LIN=100 ANG=45", drive.Source?.Text);
    }

    [Fact]
    public void Parse_WhenDriveHasNoVelocities_ShouldReturnDriveCommandWithoutRequestedVelocities()
    {
        var parser = new RobotScriptParser();

        var sequence = parser.Parse("DRIVE X=10 Y=20 HEADING=90");

        var drive = Assert.IsType<DifferentialDriveMoveCommand>(Assert.Single(sequence.Commands));
        Assert.Null(drive.RequestedLinearVelocityMillimetersPerSecond);
        Assert.Null(drive.RequestedAngularVelocityDegreesPerSecond);
    }

    [Fact]
    public void Parse_WhenScaraIsValid_ShouldReturnScaraMoveJointsCommand()
    {
        var parser = new RobotScriptParser();

        var sequence = parser.Parse("SCARA SHOULDER=45 ELBOW=30 SPEED=80");

        var command = Assert.IsType<ScaraMoveJointsCommand>(Assert.Single(sequence.Commands));
        Assert.Equal(45, command.TargetJoints.ShoulderDegrees);
        Assert.Equal(30, command.TargetJoints.ElbowDegrees);
        Assert.Equal(80, command.RequestedJointVelocityDegreesPerSecond);
        Assert.Equal(1, command.Source?.LineNumber);
        Assert.Equal("SCARA SHOULDER=45 ELBOW=30 SPEED=80", command.Source?.Text);
    }

    [Fact]
    public void Parse_WhenScaraHasNoSpeed_ShouldReturnScaraCommandWithoutRequestedVelocity()
    {
        var parser = new RobotScriptParser();

        var sequence = parser.Parse("SCARA SHOULDER=45 ELBOW=30");

        var command = Assert.IsType<ScaraMoveJointsCommand>(Assert.Single(sequence.Commands));
        Assert.Null(command.RequestedJointVelocityDegreesPerSecond);
    }

    [Fact]
    public void Parse_WhenSimpleArmIsValid_ShouldReturnSimpleArmMoveJointsCommand()
    {
        var parser = new RobotScriptParser();

        var sequence = parser.Parse("ARM BASE=45 SHOULDER=30 ELBOW=-20 SPEED=80");

        var command = Assert.IsType<SimpleArmMoveJointsCommand>(Assert.Single(sequence.Commands));
        Assert.Equal(45, command.TargetJoints.BaseDegrees);
        Assert.Equal(30, command.TargetJoints.ShoulderDegrees);
        Assert.Equal(-20, command.TargetJoints.ElbowDegrees);
        Assert.Equal(80, command.RequestedJointVelocityDegreesPerSecond);
        Assert.Equal(1, command.Source?.LineNumber);
        Assert.Equal("ARM BASE=45 SHOULDER=30 ELBOW=-20 SPEED=80", command.Source?.Text);
    }

    [Fact]
    public void Parse_WhenSimpleArmHasNoSpeed_ShouldReturnSimpleArmCommandWithoutRequestedVelocity()
    {
        var parser = new RobotScriptParser();

        var sequence = parser.Parse("ARM BASE=45 SHOULDER=30 ELBOW=-20");

        var command = Assert.IsType<SimpleArmMoveJointsCommand>(Assert.Single(sequence.Commands));
        Assert.Null(command.RequestedJointVelocityDegreesPerSecond);
    }

    [Fact]
    public void Parse_WhenCommandIsUnknown_ShouldThrowWithLineNumber()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("JUMP X=10"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("Unknown command", exception.Message);
    }

    [Fact]
    public void Parse_WhenMoveMissesCoordinate_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("MOVE X=10 Y=20"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("MOVE requires Z", exception.Message);
    }

    [Fact]
    public void Parse_WhenDriveMissesHeading_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("DRIVE X=10 Y=20"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("DRIVE requires HEADING", exception.Message);
    }

    [Fact]
    public void Parse_WhenScaraMissesElbow_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("SCARA SHOULDER=45"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("SCARA requires ELBOW", exception.Message);
    }

    [Fact]
    public void Parse_WhenSimpleArmMissesBase_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(
            () => parser.Parse("ARM SHOULDER=45 ELBOW=30"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("ARM requires BASE", exception.Message);
    }

    [Fact]
    public void Parse_WhenNumberIsInvalid_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("MOVE X=abc Y=20 Z=5"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("X must be a valid number", exception.Message);
    }

    [Fact]
    public void Parse_WhenWaitDurationIsNegative_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("WAIT -1"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("WAIT duration cannot be negative", exception.Message);
    }

    [Fact]
    public void Parse_WhenScriptIsEmpty_ShouldThrowCommandSequenceError()
    {
        var parser = new RobotScriptParser();

        Assert.Throws<RobotStudio.Domain.Exceptions.InvalidRobotCommandException>(() => parser.Parse(""));
    }

    [Fact]
    public void Parse_WhenMoveHasDuplicateArgument_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() =>
            parser.Parse("MOVE X=10 X=20 Y=20 Z=5"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("Duplicate MOVE argument", exception.Message);
    }

    [Fact]
    public void Parse_WhenMoveHasUnknownArgument_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() =>
            parser.Parse("MOVE X=10 Y=20 Z=5 COLOR=red"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("Unknown MOVE argument", exception.Message);
    }

    [Fact]
    public void Parse_WhenDriveHasUnknownArgument_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() =>
            parser.Parse("DRIVE X=10 Y=20 HEADING=90 Z=0"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("Unknown DRIVE argument", exception.Message);
    }

    [Fact]
    public void Parse_WhenScaraHasUnknownArgument_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() =>
            parser.Parse("SCARA SHOULDER=45 ELBOW=30 Z=0"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("Unknown SCARA argument", exception.Message);
    }

    [Fact]
    public void Parse_WhenSimpleArmHasUnknownArgument_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() =>
            parser.Parse("ARM BASE=45 SHOULDER=30 ELBOW=-20 Z=0"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("Unknown ARM argument", exception.Message);
    }

    [Fact]
    public void Parse_WhenHomeHasArguments_ShouldThrow()
    {
        var parser = new RobotScriptParser();

        var exception = Assert.Throws<ScriptParseException>(() => parser.Parse("HOME X=0"));

        Assert.Equal(1, exception.LineNumber);
        Assert.Contains("HOME does not accept arguments", exception.Message);
    }
}
