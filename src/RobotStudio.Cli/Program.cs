using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using RobotStudio.Domain.Cartesian;
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

var profile = CartesianRobotProfile.CreateCartesian(
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
        ["playback", var path, var intervalMilliseconds] => PrintPlaybackFile(
            path,
            intervalMilliseconds,
            profile,
            initialPosition,
            parser),
        ["export-playback", var path, var intervalMilliseconds, var outputPath] => ExportPlaybackFile(
            path,
            intervalMilliseconds,
            outputPath,
            profile,
            initialPosition,
            parser),
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
    CartesianRobotProfile profile,
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
    CartesianRobotProfile profile,
    CartesianPosition initialPosition,
    RobotScriptParser parser)
{
    var script = File.ReadAllText(path);

    return SimulateScript(script, profile, initialPosition, parser);
}

static int SimulateScript(
    string script,
    CartesianRobotProfile profile,
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
    PrintSimulationResult(result);

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
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- playback <script-file> <interval-ms>");
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- export-playback <script-file> <interval-ms> <output-json>");

    return 1;
}

static int PrintPlaybackFile(
    string path,
    string intervalMillisecondsText,
    CartesianRobotProfile profile,
    CartesianPosition initialPosition,
    RobotScriptParser parser)
{
    var interval = ParsePositiveMilliseconds(intervalMillisecondsText);
    var script = File.ReadAllText(path);
    var snapshot = BuildPlaybackSnapshot(script, profile, initialPosition, parser, interval);

    Console.WriteLine("RobotStudio CLI");
    Console.WriteLine();
    Console.WriteLine($"Playback interval: {interval.TotalMilliseconds:0.###} ms");
    Console.WriteLine();
    PrintWorkspaceBounds(snapshot.WorkspaceBounds);
    Console.WriteLine();
    PrintPlayback(snapshot.Frames);
    Console.WriteLine();
    PrintSnapshotResult(snapshot);

    return snapshot.Succeeded ? 0 : 1;
}

static int ExportPlaybackFile(
    string path,
    string intervalMillisecondsText,
    string outputPath,
    CartesianRobotProfile profile,
    CartesianPosition initialPosition,
    RobotScriptParser parser)
{
    var interval = ParsePositiveMilliseconds(intervalMillisecondsText);
    var script = File.ReadAllText(path);
    var snapshot = BuildPlaybackSnapshot(script, profile, initialPosition, parser, interval);
    var json = JsonSerializer.Serialize(snapshot, CreateJsonOptions());

    File.WriteAllText(outputPath, json);

    Console.WriteLine("Playback snapshot exported.");
    Console.WriteLine($"Output: {outputPath}");
    Console.WriteLine($"Frames: {snapshot.FrameCount}");
    Console.WriteLine($"Poses: {snapshot.PoseCount}");
    Console.WriteLine($"Scene frames: {snapshot.SceneFrameCount}");
    Console.WriteLine(
        $"Viewport target: X={snapshot.Viewport.Target.XMillimeters:0.###} mm, " +
        $"Y={snapshot.Viewport.Target.YMillimeters:0.###} mm, " +
        $"Z={snapshot.Viewport.Target.ZMillimeters:0.###} mm");
    Console.WriteLine($"Total duration: {snapshot.TotalDuration.TotalSeconds:0.###} s");

    return snapshot.Succeeded ? 0 : 1;
}

static void ValidateCommandSequence(
    RobotCommandSequence commandSequence,
    CartesianRobotProfile profile)
{
    foreach (var command in commandSequence.Commands)
    {
        RobotCommandValidator.Validate(command, profile);
    }
}

static void PrintProfile(CartesianRobotProfile profile)
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

