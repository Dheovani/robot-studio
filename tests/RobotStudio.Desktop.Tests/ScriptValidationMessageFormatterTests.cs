using RobotStudio.Desktop.Scripting;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Exceptions;
using RobotStudio.Scripting;

namespace RobotStudio.Desktop.Tests;

public sealed class ScriptValidationMessageFormatterTests
{
    [Fact]
    public void Format_WhenScriptParseException_ShouldIncludeLineAndNextStep()
    {
        var exception = new ScriptParseException(
            lineNumber: 3,
            lineText: "MOVE X=10",
            message: "MOVE requires Y.");

        var message = ScriptValidationMessageFormatter.Format(exception);

        Assert.Contains("line 3", message);
        Assert.Contains("MOVE requires Y.", message);
        Assert.Contains("validate again", message);
    }

    [Fact]
    public void Format_WhenPositionOutOfRangeException_ShouldExplainPhysicalLimit()
    {
        var exception = new PositionOutOfRangeException(AxisId.X, 400, 0, 300);

        var message = ScriptValidationMessageFormatter.Format(exception);

        Assert.Contains("Physical limit exceeded", message);
        Assert.Contains("inside the robot workspace", message);
    }

    [Fact]
    public void Format_WhenInvalidRobotCommandException_ShouldExplainCommandArguments()
    {
        var exception = new InvalidRobotCommandException("Requested speed must be positive.");

        var message = ScriptValidationMessageFormatter.Format(exception);

        Assert.Contains("Invalid robot command", message);
        Assert.Contains("positive speed or duration", message);
    }
}
