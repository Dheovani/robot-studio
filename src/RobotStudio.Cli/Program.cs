using System.Globalization;
using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Scripting;
using RobotStudio.Simulation;

const string ExampleScript =
    """
    HOME
    MOVE X=120 Y=80 Z=40 SPEED=90
    WAIT 500
    """;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

var profile = RobotProfile.CreateCartesian(
    new Axis(AxisId.X, 0, 300, 120, 240),
    new Axis(AxisId.Y, 0, 200, 100, 200),
    new Axis(AxisId.Z, 0, 150, 80, 160));

var initialPosition = new CartesianPosition(X: 40, Y: 30, Z: 20);
var parser = new RobotScriptParser();

try
{
    return args switch
    {
        [] => SimulateScript(ExampleScript, profile, initialPosition, parser),
        ["example"] => PrintExampleScript(),
        ["validate", var path] => ValidateScriptFile(path, profile, parser),
        ["simulate", var path] => SimulateScriptFile(path, profile, initialPosition, parser),
        _ => PrintUsage()
    };
}
catch (IOException exception)
{
    Console.Error.WriteLine($"File error: {exception.Message}");
    return 1;
}
catch (UnauthorizedAccessException exception)
{
    Console.Error.WriteLine($"File access error: {exception.Message}");
    return 1;
}
catch (ScriptParseException exception)
{
    Console.Error.WriteLine($"Script error: {exception.Message}");
    Console.Error.WriteLine($"Source: {exception.LineText}");
    return 1;
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine($"Validation error: {exception.Message}");
    return 1;
}

static int PrintExampleScript()
{
    Console.WriteLine(ExampleScript);
    return 0;
}

static int ValidateScriptFile(
    string path,
    RobotProfile profile,
    RobotScriptParser parser)
{
    var script = File.ReadAllText(path);
    var commands = parser.Parse(script);
    ValidateCommandSequence(commands, profile);

    Console.WriteLine("Script is valid.");
    Console.WriteLine();
    PrintCommandSequence(commands);

    return 0;
}

static int SimulateScriptFile(
    string path,
    RobotProfile profile,
    CartesianPosition initialPosition,
    RobotScriptParser parser)
{
    var script = File.ReadAllText(path);

    return SimulateScript(script, profile, initialPosition, parser);
}

static int SimulateScript(
    string script,
    RobotProfile profile,
    CartesianPosition initialPosition,
    RobotScriptParser parser)
{
    var context = SimulationContext.Create(profile, initialPosition);
    var commands = parser.Parse(script);
    ValidateCommandSequence(commands, profile);

    var simulator = new RobotSimulator();
    var result = simulator.Execute(context, commands);

    Console.WriteLine("RobotStudio CLI");
    Console.WriteLine();
    PrintProfile(profile);
    Console.WriteLine();
    PrintCommandSequence(commands);
    Console.WriteLine();
    PrintTimeline(result);
    Console.WriteLine();
    PrintFinalResult(result);

    return result.Succeeded ? 0 : 1;
}

static int PrintUsage()
{
    Console.WriteLine("RobotStudio CLI");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli");
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- example");
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- validate <script-file>");
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- simulate <script-file>");

    return 1;
}

static void ValidateCommandSequence(
    RobotCommandSequence commandSequence,
    RobotProfile profile)
{
    foreach (var command in commandSequence.Commands)
    {
        RobotCommandValidator.Validate(command, profile);
    }
}

static void PrintProfile(RobotProfile profile)
{
    Console.WriteLine("Robot profile:");

    foreach (var axis in profile.Axes)
    {
        Console.WriteLine(
            $"- {axis.Id}: {axis.MinimumMillimeters:0.###} mm to {axis.MaximumMillimeters:0.###} mm, " +
            $"max {axis.MaximumVelocityMillimetersPerSecond:0.###} mm/s, " +
            $"max {axis.MaximumAccelerationMillimetersPerSecondSquared:0.###} mm/s^2");
    }
}

static void PrintCommandSequence(RobotCommandSequence commandSequence)
{
    Console.WriteLine("Commands:");

    for (var index = 0; index < commandSequence.Commands.Count; index++)
    {
        Console.WriteLine($"- {index + 1}. {DescribeCommand(commandSequence.Commands[index])}");
    }
}

static void PrintTimeline(SimulationResult result)
{
    Console.WriteLine("Timeline:");

    foreach (var step in result.Timeline)
    {
        Console.WriteLine(
            $"- t={step.Time.TotalSeconds,6:0.###}s | " +
            $"{step.State,-9} | " +
            $"X={step.Position.X,7:0.###} mm " +
            $"Y={step.Position.Y,7:0.###} mm " +
            $"Z={step.Position.Z,7:0.###} mm | " +
            $"{FormatCommandSource(step)} | " +
            step.Description);
    }
}

static string FormatCommandSource(SimulationStep step)
{
    if (step.CommandIndex is null || step.CommandName is null)
    {
        return "simulation";
    }

    return $"command {step.CommandIndex.Value + 1}: {step.CommandName}";
}

static void PrintFinalResult(SimulationResult result)
{
    Console.WriteLine(result.Succeeded ? "Simulation completed." : "Simulation failed.");
    Console.WriteLine($"Final state: {result.FinalContext.State}");
    Console.WriteLine(
        $"Final position: X={result.FinalContext.CurrentPosition.X:0.###} mm, " +
        $"Y={result.FinalContext.CurrentPosition.Y:0.###} mm, " +
        $"Z={result.FinalContext.CurrentPosition.Z:0.###} mm");
    Console.WriteLine($"Total simulated time: {result.FinalContext.ElapsedTime.TotalSeconds:0.###} s");

    if (result.Failure is not null)
    {
        Console.WriteLine($"Failure: {result.Failure.Message}");
    }
}

static string DescribeCommand(RobotCommand command) => command switch
{
    HomeCommand => "HOME",
    MoveToCommand moveToCommand => DescribeMoveCommand(moveToCommand),
    WaitCommand waitCommand => $"WAIT {waitCommand.Duration.TotalMilliseconds:0.###} ms",
    _ => command.GetType().Name
};

static string DescribeMoveCommand(MoveToCommand command)
{
    var description =
        $"MOVE X={command.TargetPosition.X:0.###} " +
        $"Y={command.TargetPosition.Y:0.###} " +
        $"Z={command.TargetPosition.Z:0.###}";

    return command.RequestedVelocityMillimetersPerSecond.HasValue
        ? $"{description} SPEED={command.RequestedVelocityMillimetersPerSecond.Value:0.###}"
        : description;
}
