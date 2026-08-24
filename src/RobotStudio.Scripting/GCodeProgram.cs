namespace RobotStudio.Scripting;

public sealed class GCodeProgram
{
    public GCodeProgram(IEnumerable<GCodeInstruction> instructions)
    {
        ArgumentNullException.ThrowIfNull(instructions);

        var materializedInstructions = instructions.ToArray();
        if (materializedInstructions.Any(instruction => instruction is null))
        {
            throw new ArgumentException(
                "A G-code program cannot contain null instructions.",
                nameof(instructions));
        }

        Instructions = materializedInstructions;
    }

    public IReadOnlyList<GCodeInstruction> Instructions { get; }
}
