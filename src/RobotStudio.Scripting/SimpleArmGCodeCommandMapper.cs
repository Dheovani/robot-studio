using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public sealed class SimpleArmGCodeCommandMapper : IGCodeCommandMapper
{
    private const double PlanarToleranceMillimeters = 0.000_001;

    private readonly SimpleArmRobotProfile robotProfile;
    private readonly SimpleArmKinematics kinematics;

    public SimpleArmGCodeCommandMapper(SimpleArmRobotProfile robotProfile)
        : this(robotProfile, new SimpleArmKinematics())
    {
    }

    public SimpleArmGCodeCommandMapper(
        SimpleArmRobotProfile robotProfile,
        SimpleArmKinematics kinematics)
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
                        new SimpleArmJointPosition(0, 0, 0));
                    statements.Add(new RobotScriptCommandStatement(new HomeCommand(home.Source)));
                    break;

                case GCodeDwellInstruction dwell:
                    statements.Add(new RobotScriptCommandStatement(
                        new WaitCommand(dwell.Duration, dwell.Source)));
                    break;

                case GCodeLinearMoveInstruction move:
                    EnsureSupportedWords(move);
                    currentPose = ResolveTarget(move, positioningMode, currentPose);
                    _ = kinematics.InversePositiveBend(robotProfile, currentPose.Value);
                    statements.Add(new RobotScriptCommandStatement(
                        new SimpleArmLinearMoveCommand(
                            currentPose.Value,
                            move.FeedRateMillimetersPerMinute / 60d,
                            move.Source)));
                    break;

                default:
                    throw new NotSupportedException(
                        $"The Simple Arm G-code mapper does not support {instruction.GetType().Name}.");
            }
        }

        return new RobotScriptCompilation(statements);
    }

    private SimpleArmToolPose? ResolveInitialPose(RobotScriptParseContext? context) =>
        context?.InitialPosition switch
        {
            null => null,
            SimpleArmJointPosition joints => kinematics.Forward(robotProfile, joints),
            var position => throw new ArgumentException(
                $"The Simple Arm G-code mapping requires a {nameof(SimpleArmJointPosition)} initial position, but received {position.GetType().Name}.",
                nameof(context))
        };

    private static SimpleArmToolPose ResolveTarget(
        GCodeLinearMoveInstruction move,
        RobotScriptPositioningMode mode,
        SimpleArmToolPose? currentPose) => mode switch
        {
            RobotScriptPositioningMode.Absolute => ResolveAbsoluteTarget(move, currentPose),
            RobotScriptPositioningMode.Relative => ResolveRelativeTarget(move, currentPose),
            _ => throw new InvalidOperationException($"Unsupported G-code positioning mode: {mode}.")
        };

    private static SimpleArmToolPose ResolveAbsoluteTarget(
        GCodeLinearMoveInstruction move,
        SimpleArmToolPose? currentPose)
    {
        if (currentPose is null &&
            (move.XMillimeters is null || move.YMillimeters is null || move.ADegrees is null))
        {
            throw CreateMappingException(
                move,
                "Simple Arm G1 with omitted X, Y, or A requires known initial joints, a previous complete target, or G28.");
        }

        return new SimpleArmToolPose(
            move.XMillimeters ?? currentPose?.X ?? 0,
            move.YMillimeters ?? currentPose?.Y ?? 0,
            NormalizeDegrees(move.ADegrees ?? currentPose?.OrientationDegrees ?? 0));
    }

    private static SimpleArmToolPose ResolveRelativeTarget(
        GCodeLinearMoveInstruction move,
        SimpleArmToolPose? currentPose)
    {
        if (currentPose is null)
        {
            throw CreateMappingException(
                move,
                "Simple Arm G91 relative movement requires known initial joints, a previous complete target, or G28.");
        }

        return new SimpleArmToolPose(
            currentPose.Value.X + (move.XMillimeters ?? 0),
            currentPose.Value.Y + (move.YMillimeters ?? 0),
            NormalizeDegrees(currentPose.Value.OrientationDegrees + (move.ADegrees ?? 0)));
    }

    private static void EnsureSupportedWords(GCodeLinearMoveInstruction move)
    {
        if (move.ZMillimeters is { } z && Math.Abs(z) > PlanarToleranceMillimeters)
        {
            throw CreateMappingException(
                move,
                $"The Simple Arm is planar and accepts only Z0 or an omitted Z word. Received Z{z:0.###}.");
        }

        if (move.BDegrees is not null || move.CDegrees is not null)
        {
            throw CreateMappingException(
                move,
                "The Simple Arm uses A for planar TCP orientation and does not support B or C.");
        }
    }

    private static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360;
        if (normalized > 180)
        {
            normalized -= 360;
        }

        if (normalized < -180)
        {
            normalized += 360;
        }

        return normalized;
    }

    private static ScriptParseException CreateMappingException(
        GCodeInstruction instruction,
        string message) =>
        new(instruction.Source.LineNumber, instruction.Source.Text, message);
}
