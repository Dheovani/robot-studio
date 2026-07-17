using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Commands;

public sealed class RobotCommandSequence
{
    public RobotCommandSequence(IEnumerable<RobotCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);

        var commandList = commands.ToArray();

        if (commandList.Length == 0)
        {
            throw new InvalidRobotCommandException("A command sequence must contain at least one command.");
        }

        if (commandList.Any(command => command is null))
        {
            throw new InvalidRobotCommandException("A command sequence cannot contain null commands.");
        }

        Commands = Array.AsReadOnly(commandList);
    }

    public IReadOnlyList<RobotCommand> Commands { get; }
}
