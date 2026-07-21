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

## Run The Desktop Viewer

Run from the repository root on Windows:

```bash
dotnet run --project src/RobotStudio.Desktop
```

The first desktop viewer opens a WPF window with a 3D viewport, renders the built-in Cartesian simulation, and provides basic playback controls.

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

Validate exported playback data:

```bash
dotnet run --project src/RobotStudio.Cli -- validate-playback playback.json
```

Current output includes:

- robot profile limits;
- axis velocity and acceleration limits;
- command sequence summary;
- simulation timeline with command source line numbers;
- fixed-interval playback frames when using the `playback` command;
- Cartesian workspace bounds when using the `playback` command;
- JSON playback snapshots when using the `export-playback` command;
- playback snapshot validation when using the `validate-playback` command;
- Cartesian robot poses in exported playback snapshots;
- Cartesian scene frames with renderable primitives in exported playback snapshots;
- Cartesian viewport data for initial 3D camera framing in exported playback snapshots;
- versioned playback metadata in exported playback snapshots;
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
- didactic Cartesian mechanism poses derived from playback frames;
- renderable Cartesian scene primitives for future 3D tools;
- deterministic initial viewport data for future 3D tools;
- versioned snapshot metadata for future UI compatibility checks;
- snapshot validation for future UI compatibility checks;
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

- Loading custom scripts in the desktop viewer.
- Rich camera controls in the desktop viewer.
- G-code parsing.
- Hardware communication.
- Arduino or ESP32 integration.
