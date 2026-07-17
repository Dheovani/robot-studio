using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class RobotCommandSequenceTests
{
    [Fact]
    public void Constructor_WhenCommandsIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new RobotCommandSequence(null!));
    }

    [Fact]
    public void Constructor_WhenCommandsIsEmpty_ShouldThrow()
    {
        var exception = Assert.Throws<InvalidRobotCommandException>(() =>
            new RobotCommandSequence(Array.Empty<RobotCommand>()));

        Assert.Contains("at least one command", exception.Message);
    }

    [Fact]
    public void Constructor_WhenCommandsContainsNull_ShouldThrow()
    {
        RobotCommand[] commands = [new HomeCommand(), null!];

        var exception = Assert.Throws<InvalidRobotCommandException>(() =>
            new RobotCommandSequence(commands));

        Assert.Contains("cannot contain null commands", exception.Message);
    }

    [Fact]
    public void Constructor_WhenCommandsAreValid_ShouldExposeCommandsInOrder()
    {
        RobotCommand[] commands =
        [
            new HomeCommand(),
            new MoveToCommand(new CartesianPosition(X: 10, Y: 20, Z: 30)),
            new WaitCommand(TimeSpan.FromMilliseconds(500))
        ];

        var sequence = new RobotCommandSequence(commands);

        Assert.Equal(commands, sequence.Commands);
    }
}
