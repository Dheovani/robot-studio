using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public sealed class RobotScriptCompilation
{
    public RobotScriptCompilation(
        IEnumerable<RobotScriptStatement> statements)
    {
        ArgumentNullException.ThrowIfNull(statements);

        var materializedStatements = statements.ToArray();
        if (materializedStatements.Any(statement => statement is null))
        {
            throw new ArgumentException(
                "A script compilation cannot contain null statements.",
                nameof(statements));
        }

        Statements = materializedStatements;
        Commands = new RobotCommandSequence(
            Statements
                .OfType<RobotScriptCommandStatement>()
                .Select(statement => statement.Command));
    }

    public IReadOnlyList<RobotScriptStatement> Statements { get; }

    public RobotCommandSequence Commands { get; }
}
