using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Parallel;

namespace RobotStudio.Scripting;

public sealed class DeltaGCodeCommandMapper : IGCodeCommandMapper
{
    private readonly DeltaRobotProfile robotProfile;
    private readonly DeltaKinematics kinematics;

    public DeltaGCodeCommandMapper(DeltaRobotProfile robotProfile)
        : this(robotProfile, new DeltaKinematics())
    {
    }

    public DeltaGCodeCommandMapper(
        DeltaRobotProfile robotProfile,
        DeltaKinematics kinematics)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);
        ArgumentNullException.ThrowIfNull(kinematics);
        this.robotProfile = robotProfile;
        this.kinematics = kinematics;
    }

    public RobotScriptCompilation Map(
        GCodeProgram program,
        RobotScriptParseContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(program);

        var statements = new List<RobotScriptStatement>();
        var positioningMode = RobotScriptPositioningMode.Absolute;
        var currentPose = ResolveInitialPose(context);

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
                    currentPose = kinematics.Forward(
                        robotProfile,
                        new DeltaActuatorPosition(0, 0, 0));
                    statements.Add(new RobotScriptCommandStatement(new HomeCommand(home.Source)));
                    break;

                case GCodeDwellInstruction dwell:
                    statements.Add(new RobotScriptCommandStatement(
                        new WaitCommand(dwell.Duration, dwell.Source)));
                    break;

                case GCodeLinearMoveInstruction move:
                    EnsurePositionOnly(move);
                    currentPose = ResolveTarget(move, positioningMode, currentPose);
                    _ = kinematics.Inverse(robotProfile, currentPose.Value);
                    statements.Add(new RobotScriptCommandStatement(
                        new DeltaLinearMoveCommand(
                            currentPose.Value,
                            move.FeedRateMillimetersPerMinute / 60d,
                            move.Source)));
                    break;

                default:
                    throw new NotSupportedException(
                        $"The Delta G-code mapper does not support {instruction.GetType().Name}.");
            }
        }

        return new RobotScriptCompilation(statements);
    }

    private DeltaToolPose? ResolveInitialPose(RobotScriptParseContext? context) =>
        context?.InitialPosition switch
        {
            null => null,
            DeltaActuatorPosition actuators => kinematics.Forward(robotProfile, actuators),
            var position => throw new ArgumentException(
                $"The Delta G-code mapping requires a {nameof(DeltaActuatorPosition)} initial position, but received {position.GetType().Name}.",
                nameof(context))
        };

    private static DeltaToolPose ResolveTarget(
        GCodeLinearMoveInstruction move,
        RobotScriptPositioningMode mode,
        DeltaToolPose? currentPose) => mode switch
        {
            RobotScriptPositioningMode.Absolute => ResolveAbsoluteTarget(move, currentPose),
            RobotScriptPositioningMode.Relative => ResolveRelativeTarget(move, currentPose),
            _ => throw new InvalidOperationException($"Unsupported G-code positioning mode: {mode}.")
        };

    private static DeltaToolPose ResolveAbsoluteTarget(
        GCodeLinearMoveInstruction move,
        DeltaToolPose? currentPose)
    {
        if (currentPose is null &&
            (move.XMillimeters is null || move.YMillimeters is null || move.ZMillimeters is null))
        {
            throw CreateMappingException(
                move,
                "Delta G1 with omitted X, Y, or Z requires known initial actuators, a previous complete target, or G28.");
        }

        return new DeltaToolPose(
            move.XMillimeters ?? currentPose?.XMillimeters ?? 0,
            move.YMillimeters ?? currentPose?.YMillimeters ?? 0,
            move.ZMillimeters ?? currentPose?.ZMillimeters ?? 0);
    }

    private static DeltaToolPose ResolveRelativeTarget(
        GCodeLinearMoveInstruction move,
        DeltaToolPose? currentPose)
    {
        if (currentPose is null)
        {
            throw CreateMappingException(
                move,
                "Delta G91 relative movement requires known initial actuators, a previous complete target, or G28.");
        }

        return new DeltaToolPose(
            currentPose.Value.XMillimeters + (move.XMillimeters ?? 0),
            currentPose.Value.YMillimeters + (move.YMillimeters ?? 0),
            currentPose.Value.ZMillimeters + (move.ZMillimeters ?? 0));
    }

    private static void EnsurePositionOnly(GCodeLinearMoveInstruction move)
    {
        if (move.ADegrees is not null || move.BDegrees is not null || move.CDegrees is not null)
        {
            throw CreateMappingException(
                move,
                "The introductory Delta model controls TCP position and does not support A, B, or C orientation words.");
        }
    }

    private static ScriptParseException CreateMappingException(
        GCodeInstruction instruction,
        string message) =>
        new(instruction.Source.LineNumber, instruction.Source.Text, message);
}
