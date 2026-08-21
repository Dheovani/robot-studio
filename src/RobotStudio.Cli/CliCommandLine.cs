namespace RobotStudio.Cli;

public sealed record CliCommandLine(
    IReadOnlyList<string> Arguments,
    string? DialectName)
{
    public static CliCommandLine Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var arguments = new List<string>();
        string? dialectName = null;

        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];
            if (argument.Equals("--dialect", StringComparison.OrdinalIgnoreCase))
            {
                if (dialectName is not null)
                {
                    throw new ArgumentException("The --dialect option can be specified only once.");
                }

                if (++index >= args.Count || args[index].StartsWith("--", StringComparison.Ordinal))
                {
                    throw new ArgumentException("The --dialect option requires 'dsl' or 'gcode'.");
                }

                dialectName = args[index];
                continue;
            }

            const string dialectPrefix = "--dialect=";
            if (argument.StartsWith(dialectPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (dialectName is not null)
                {
                    throw new ArgumentException("The --dialect option can be specified only once.");
                }

                dialectName = argument[dialectPrefix.Length..];
                if (string.IsNullOrWhiteSpace(dialectName))
                {
                    throw new ArgumentException("The --dialect option requires 'dsl' or 'gcode'.");
                }

                continue;
            }

            if (argument.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unknown option '{argument}'.");
            }

            arguments.Add(argument);
        }

        return new CliCommandLine(arguments.AsReadOnly(), dialectName);
    }
}
