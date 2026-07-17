using RobotStudio.Domain;
using RobotStudio.Motion;

var profile = RobotProfile.CreateCartesian(
    new Axis(AxisId.X, 0, 300, 120),
    new Axis(AxisId.Y, 0, 200, 100),
    new Axis(AxisId.Z, 0, 150, 80));

var start = new CartesianPosition(X: 0, Y: 0, Z: 0);
var end = new CartesianPosition(X: 120, Y: 80, Z: 40);

var planner = new MotionPlanner();
var plan = planner.PlanLinearMove(start, end, profile);

Console.WriteLine("RobotStudio - linear motion plan");
Console.WriteLine($"Start: X={plan.Start.X:0.###} mm, Y={plan.Start.Y:0.###} mm, Z={plan.Start.Z:0.###} mm");
Console.WriteLine($"End:   X={plan.End.X:0.###} mm, Y={plan.End.Y:0.###} mm, Z={plan.End.Z:0.###} mm");
Console.WriteLine($"Segments: {plan.Segments.Count}");
Console.WriteLine($"Total duration: {plan.TotalDuration.TotalSeconds:0.###} s");

foreach (var segment in plan.Segments)
{
    Console.WriteLine(
        $"- Linear segment at {segment.VelocityMillimetersPerSecond:0.###} mm/s " +
        $"for {segment.Duration.TotalSeconds:0.###} s");
}
