# RobotStudio

RobotStudio is an initial C#/.NET project for controlling and simulating a Cartesian robot. The first milestone focuses on a clean, testable core that can be used from automated tests and a CLI before any graphical UI exists.

## Project Structure

- `src/RobotStudio.Domain`: pure domain model for axes, Cartesian positions, robot profiles, validation, and robot commands.
- `src/RobotStudio.Motion`: simple motion planning based on domain types.
- `src/RobotStudio.Simulation`: future deterministic robot simulation.
- `src/RobotStudio.Hardware`: future serial communication and hardware drivers.
- `src/RobotStudio.Scripting`: future DSL or G-code subset.
- `src/RobotStudio.Cli`: terminal entry point for examples and early workflows.
- `tests`: xUnit tests for domain and motion behavior.

## Run Tests

```bash
dotnet test
```

## Run CLI

```bash
dotnet run --project src/RobotStudio.Cli
```
