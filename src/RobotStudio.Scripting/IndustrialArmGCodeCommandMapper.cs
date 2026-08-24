using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public sealed class IndustrialArmGCodeCommandMapper : IGCodeCommandMapper
{
    private readonly IndustrialArmRobotProfile robotProfile;
    private readonly IndustrialArmKinematics kinematics;
    private readonly IndustrialArmConfiguration configuration;

    public IndustrialArmGCodeCommandMapper(
        IndustrialArmRobotProfile robotProfile,
        IndustrialArmConfiguration configuration = IndustrialArmConfiguration.PositiveElbowWristNeutral)
        : this(robotProfile, new IndustrialArmKinematics(), configuration)
    {
    }

    public IndustrialArmGCodeCommandMapper(
        IndustrialArmRobotProfile robotProfile,
        IndustrialArmKinematics kinematics,
        IndustrialArmConfiguration configuration = IndustrialArmConfiguration.PositiveElbowWristNeutral)
    {
        ArgumentNullException.ThrowIfNull(robotProfile);
        ArgumentNullException.ThrowIfNull(kinematics);
        this.robotProfile = robotProfile;
        this.kinematics = kinematics;
        this.configuration = configuration;
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
                    currentPose = kinematics.Forward(robotProfile, IndustrialArmJointPosition.Home);
                    statements.Add(new RobotScriptCommandStatement(new HomeCommand(home.Source)));
                    break;

                case GCodeDwellInstruction dwell:
                    statements.Add(new RobotScriptCommandStatement(
                        new WaitCommand(dwell.Duration, dwell.Source)));
                    break;

                case GCodeLinearMoveInstruction move:
                    currentPose = ResolveTarget(move, positioningMode, currentPose);
                    _ = kinematics.Inverse(robotProfile, currentPose.Value, configuration);
                    statements.Add(new RobotScriptCommandStatement(
                        new IndustrialArmLinearMoveCommand(
                            currentPose.Value,
                            move.FeedRateMillimetersPerMinute / 60d,
                            configuration,
                            move.Source)));
                    break;

                default:
                    throw new NotSupportedException(
                        $"The industrial arm G-code mapper does not support {instruction.GetType().Name}.");
            }
        }

        return new RobotScriptCompilation(statements);
    }

    private IndustrialArmToolPose? ResolveInitialPose(RobotScriptParseContext? context) =>
        context?.InitialPosition switch
        {
            null => null,
            IndustrialArmJointPosition joints => kinematics.Forward(robotProfile, joints),
            var position => throw new ArgumentException(
                $"The industrial arm G-code mapping requires an {nameof(IndustrialArmJointPosition)} initial position, but received {position.GetType().Name}.",
                nameof(context))
        };

    private static IndustrialArmToolPose ResolveTarget(
        GCodeLinearMoveInstruction move,
        RobotScriptPositioningMode mode,
        IndustrialArmToolPose? currentPose) => mode switch
        {
            RobotScriptPositioningMode.Absolute => ResolveAbsoluteTarget(move, currentPose),
            RobotScriptPositioningMode.Relative => ResolveRelativeTarget(move, currentPose),
            _ => throw new InvalidOperationException($"Unsupported G-code positioning mode: {mode}.")
        };

    private static IndustrialArmToolPose ResolveAbsoluteTarget(
        GCodeLinearMoveInstruction move,
        IndustrialArmToolPose? currentPose)
    {
        if (currentPose is null &&
            (move.XMillimeters is null ||
             move.YMillimeters is null ||
             move.ZMillimeters is null ||
             move.ADegrees is null ||
             move.BDegrees is null ||
             move.CDegrees is null))
        {
            throw CreateMappingException(
                move,
                "Industrial arm G1 with omitted X, Y, Z, A, B, or C requires known initial joints, a previous complete target, or G28.");
        }

        return new IndustrialArmToolPose(
            move.XMillimeters ?? currentPose?.XMillimeters ?? 0,
            move.YMillimeters ?? currentPose?.YMillimeters ?? 0,
            move.ZMillimeters ?? currentPose?.ZMillimeters ?? 0,
            NormalizeDegrees(move.ADegrees ?? currentPose?.RollDegrees ?? 0),
            NormalizeDegrees(move.BDegrees ?? currentPose?.PitchDegrees ?? 0),
            NormalizeDegrees(move.CDegrees ?? currentPose?.YawDegrees ?? 0));
    }

    private static IndustrialArmToolPose ResolveRelativeTarget(
        GCodeLinearMoveInstruction move,
        IndustrialArmToolPose? currentPose)
    {
        if (currentPose is null)
        {
            throw CreateMappingException(
                move,
                "Industrial arm G91 relative movement requires known initial joints, a previous complete target, or G28.");
        }

        return new IndustrialArmToolPose(
            currentPose.Value.XMillimeters + (move.XMillimeters ?? 0),
            currentPose.Value.YMillimeters + (move.YMillimeters ?? 0),
            currentPose.Value.ZMillimeters + (move.ZMillimeters ?? 0),
            NormalizeDegrees(currentPose.Value.RollDegrees + (move.ADegrees ?? 0)),
            NormalizeDegrees(currentPose.Value.PitchDegrees + (move.BDegrees ?? 0)),
            NormalizeDegrees(currentPose.Value.YawDegrees + (move.CDegrees ?? 0)));
    }

    private static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360;
        return normalized switch
        {
            > 180 => normalized - 360,
            < -180 => normalized + 360,
            _ => normalized
        };
    }

    private static ScriptParseException CreateMappingException(
        GCodeInstruction instruction,
        string message) =>
        new(instruction.Source.LineNumber, instruction.Source.Text, message);
}
