using System.Globalization;
using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Simulation;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

var profile = RobotProfile.CreateCartesian(
    new Axis(AxisId.X, 0, 300, 120),
    new Axis(AxisId.Y, 0, 200, 100),
    new Axis(AxisId.Z, 0, 150, 80));

var initialPosition = new CartesianPosition(X: 40, Y: 30, Z: 20);
var targetPosition = new CartesianPosition(X: 120, Y: 80, Z: 40);
var context = SimulationContext.Create(profile, initialPosition);
var commands = new RobotCommandSequence(
[
    new HomeCommand(),
    new MoveToCommand(targetPosition),
    new WaitCommand(TimeSpan.FromMilliseconds(500))
]);

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

static void PrintProfile(RobotProfile profile)
{
    Console.WriteLine("Robot profile:");

    foreach (var axis in profile.Axes)
    {
        Console.WriteLine(
            $"- {axis.Id}: {axis.MinimumMillimeters:0.###} mm to {axis.MaximumMillimeters:0.###} mm, " +
            $"max {axis.MaximumVelocityMillimetersPerSecond:0.###} mm/s");
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
            step.Description);
    }
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
    MoveToCommand moveToCommand =>
        $"MOVE X={moveToCommand.TargetPosition.X:0.###} " +
        $"Y={moveToCommand.TargetPosition.Y:0.###} " +
        $"Z={moveToCommand.TargetPosition.Z:0.###}",
    WaitCommand waitCommand => $"WAIT {waitCommand.Duration.TotalMilliseconds:0.###} ms",
    _ => command.GetType().Name
};
