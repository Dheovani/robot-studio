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

The default CLI run uses the built-in example script, executes it through the simulator, and prints the result.

Print the built-in example script:

```bash
dotnet run --project src/RobotStudio.Cli -- example
```

Validate a script file:

```bash
dotnet run --project src/RobotStudio.Cli -- validate examples/cartesian.robot
```

Simulate a script file:

```bash
dotnet run --project src/RobotStudio.Cli -- simulate examples/cartesian.robot
```

Print fixed-interval playback frames for a script file:

```bash
dotnet run --project src/RobotStudio.Cli -- playback examples/cartesian.robot 500
```

Export fixed-interval playback data as JSON:

```bash
dotnet run --project src/RobotStudio.Cli -- export-playback examples/cartesian.robot 500 playback.json
```

Current output includes:

- robot profile limits;
- axis velocity and acceleration limits;
- command sequence summary;
- simulation timeline with command source line numbers;
- fixed-interval playback frames when using the `playback` command;
- Cartesian workspace bounds when using the `playback` command;
- JSON playback snapshots when using the `export-playback` command;
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
- fixed-interval visual playback frames;
- exportable playback snapshots for future visual tools;
- command source metadata for script-generated steps;
- success/failure flag;
- failure exception when execution cannot continue.

## Current DSL

The simple DSL parser can convert text scripts into command sequences. The CLI can validate and simulate script files.

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

- richer help output;
- more examples;
- friendlier formatting for script validation errors.

## Not Available Yet

- Desktop UI.
- 3D robot visualization.
- G-code parsing.
- Hardware communication.
- Arduino or ESP32 integration.
