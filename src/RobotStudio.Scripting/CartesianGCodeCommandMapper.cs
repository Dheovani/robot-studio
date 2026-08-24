using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public sealed class CartesianGCodeCommandMapper : IGCodeCommandMapper
{
    public RobotScriptCompilation Map(
        GCodeProgram program,
        RobotScriptParseContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(program);

        var statements = new List<RobotScriptStatement>();
        var positioningMode = RobotScriptPositioningMode.Absolute;
        var currentPosition = ResolveInitialPosition(context);

        foreach (var instruction in program.Instructions)
        {
            switch (instruction)
            {
                case GCodePositioningModeInstruction positioning:
                    positioningMode = positioning.Mode;
                    statements.Add(new RobotScriptPositioningModeStatement(
                        positioning.Source,
                        positioning.Mode));
                    break;

                case GCodeUnitInstruction unit:
                    statements.Add(new RobotScriptUnitStatement(unit.Source, unit.Unit));
                    break;

                case GCodeHomeInstruction home:
                    currentPosition = new CartesianPosition(0, 0, 0);
                    statements.Add(new RobotScriptCommandStatement(
                        new HomeCommand(home.Source)));
                    break;

                case GCodeDwellInstruction dwell:
                    statements.Add(new RobotScriptCommandStatement(
                        new WaitCommand(dwell.Duration, dwell.Source)));
                    break;

                case GCodeLinearMoveInstruction move:
                    currentPosition = ResolveTarget(move, positioningMode, currentPosition);
                    statements.Add(new RobotScriptCommandStatement(
                        new MoveToCommand(
                            currentPosition.Value,
                            move.FeedRateMillimetersPerMinute / 60d,
                            move.Source)));
                    break;

                default:
                    throw new NotSupportedException(
                        $"The Cartesian G-code mapper does not support {instruction.GetType().Name}.");
            }
        }

        return new RobotScriptCompilation(statements);
    }

    private static CartesianPosition? ResolveInitialPosition(
        RobotScriptParseContext? context) =>
        context?.InitialPosition switch
        {
            null => null,
            CartesianPosition position => position,
            var position => throw new ArgumentException(
                $"The Cartesian G-code mapping requires a {nameof(CartesianPosition)} initial position, but received {position.GetType().Name}. " +
                "G-code describes tool-space coordinates and no compatible mapping exists for this robot position type.",
                nameof(context))
        };

    private static CartesianPosition ResolveTarget(
        GCodeLinearMoveInstruction move,
        RobotScriptPositioningMode positioningMode,
        CartesianPosition? currentPosition) => positioningMode switch
        {
            RobotScriptPositioningMode.Absolute => ResolveAbsoluteTarget(move, currentPosition),
            RobotScriptPositioningMode.Relative => ResolveRelativeTarget(move, currentPosition),
            _ => throw new InvalidOperationException(
                $"Unsupported G-code positioning mode: {positioningMode}.")
        };

    private static CartesianPosition ResolveAbsoluteTarget(
        GCodeLinearMoveInstruction move,
        CartesianPosition? currentPosition)
    {
        if (currentPosition is null &&
            (move.XMillimeters is null || move.YMillimeters is null || move.ZMillimeters is null))
        {
            throw CreateMappingException(
                move,
                "G1 with omitted axes requires a known initial Cartesian position, a previous complete G1 target, or G28.");
        }

        return new CartesianPosition(
            move.XMillimeters ?? currentPosition?.X ?? 0,
            move.YMillimeters ?? currentPosition?.Y ?? 0,
            move.ZMillimeters ?? currentPosition?.Z ?? 0);
    }

    private static CartesianPosition ResolveRelativeTarget(
        GCodeLinearMoveInstruction move,
        CartesianPosition? currentPosition)
    {
        if (currentPosition is null)
        {
            throw CreateMappingException(
                move,
                "G91 relative movement requires a known initial Cartesian position, a previous complete G1 target, or G28.");
        }

        return new CartesianPosition(
            currentPosition.Value.X + (move.XMillimeters ?? 0),
            currentPosition.Value.Y + (move.YMillimeters ?? 0),
            currentPosition.Value.Z + (move.ZMillimeters ?? 0));
    }

    private static ScriptParseException CreateMappingException(
        GCodeInstruction instruction,
        string message) =>
        new(instruction.Source.LineNumber, instruction.Source.Text, message);
}