static void PrintPlayback(IReadOnlyList<RobotVisualState> frames)
{
    Console.WriteLine("Playback frames:");

    foreach (var frame in frames)
    {
        Console.WriteLine(
            $"- t={frame.Time.TotalSeconds,6:0.###}s | " +
            $"{frame.State,-9} | " +
            $"X={frame.Position.XMillimeters,7:0.###} mm " +
            $"Y={frame.Position.YMillimeters,7:0.###} mm " +
            $"Z={frame.Position.ZMillimeters,7:0.###} mm | " +
            FormatVisualCommandSource(frame));
    }
}

static void PrintWorkspaceBounds(CartesianWorkspaceBounds bounds)
{
    Console.WriteLine("Workspace bounds:");
    Console.WriteLine(
        $"- Minimum: X={bounds.Minimum.XMillimeters:0.###} mm, " +
        $"Y={bounds.Minimum.YMillimeters:0.###} mm, " +
        $"Z={bounds.Minimum.ZMillimeters:0.###} mm");
    Console.WriteLine(
        $"- Maximum: X={bounds.Maximum.XMillimeters:0.###} mm, " +
        $"Y={bounds.Maximum.YMillimeters:0.###} mm, " +
        $"Z={bounds.Maximum.ZMillimeters:0.###} mm");
    Console.WriteLine(
        $"- Size: X={bounds.Size.XMillimeters:0.###} mm, " +
        $"Y={bounds.Size.YMillimeters:0.###} mm, " +
        $"Z={bounds.Size.ZMillimeters:0.###} mm");
    Console.WriteLine(
        $"- Center: X={bounds.Center.XMillimeters:0.###} mm, " +
        $"Y={bounds.Center.YMillimeters:0.###} mm, " +
        $"Z={bounds.Center.ZMillimeters:0.###} mm");
}

static string FormatCommandSource(SimulationStep step)
{
    if (step.CommandIndex is null || step.CommandName is null)
    {
        return "simulation";
    }

    var source = step.CommandSource is null
        ? string.Empty
        : $" line {step.CommandSource.LineNumber}";

    return $"command {step.CommandIndex.Value + 1}: {step.CommandName}{source}";
}

static string FormatVisualCommandSource(RobotVisualState frame)
{
    if (frame.CommandIndex is null || frame.CommandName is null)
    {
        return "simulation";
    }

    var source = frame.CommandSource is null
        ? string.Empty
        : $" line {frame.CommandSource.LineNumber}";

    return $"command {frame.CommandIndex.Value + 1}: {frame.CommandName}{source}";
}

static void PrintSimulationResult(SimulationResult result)
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

static void PrintSnapshotResult(CartesianPlaybackSnapshot snapshot)
{
    Console.WriteLine(snapshot.Succeeded ? "Simulation completed." : "Simulation failed.");
    Console.WriteLine($"Total frames: {snapshot.FrameCount}");
    Console.WriteLine($"Total simulated time: {snapshot.TotalDuration.TotalSeconds:0.###} s");

    if (snapshot.FailureMessage is not null)
    {
        Console.WriteLine($"Failure: {snapshot.FailureMessage}");
    }
}

static CartesianPlaybackSnapshot BuildPlaybackSnapshot(
    string script,
    CartesianRobotProfile profile,
    CartesianPosition initialPosition,
    RobotScriptParser parser,
    TimeSpan interval)
{
    var context = SimulationContext.Create(profile, initialPosition);
    var commands = parser.Parse(script);
    ValidateCommandSequence(commands, profile);

    var simulator = new RobotSimulator();
    var result = simulator.Execute(context, commands);
    var snapshotBuilder = new CartesianPlaybackSnapshotBuilder();

    return snapshotBuilder.Build(profile, result, interval);
}

static JsonSerializerOptions CreateJsonOptions() =>
    new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

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

static TimeSpan ParsePositiveMilliseconds(string text)
{
    if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var milliseconds) ||
        milliseconds <= 0)
    {
        throw new InvalidOperationException("Playback interval must be a positive number of milliseconds.");
    }

    return TimeSpan.FromMilliseconds(milliseconds);
}
