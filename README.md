# RobotStudio

RobotStudio is a didactic robotics platform built with C# and .NET. Its first supported robot is a generic three-axis Cartesian robot, but the project is being designed to grow into a learning tool for multiple robot families, such as articulated robots and drones.

The current goal is to build a clean, testable foundation before adding hardware integration or a desktop UI.

## Current Status

- The first domain model exists for a generic Cartesian robot.
- Axis limits and Cartesian positions are validated in the domain layer.
- The first linear motion planner can estimate a simple movement plan.
- The first deterministic simulator can execute `HOME`, `MOVE`, and `WAIT` command sequences.
- The CLI runs a hard-coded command sequence and prints a readable simulation timeline.
- xUnit tests cover the first domain and motion planner behaviors.
- Scripting, hardware communication, and UI are planned but not implemented yet.

## Project Structure

- `src/RobotStudio.Domain`: pure domain model for robot concepts, Cartesian axes, positions, robot profiles, commands, and domain errors.
- `src/RobotStudio.Motion`: simple motion planning based on domain types.
- `src/RobotStudio.Simulation`: deterministic command execution and robot state simulation.
- `src/RobotStudio.Hardware`: future serial communication and hardware adapters.
- `src/RobotStudio.Scripting`: future simple DSL parser; G-code support is planned for later.
- `src/RobotStudio.Cli`: terminal entry point for command sequence examples and early learning workflows.
- `tests/RobotStudio.Domain.Tests`: xUnit tests for domain behavior.
- `tests/RobotStudio.Motion.Tests`: xUnit tests for motion planning behavior.
- `docs`: product, architecture, use case, testing, and user documentation.

## Documentation

- [Documentation Index](docs/README.md)
- [Technical Decisions](docs/technical-decisions.md)
- [Use Cases](docs/use-cases.md)
- [Test Map](docs/test-map.md)
- [User Guide](docs/user-guide.md)
- [Continuous Integration](docs/ci.md)

## Build

```bash
dotnet build
```

## Run Tests

```bash
dotnet test
```

## Run CLI

```bash
dotnet run --project src/RobotStudio.Cli
```

## Not In Scope Yet

- Desktop UI.
- 3D visualization.
- Serial communication.
- Arduino or ESP32 integration.
- DSL parser.
- G-code parser.
