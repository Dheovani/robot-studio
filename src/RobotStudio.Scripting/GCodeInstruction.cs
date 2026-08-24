using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting;

public abstract record GCodeInstruction(RobotCommandSource Source);

public sealed record GCodeLinearMoveInstruction(
    RobotCommandSource Source,
    double? XMillimeters,
    double? YMillimeters,
    double? ZMillimeters,
    double? FeedRateMillimetersPerMinute) : GCodeInstruction(Source);

public sealed record GCodeDwellInstruction(
    RobotCommandSource Source,
    TimeSpan Duration) : GCodeInstruction(Source);

public sealed record GCodeHomeInstruction(
    RobotCommandSource Source) : GCodeInstruction(Source);

public sealed record GCodePositioningModeInstruction(
    RobotCommandSource Source,
    RobotScriptPositioningMode Mode) : GCodeInstruction(Source);

public sealed record GCodeUnitInstruction(
    RobotCommandSource Source,
    RobotScriptUnit Unit) : GCodeInstruction(Source);
