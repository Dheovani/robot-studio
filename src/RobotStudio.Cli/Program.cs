using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using RobotStudio.Cli;
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

try
{
    var commandLine = CliCommandLine.Parse(args);

    return commandLine.Arguments switch
    {
        [] => SimulateExample(profile, initialPosition, commandLine.DialectName),
        ["example"] => PrintExampleScript(commandLine.DialectName),
        ["validate", var path] => ValidateScriptFile(
            path,
            profile,
            initialPosition,
            ResolveDialect(commandLine, path)),
        ["simulate", var path] => SimulateScriptFile(
            path,
            profile,
            initialPosition,
            ResolveDialect(commandLine, path)),
        ["playback", var path, var intervalMilliseconds] => PrintPlaybackFile(
            path,
            intervalMilliseconds,
            profile,
            initialPosition,
            ResolveDialect(commandLine, path)),
        ["export-playback", var path, var intervalMilliseconds, var outputPath] => ExportPlaybackFile(
            path,
            intervalMilliseconds,
            outputPath,
            profile,
            initialPosition,
            ResolveDialect(commandLine, path)),
        ["validate-playback", var path] => ValidatePlaybackFile(
            path,
            commandLine.DialectName),
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
catch (JsonException exception)
{
    Console.Error.WriteLine($"Snapshot JSON error: {exception.Message}");
    return 1;
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine($"Validation error: {exception.Message}");
    return 1;
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine($"Argument error: {exception.Message}");
    return 1;
}

static int SimulateExample(
    CartesianRobotProfile profile,
    CartesianPosition initialPosition,
    string? dialectName)
{
    var dialect = RobotScriptDialectResolver.Resolve(dialectName);
    var script = GetExampleScript(dialect);

    return SimulateScript(script, profile, initialPosition, dialect);
}

static int PrintExampleScript(string? dialectName)
{
    var dialect = RobotScriptDialectResolver.Resolve(dialectName);
    Console.WriteLine(GetExampleScript(dialect));
    return 0;
}

static string GetExampleScript(IRobotScriptDialect dialect) =>
    dialect.Descriptor.Id == RobotScriptDialectId.GCode
        ? GCodeWriter.Write(new RobotScriptParser().Parse(ExampleScript))
        : ExampleScript;

static IRobotScriptDialect ResolveDialect(
    CliCommandLine commandLine,
    string scriptPath) =>
    RobotScriptDialectResolver.Resolve(commandLine.DialectName, scriptPath);

static int ValidateScriptFile(
    string path,
    CartesianRobotProfile profile,
    CartesianPosition initialPosition,
    IRobotScriptDialect scriptDialect)
{
    var script = File.ReadAllText(path);
    var commands = ParseScript(script, scriptDialect, initialPosition);
    ValidateCommandSequence(commands, profile);

    Console.WriteLine("Script is valid.");
    Console.WriteLine($"Dialect: {scriptDialect.Descriptor.Name}");
    Console.WriteLine();
    PrintCommandSequence(commands);

    return 0;
}

static int SimulateScriptFile(
    string path,
    CartesianRobotProfile profile,
    CartesianPosition initialPosition,
    IRobotScriptDialect scriptDialect)
{
    var script = File.ReadAllText(path);

    return SimulateScript(script, profile, initialPosition, scriptDialect);
}

static int SimulateScript(
    string script,
    CartesianRobotProfile profile,
    CartesianPosition initialPosition,
    IRobotScriptDialect scriptDialect)
{
    var context = SimulationContext.Create(profile, initialPosition);
    var commands = ParseScript(script, scriptDialect, initialPosition);
    ValidateCommandSequence(commands, profile);

    var simulator = new RobotSimulator();
    var result = simulator.Execute(context, commands);

    Console.WriteLine("RobotStudio CLI");
    Console.WriteLine($"Script dialect: {scriptDialect.Descriptor.Name}");
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
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- example [--dialect dsl|gcode]");
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- validate <script-file> [--dialect dsl|gcode]");
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- simulate <script-file> [--dialect dsl|gcode]");
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- playback <script-file> <interval-ms> [--dialect dsl|gcode]");
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- export-playback <script-file> <interval-ms> <output-json> [--dialect dsl|gcode]");
    Console.WriteLine("  dotnet run --project src/RobotStudio.Cli -- validate-playback <snapshot-json>");
    Console.WriteLine();
    Console.WriteLine("Script dialect is inferred from .robot or .gcode files. Use --dialect to override it.");

    return 1;
}

static int PrintPlaybackFile(
    string path,
    string intervalMillisecondsText,
    CartesianRobotProfile profile,
    CartesianPosition initialPosition,
    IRobotScriptDialect scriptDialect)
{
    var interval = ParsePositiveMilliseconds(intervalMillisecondsText);
    var script = File.ReadAllText(path);
    var snapshot = BuildPlaybackSnapshot(script, profile, initialPosition, scriptDialect, interval);

    Console.WriteLine("RobotStudio CLI");
    Console.WriteLine($"Script dialect: {scriptDialect.Descriptor.Name}");
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
    IRobotScriptDialect scriptDialect)
{
    var interval = ParsePositiveMilliseconds(intervalMillisecondsText);
    var script = File.ReadAllText(path);
    var snapshot = BuildPlaybackSnapshot(script, profile, initialPosition, scriptDialect, interval);
    var json = JsonSerializer.Serialize(snapshot, CreateJsonOptions());

    File.WriteAllText(outputPath, json);

    Console.WriteLine("Playback snapshot exported.");
    Console.WriteLine($"Output: {outputPath}");
    Console.WriteLine($"Format version: {snapshot.Metadata.FormatVersion}");
    Console.WriteLine($"Robot family: {snapshot.Metadata.RobotFamily}");
    Console.WriteLine($"Distance unit: {snapshot.Metadata.DistanceUnit}");
    Console.WriteLine($"Sample interval: {snapshot.Metadata.SampleIntervalMilliseconds:0.###} ms");
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

static int ValidatePlaybackFile(
    string path,
    string? dialectName)
{
    if (dialectName is not null)
    {
        throw new ArgumentException("The --dialect option cannot be used with validate-playback.");
    }

    var json = File.ReadAllText(path);
    var snapshot = JsonSerializer.Deserialize<CartesianPlaybackSnapshot>(json, CreateJsonOptions());
    var validator = new PlaybackSnapshotValidator();
    var result = validator.Validate(snapshot);

    if (result.IsValid)
    {
        Console.WriteLine("Playback snapshot is valid.");
        Console.WriteLine($"Format version: {snapshot!.Metadata.FormatVersion}");
        Console.WriteLine($"Robot family: {snapshot.Metadata.RobotFamily}");
        Console.WriteLine($"Frames: {snapshot.FrameCount}");
        Console.WriteLine($"Scene frames: {snapshot.SceneFrameCount}");

        return 0;
    }

    Console.Error.WriteLine("Playback snapshot is invalid.");

    foreach (var error in result.Errors)
    {
        Console.Error.WriteLine($"- {error}");
    }

    return 1;
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
    IRobotScriptDialect scriptDialect,
    TimeSpan interval)
{
    var context = SimulationContext.Create(profile, initialPosition);
    var commands = ParseScript(script, scriptDialect, initialPosition);
    ValidateCommandSequence(commands, profile);

    var simulator = new RobotSimulator();
    var result = simulator.Execute(context, commands);
    var snapshotBuilder = new CartesianPlaybackSnapshotBuilder();

    return snapshotBuilder.Build(profile, result, interval);
}

static RobotCommandSequence ParseScript(
    string script,
    IRobotScriptDialect scriptDialect,
    CartesianPosition initialPosition) =>
    scriptDialect.Parse(script, new RobotScriptParseContext(initialPosition));

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
