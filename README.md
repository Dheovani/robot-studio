# RobotStudio

RobotStudio is a didactic robotics platform built with C# and .NET. Its first supported robot is a generic three-axis Cartesian robot, but the project is being designed to grow into a learning tool for multiple robot families, such as articulated robots and drones.

The current goal is to build a clean, testable foundation before adding hardware integration or a desktop UI.

## Current Status

- The first domain model exists for a generic Cartesian robot.
- Axis position, velocity, and acceleration limits are validated in the domain layer.
- The first linear motion planner can estimate a simple movement plan.
- The first deterministic simulator can execute `HOME`, `MOVE`, and `WAIT` command sequences.
- The simple DSL parser can read `HOME`, `MOVE`, and `WAIT` scripts.
- The CLI runs the built-in script, validates script files, simulates script files, prints a readable simulation timeline, prints fixed-interval playback frames with workspace bounds, and exports playback snapshots as JSON.
- xUnit tests cover the first domain and motion planner behaviors.
- Hardware communication and UI are planned but not implemented yet.

## Project Structure

- `src/RobotStudio.Domain`: pure domain model for general robot concepts, commands, state, contracts, domain errors, and the first Cartesian model under `RobotStudio.Domain.Cartesian`.
- `src/RobotStudio.Motion`: simple motion planning based on domain types.
- `src/RobotStudio.Simulation`: deterministic command execution and robot state simulation.
- `src/RobotStudio.Hardware`: future serial communication and hardware adapters.
- `src/RobotStudio.Scripting`: simple educational DSL parser; G-code support is planned for later.
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
- [Contributing](CONTRIBUTING.md)
- [Code of Conduct](CODE_OF_CONDUCT.md)
- [Security Policy](SECURITY.md)

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

Other CLI modes:

```bash
dotnet run --project src/RobotStudio.Cli -- example
dotnet run --project src/RobotStudio.Cli -- validate examples/cartesian.robot
dotnet run --project src/RobotStudio.Cli -- simulate examples/cartesian.robot
dotnet run --project src/RobotStudio.Cli -- playback examples/cartesian.robot 500
dotnet run --project src/RobotStudio.Cli -- export-playback examples/cartesian.robot 500 playback.json
```

## License

RobotStudio is proprietary software. Personal, non-commercial study use is
allowed under the [RobotStudio Personal Study License](LICENSE).

Commercial, business, organizational, institutional, brand-related,
redistribution, sublicensing, and public hosting uses are not allowed without
prior written permission from the copyright holder.

## Not In Scope Yet

- Desktop UI.
- 3D visualization.
- Serial communication.
- Arduino or ESP32 integration.
- G-code parser.
