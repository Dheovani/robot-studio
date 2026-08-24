using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public sealed class ScaraGCodeCommandMapper : IGCodeCommandMapper
{
    private const double PlanarToleranceMillimeters = 0.000_001;

    private readonly ScaraRobotProfile robotProfile;
    private readonly ScaraKinematics kinematics;

    public ScaraGCodeCommandMapper(ScaraRobotProfile robotProfile)
        : this(robotProfile, new ScaraKinematics())
    {
    }

    public ScaraGCodeCommandMapper(
        ScaraRobotProfile robotProfile,
        ScaraKinematics kinematics)
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
        var currentToolPose = ResolveInitialToolPose(context);

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
                    currentToolPose = kinematics.Forward(
                        robotProfile,
                        new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0));
                    statements.Add(new RobotScriptCommandStatement(
                        new HomeCommand(home.Source)));
                    break;

                case GCodeDwellInstruction dwell:
                    statements.Add(new RobotScriptCommandStatement(
                        new WaitCommand(dwell.Duration, dwell.Source)));
                    break;

                case GCodeLinearMoveInstruction move:
                    EnsurePlanarMove(move);
                    currentToolPose = ResolveTarget(move, positioningMode, currentToolPose);
                    _ = kinematics.InverseElbowDown(robotProfile, currentToolPose.Value);
                    statements.Add(new RobotScriptCommandStatement(
                        new ScaraLinearMoveCommand(
                            currentToolPose.Value,
                            move.FeedRateMillimetersPerMinute / 60d,
                            move.Source)));
                    break;

                default:
                    throw new NotSupportedException(
                        $"The SCARA G-code mapper does not support {instruction.GetType().Name}.");
            }
        }

        return new RobotScriptCompilation(statements);
    }

    private ScaraToolPose? ResolveInitialToolPose(
        RobotScriptParseContext? context) =>
        context?.InitialPosition switch
        {
            null => null,
            ScaraJointPosition joints => kinematics.Forward(robotProfile, joints),
            var position => throw new ArgumentException(
                $"The SCARA G-code mapping requires a {nameof(ScaraJointPosition)} initial position, but received {position.GetType().Name}.",
                nameof(context))
        };

    private static ScaraToolPose ResolveTarget(
        GCodeLinearMoveInstruction move,
        RobotScriptPositioningMode positioningMode,
        ScaraToolPose? currentToolPose) => positioningMode switch
        {
            RobotScriptPositioningMode.Absolute => ResolveAbsoluteTarget(move, currentToolPose),
            RobotScriptPositioningMode.Relative => ResolveRelativeTarget(move, currentToolPose),
            _ => throw new InvalidOperationException(
                $"Unsupported G-code positioning mode: {positioningMode}.")
        };

    private static ScaraToolPose ResolveAbsoluteTarget(
        GCodeLinearMoveInstruction move,
        ScaraToolPose? currentToolPose)
    {
        if (currentToolPose is null &&
            (move.XMillimeters is null || move.YMillimeters is null))
        {
            throw CreateMappingException(
                move,
                "SCARA G1 with omitted X or Y requires known initial joints, a previous complete target, or G28.");
        }

        return new ScaraToolPose(
            move.XMillimeters ?? currentToolPose?.X ?? 0,
            move.YMillimeters ?? currentToolPose?.Y ?? 0);
    }

    private static ScaraToolPose ResolveRelativeTarget(
        GCodeLinearMoveInstruction move,
        ScaraToolPose? currentToolPose)
    {
        if (currentToolPose is null)
        {
            throw CreateMappingException(
                move,
                "SCARA G91 relative movement requires known initial joints, a previous complete target, or G28.");
        }

        return new ScaraToolPose(
            currentToolPose.Value.X + (move.XMillimeters ?? 0),
            currentToolPose.Value.Y + (move.YMillimeters ?? 0));
    }

    private static void EnsurePlanarMove(GCodeLinearMoveInstruction move)
    {
        if (move.ADegrees is not null || move.BDegrees is not null || move.CDegrees is not null)
        {
            throw CreateMappingException(
                move,
                "This SCARA model does not control TCP orientation and rejects A, B, and C words.");
        }

        if (move.ZMillimeters is { } z && Math.Abs(z) > PlanarToleranceMillimeters)
        {
            throw CreateMappingException(
                move,
                $"This SCARA model is planar and accepts only Z0 or an omitted Z word. Received Z{z:0.###}.");
        }
    }

    private static ScriptParseException CreateMappingException(
        GCodeInstruction instruction,
        string message) =>
        new(instruction.Source.LineNumber, instruction.Source.Text, message);
}
