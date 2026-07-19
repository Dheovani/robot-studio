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

The current CLI creates a generic Cartesian robot profile, creates a start position, creates a target position, generates a linear motion plan, and prints the result.

Current output includes:

- start position;
- end position;
- number of motion segments;
- total estimated duration;
- segment velocity and duration.

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

## Planned DSL

The simple DSL is not implemented yet. The planned introductory syntax is:

```txt
HOME
MOVE X=10 Y=20 Z=5 SPEED=100
WAIT 500
```

Planned behavior:

- `HOME` moves the first Cartesian robot to `(0, 0, 0)`;
- `MOVE` moves to a Cartesian position;
- `WAIT` advances simulated time without moving the robot;
- parser and validation errors should be written for beginners.

## Planned CLI Learning Flow

The CLI should later support:

- running a built-in Cartesian example;
- printing the built-in example script;
- validating a script file;
- simulating a script file;
- printing final robot state;
- printing final robot position;
- printing total simulated duration.

## Not Available Yet

- Desktop UI.
- 3D robot visualization.
- Script file execution.
- G-code parsing.
- Hardware communication.
- Arduino or ESP32 integration.
