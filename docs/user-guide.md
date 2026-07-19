# User Guide

## Current Requirements

- .NET SDK matching `global.json`.
- A terminal capable of running `dotnet` commands.

## Build The Project

Run from the repository root:

```bash
dotnet build
```

## Run Tests

Run from the repository root:

```bash
dotnet test
```

## Run The CLI Example

Run from the repository root:

```bash
dotnet run --project src/RobotStudio.Cli
```

The current CLI creates a generic Cartesian robot profile, creates a command sequence, executes it through the simulator, and prints the result.

Current output includes:

- robot profile limits;
- command sequence summary;
- simulation timeline;
- final robot state;
- final robot position;
- total simulated duration.

## Current Simulation Capabilities

The simulation library can already execute command sequences created in code.

Supported commands:

- `HomeCommand`
- `MoveToCommand`
- `WaitCommand`

Current simulation output includes:

- initial context;
- final context;
- timeline steps;
- success/failure flag;
- failure exception when execution cannot continue.

## Current DSL

The simple DSL parser can convert text scripts into command sequences. Script file execution from the CLI is not implemented yet.

```txt
HOME
MOVE X=10 Y=20 Z=5 SPEED=100
WAIT 500
```

Current parser behavior:

- `HOME` moves the first Cartesian robot to `(0, 0, 0)`;
- `MOVE` moves to a Cartesian position;
- `WAIT` advances simulated time without moving the robot;
- `SPEED` requests a movement speed in millimeters per second;
- physical axis limits still cap the effective movement speed;
- parser errors include the script line number.

## Planned CLI Learning Flow

The CLI should later support:

- printing the built-in example script;
- validating a script file;
- simulating a script file.

## Not Available Yet

- Desktop UI.
- 3D robot visualization.
- Script file execution.
- G-code parsing.
- Hardware communication.
- Arduino or ESP32 integration.
